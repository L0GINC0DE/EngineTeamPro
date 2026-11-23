// DLJ: bake pixelfx pixels into the existing Aseprite files, preserving their body layers.
// From the repository root: node output/DLJ/HeadUIParticles/build.mjs [pixelfx checkout]
import fs from 'node:fs';
import path from 'node:path';
import {pathToFileURL} from 'node:url';
import {inflateSync,deflateSync} from 'node:zlib';
const root=process.cwd(),out=path.join(root,'output/DLJ/HeadUIParticles');
const fxRoot=path.resolve(process.argv[2]||'.utmp/DLJ_WhiteWater/pixelfx');
const {createFrame,painterFor,composite,cycle,fit}=await import(pathToFileURL(path.join(fxRoot,'src/core/index.js')));
const {encodePNG}=await import(pathToFileURL(path.join(fxRoot,'tools/lib/png.mjs')));
const styles=[
 ['Attack','Red',1,['#751829','#b6293e','#ef4858','#ff8b91']],
 ['Setting','Gray',2,['#535b59','#858d89','#bec5bf','#edf1e9']],
 ['Heal','Green',3,['#2c6b19','#4dac25','#83e83d','#c3ff87']],
 ['Act','Orange',4,['#99420c','#e77612','#ffa42c','#ffdb80']],
 ['HealthBar','Yellow',5,['#a56c10','#dfaa19','#ffd83b','#fff095']],
 ['Defend','Blue',6,['#123b9d','#215ee1','#4b8bff','#9dcaff']],
 ['TextBar','White',7,['#747d8d','#abb5c7','#e1e7ef','#ffffff']],
];
const LAYER='DLJ_Head_Particles',W=64,H=77,ZOOM=3;
function chunk(type,data){const b=Buffer.alloc(data.length+6);b.writeUInt32LE(b.length);b.writeUInt16LE(type,4);data.copy(b,6);return b;}
function layer(){const name=Buffer.from(LAYER),d=Buffer.alloc(18+name.length);d.writeUInt16LE(3);d[12]=255;d.writeUInt16LE(name.length,16);name.copy(d,18);return chunk(0x2004,d);}
function cel(rgba,index){const d=Buffer.alloc(20);d.writeUInt16LE(index);d[6]=255;d.writeUInt16LE(2,7);d.writeUInt16LE(W,16);d.writeUInt16LE(H,18);return chunk(0x2005,Buffer.concat([d,deflateSync(rgba)]));}
function parse(bytes){
 if(bytes.readUInt16LE(4)!==0xa5e0||bytes.readUInt16LE(8)!==W||bytes.readUInt16LE(10)!==H||bytes.readUInt16LE(12)!==32)throw Error('Unexpected source canvas');
 const frames=[];let offset=128,layerIndex=-1,fxIndex=-1;
 for(let f=0;f<bytes.readUInt16LE(6);f++){
  const end=offset+bytes.readUInt32LE(offset),chunks=[];
  for(let p=offset+16;p<end;){const size=bytes.readUInt32LE(p),type=bytes.readUInt16LE(p+4),data=bytes.subarray(p+6,p+size);let keep=true;
   if(type===0x2004){layerIndex++;const name=data.subarray(18,18+data.readUInt16LE(16)).toString();if(name===LAYER){fxIndex=layerIndex;keep=false;}}
   if(type===0x2005&&data.readUInt16LE(0)===fxIndex)keep=false;
   if(keep)chunks.push(Buffer.from(bytes.subarray(p,p+size)));p+=size;
  }
  frames.push({header:Buffer.from(bytes.subarray(offset,offset+16)),chunks,duration:bytes.readUInt16LE(offset+8)});offset=end;
 }
 if(fxIndex>=0&&fxIndex!==layerIndex)throw Error('Particle layer must be the final layer');
 return {header:Buffer.from(bytes.subarray(0,128)),frames,layers:layerIndex+1-(fxIndex>=0?1:0)};
}
function over(base,top){const rgba=Buffer.from(base);for(let p=0;p<rgba.length;p+=4){const sa=top[p+3]/255,da=rgba[p+3]/255,a=sa+da*(1-sa);if(a)for(let c=0;c<3;c++)rgba[p+c]=Math.round((top[p+c]*sa+rgba[p+c]*da*(1-sa))/a);rgba[p+3]=Math.round(a*255);}return rgba;}
function decode(frames){
 const result=[],cels=[];
 for(const frame of frames){let rgba=Buffer.alloc(W*H*4);const current=[];
  for(const c of frame.chunks){if(c.readUInt16LE(4)!==0x2005)continue;const d=c.subarray(6),index=d.readUInt16LE(0),kind=d.readUInt16LE(7);let pixels;
   if(kind===1)pixels=cels[d.readUInt16LE(16)][index];
   else if(kind===0||kind===2){const w=d.readUInt16LE(16),h=d.readUInt16LE(18),raw=kind===2?inflateSync(d.subarray(20)):d.subarray(20),x=d.readInt16LE(2),y=d.readInt16LE(4);pixels=Buffer.alloc(W*H*4);for(let j=0;j<h;j++)raw.copy(pixels,((y+j)*W+x)*4,j*w*4,(j+1)*w*4);}
   else throw Error('Unsupported cel type');
   current[index]=pixels;rgba=over(rgba,pixels);
  }cels.push(current);result.push(rgba);
 }return result;
}
function isEquipment(r,g,b,a,y,index){
 if(!a||y>=28)return false;
 if(index===1)return r>70&&r>g*1.5&&r>b*1.25;
 if(index===2)return r>65&&Math.max(r,g,b)-Math.min(r,g,b)<30;
 if(index===3)return g>85&&g>r*1.35&&g>b*1.5;
 if(index===4)return r>110&&g>40&&r>g*1.15&&g>b*1.6;
 if(index===5)return r>130&&g>110&&b<110;
 if(index===6)return b>110&&b>r*1.5&&b>g*1.15;
 return r>180&&g>180&&b>180;
}
function draw(p,t,o){
 const C=fit(o.palette,4);
 cycle(t,o.duration,o.count,o.seed,r=>r.range(.55,.9),(age,life,r,i)=>{
  const [sx,sy]=o.source[Math.floor(r.next()*o.source.length)],q=age/life;
  const x=sx+r.range(-2.4,2.4)*age+Math.sin(age*4+i)*.45;
  const y=sy-Math.min(r.range(6,10),Math.max(0,sy-1)/life)*age;
  const alpha=Math.min(1,age/.08)*Math.min(1,(1-q)/.55);
  p.blend(x,y,q<.22?C[3]:q<.65?C[2]:C[1],alpha);
  if(i%7===0&&q<.45)p.blend(x,y+1,C[1],alpha*.32);
 });
}
const contact=Buffer.alloc(W*7*ZOOM*H*8*ZOOM*4),manifest=[];
const backup=path.join(root,'.utmp/DLJ_BakedHeadParticles/originals');fs.mkdirSync(backup,{recursive:true});
for(const [label,color,index,palette]of styles){
 const name=`Player_${String(index).padStart(2,'0')}_${color}_DLJ_Idle.aseprite`,file=path.join(root,'Assets/02.Sprites/DLJ/Player',name);
 const bytes=fs.readFileSync(file),source=parse(bytes),originals=decode(source.frames),duration=source.frames.reduce((sum,f)=>sum+f.duration,0)/1000;
 if(source.frames.length!==8||source.layers!==4)throw Error('Unexpected original animation structure: '+name);
 if(!fs.existsSync(path.join(backup,name))){fs.writeFileSync(path.join(backup,name),bytes);fs.copyFileSync(file+'.meta',path.join(backup,name+'.meta'));}
 let time=0;const packed=[],views=[];
 for(let f=0;f<source.frames.length;f++){
  const mask=[];for(let y=0;y<H;y++)for(let x=0;x<W;x++){const p=(y*W+x)*4;if(isEquipment(...originals[f].subarray(p,p+4),y,index))mask.push([x,y]);}
  const has=new Set(mask.map(([x,y])=>`${x},${y}`)),edge=mask.filter(([x,y])=>!has.has(`${x-1},${y}`)||!has.has(`${x+1},${y}`)||!has.has(`${x},${y-1}`)||!has.has(`${x},${y+1}`));
  if(edge.length<5)throw Error('Equipment mask missing');
  const opts={palette,source:edge,seed:317+index*41,count:Math.min(54,Math.max(36,Math.round(edge.length*.65))),duration};
  const frame=createFrame(W,H);draw(painterFor(frame),time,opts);const fx=Buffer.alloc(W*H*4);composite(frame,fx);
  const combined=over(originals[f],fx);if(!combined.subarray(28*W*4).equals(originals[f].subarray(28*W*4)))throw Error('Pixels below head changed');
  views.push(combined);const chunks=[...source.frames[f].chunks];
  if(f===0){const firstCel=chunks.findIndex(c=>c.readUInt16LE(4)===0x2005);chunks.splice(firstCel,0,layer());}
  chunks.push(cel(fx,source.layers));const header=Buffer.from(source.frames[f].header),data=Buffer.concat(chunks);header.writeUInt32LE(data.length+16);header.writeUInt16LE(chunks.length,6);header.writeUInt32LE(chunks.length,12);packed.push(Buffer.concat([header,data]));time+=source.frames[f].duration/1000;
 }
 const baked=Buffer.concat([source.header,...packed]);baked.writeUInt32LE(baked.length);
 const check=parse(baked);for(let f=0;f<source.frames.length;f++)if(!Buffer.concat(source.frames[f].chunks).equals(Buffer.concat(check.frames[f].chunks)))throw Error('Original layer chunks changed');
 fs.writeFileSync(file,baked);
 for(let f=0;f<8;f++)for(let y=0;y<H*ZOOM;y++)for(let x=0;x<W*ZOOM;x++){
  const s=(Math.floor(y/ZOOM)*W+Math.floor(x/ZOOM))*4,t=((f*H*ZOOM+y)*W*7*ZOOM+(index-1)*W*ZOOM+x)*4,a=views[f][s+3]/255;
  for(let c=0;c<3;c++)contact[t+c]=views[f][s+c]*a+[24,21,30][c]*(1-a);contact[t+3]=255;
 }
 manifest.push({label,color,file:path.relative(root,file).replaceAll('\\','/'),canvas:[W,H],frames:8,durations:source.frames.map(f=>f.duration),particleLayer:LAYER,originalChunksPreserved:true});
}
fs.writeFileSync(path.join(out,'preview.png'),encodePNG(W*7*ZOOM,H*8*ZOOM,contact));
fs.writeFileSync(path.join(out,'manifest.json'),JSON.stringify(manifest,null,2));
console.log('DLJ_BAKED_ASSETS_PASS: seven original files updated; original body chunks, 64x77 canvas and 8x160ms timing preserved');
