const fs=require('node:fs');
const path=require('node:path');
const vm=require('node:vm');
const assert=require('node:assert/strict');
const elements=new Map();
function element(id){
  if(!elements.has(id))elements.set(id,{value:({angle:'53.13',length:'300',facing:'1'})[id]||'',textContent:'',innerHTML:'',attrs:{},listeners:{},setAttribute(k,v){this.attrs[k]=v;},addEventListener(k,fn){this.listeners[k]=fn;}});
  return elements.get(id);
}
const buttons=[0,90,180,-90].map(angle=>({dataset:{angle:String(angle)},listeners:{},addEventListener(k,fn){this.listeners[k]=fn;}}));
const context=vm.createContext({window:{print(){}},document:{getElementById:element,querySelectorAll:()=>buttons},console});
vm.runInContext(fs.readFileSync(path.join(__dirname,'math-diagram.js'),'utf8'),context);
const {calculate}=context.window.chainMathDiagram;
const gameSource=fs.readFileSync(path.join(__dirname,'../sanabi-chain-arm.html'),'utf8');
const helper=gameSource.match(/function getGrappleFrame\(\) \{[\s\S]+?\n        \}/)[0];
const game=vm.createContext({player:{rotation:0},grapple:{anchorX:100,anchorY:100}});
vm.runInContext(helper,game);
let cases=0;
for(const angle of [-180,-135,-90,-45,0,45,53.13,90,135,180])for(const length of [100,300,480])for(const facing of [-1,1]){
  const s=calculate(angle,length,facing);
  assert(Math.abs(s.distance-length)<1e-9);
  assert(Math.abs(s.sin*s.sin+s.cos*s.cos-1)<1e-12);
  assert(Math.abs(s.dx*s.sin+s.dy*s.cos)<1e-12);
  assert(Math.abs(Math.hypot(s.vx,s.vy)-1000)<1e-9);
  game.player.x=s.px;game.player.y=s.py;
  const frame=vm.runInContext('getGrappleFrame()',game);
  assert(Math.abs(s.dx-frame.rightX*facing)<1e-12);
  assert(Math.abs(s.dy-frame.rightY*facing)<1e-12);
  assert(Math.abs(Math.atan2(Math.sin(s.rotation-frame.rotation),Math.cos(s.rotation-frame.rotation)))<1e-12);
  element('angle').value=String(angle);element('length').value=String(length);element('facing').value=String(facing);
  element('angle').listeners.input();
  assert(!/NaN|Infinity/.test(element('chainStage').innerHTML));
  assert(element('liveMath').innerHTML.includes('px/s'));
  cases++;
}
const example=calculate(Math.atan2(240,180)*180/Math.PI,300,1);
assert(Math.abs(example.px-340)<1e-10);assert(Math.abs(example.py-280)<1e-10);
assert(Math.abs(example.vx-600)<1e-9);assert(Math.abs(example.vy+800)<1e-9);
buttons[1].listeners.click();assert.equal(element('angle').value,'90');
element('length').value='300';element('facing').value='-1';element('facing').listeners.input();
assert(element('chainStage').innerHTML.includes('A · 몸체 왼쪽'));
const html=fs.readFileSync(path.join(__dirname,'math-game-production-plan.html'),'utf8');
assert(!/<(?:script|link)[^>]+(?:src|href)=["']https?:/i.test(html));
assert(!/NaN|undefined/.test(html));
for(const id of [...html.matchAll(/href="#([^"]+)"/g)].map(m=>m[1]))assert(html.includes('id="'+id+'"'));
new vm.Script(html.match(/<script>([\s\S]*?)<\/script>/)[1]);
fs.mkdirSync(path.join(__dirname,'checks'),{recursive:true});
fs.writeFileSync(path.join(__dirname,'checks/verification.json'),JSON.stringify({date:'2026-10-27',mathAndGameDirectionCases:cases,workedExample:true,inputHandlers:true,offlineDependencies:true,documentLinks:true,scriptSyntax:true,browserVisuallyTested:false},null,2));
console.log(`PASS ${cases} angle/length/facing cases match the actual game frame. PASS distance, trig identity, tangency, speed, worked example, controls, offline dependencies, anchors and script syntax.`);
if(process.argv.includes('--render')){
  const {createCanvas,loadImage}=require('C:/Users/TAEHYEON/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas');
  (async()=>{
    const out=createCanvas(1440,600),ctx=out.getContext('2d');
    for(const [i,facing]of[1,-1].entries()){
      element('angle').value='53.13';element('length').value='300';element('facing').value=String(facing);context.window.chainMathDiagram.render();
      const img=await loadImage(Buffer.from(element('chainStage').innerHTML));ctx.drawImage(img,i*720,0);
    }
    fs.writeFileSync(path.join(__dirname,'checks/math-diagram-preview.png'),out.toBuffer('image/png'));
    console.log('Rendered both facing directions from the document SVG.');
  })().catch(error=>{console.error(error);process.exitCode=1;});
}
