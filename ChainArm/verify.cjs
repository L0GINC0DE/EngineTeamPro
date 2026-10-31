const fs = require('node:fs');
const vm = require('node:vm');
const assert = require('node:assert/strict');
const html = fs.readFileSync(__dirname + '/sanabi-chain-arm.html', 'utf8');
const source = html.match(/<script>([\s\S]*?)<\/script>/)[1];
function createGame(renderCanvas = null) {
  const listeners = {};
  class Element {
    constructor() { this.style = {}; this.dataset = {}; this.textContent = ''; this.value = ''; this.checked = false; this.classList = { add(){}, remove(){}, toggle(){} }; }
    addEventListener() {} matches() { return false; } blur() {}
    getContext() { return renderCanvas ? renderCanvas.getContext('2d') : new Proxy({}, { get: (_, k) => k === 'createLinearGradient' ? () => ({addColorStop(){}}) : k === 'measureText' ? () => ({width:100}) : () => {} }); }
  }
  const elements = new Map();
  const document = {getElementById(id){if(!elements.has(id))elements.set(id,new Element());return elements.get(id);},querySelectorAll(){return [];},addEventListener(){},hidden:false};
  const window = { innerWidth:1280, innerHeight:800, addEventListener(k,v){(listeners[k]??=[]).push(v);} };
  const context = vm.createContext({window,document,HTMLElement:Element,console,performance:{now:()=>0},setTimeout:()=>0,clearTimeout(){},requestAnimationFrame(){},Math});
  vm.runInContext(source, context);
  const run = text => vm.runInContext(text, context);
  function key(code, down = true) { for(const f of listeners[down?'keydown':'keyup']||[]) f({code,repeat:false,target:new Element(),preventDefault(){}}); }
  function event(name, payload) { for(const f of listeners[name]||[]) f(payload); }
  return {run,key,event,context};
}
const tests = [];
function test(name, fn) { fn(); tests.push(name); console.log('PASS', name); }
test('Standalone offline HTML has no external script or stylesheet dependency',()=>assert(!/(?:src|href)=["']https?:/i.test(html)));
test('A/D dash stays attached and tangent to its original anchor; preview equals velocity',()=>{
  const g=createGame(); let cases=0;
  for(const axis of [-1,1]) for(const rx of [-200,0,200]) for(const ry of [-200,-0.01,0.01,200]) for(const vy of [-500,0,500]) {
    g.run(`Object.assign(player,{x:3000,y:-2000,vx:400,vy:${vy},state:'GRAPPLING',swingDashReady:true});Object.assign(grapple,{active:true,mode:'WALL',anchorX:${3000-rx},anchorY:${-2000-ry},length:400});keys.a=${axis<0};keys.d=${axis>0};`);
    const expected=g.run('getDashDirection()');
    g.run('executeSwingDash()');
    const actual=g.run('({vx:player.vx,vy:player.vy,active:grapple.active,ready:player.swingDashReady,state:player.state,anchorX:grapple.anchorX,anchorY:grapple.anchorY,length:grapple.length})');
    const speed=Math.hypot(actual.vx,actual.vy);
    assert(Math.abs(actual.vx/speed-expected.x)<1e-10);
    assert(Math.abs(actual.vy/speed-expected.y)<1e-10);
    assert(Math.abs(actual.vx/speed-axis*ry/Math.hypot(rx,ry))<1e-10);
    assert(Math.abs(actual.vy/speed+axis*rx/Math.hypot(rx,ry))<1e-10);
    assert(g.run('Math.abs(Math.cos(player.rotation)*player.visualFacing-player.vx/Math.hypot(player.vx,player.vy))')<1e-10);
    assert(g.run('Math.abs(Math.sin(player.rotation)*player.visualFacing-player.vy/Math.hypot(player.vx,player.vy))')<1e-10);
    assert(Math.abs((actual.vx*rx+actual.vy*ry)/speed)<1e-8);
    assert.equal(actual.active,true); assert.equal(actual.ready,false); assert.equal(actual.state,'GRAPPLING');
    assert.equal(actual.anchorX,3000-rx);assert.equal(actual.anchorY,-2000-ry);assert.equal(actual.length,400);
    cases++;
  }
  console.log(' ', cases, 'direction cases');
});
test('Neutral dash keeps the chosen local facing regardless of velocity and mouse aim',()=>{
  const g=createGame();
  g.run('Object.assign(player,{x:100,y:100,vx:300,vy:-400});Object.assign(grapple,{active:true,anchorX:0,anchorY:0})');
  let d=g.run('getDashDirection()');assert(Math.abs(d.x-Math.SQRT1_2)<1e-10);assert(Math.abs(d.y+Math.SQRT1_2)<1e-10);
  g.run('player.vx=player.vy=0;mouse.screenX=player.x-camera.x+canvas.width/2-100;mouse.screenY=player.y-camera.y+canvas.height/2+100');
  d=g.run('getDashDirection()');assert(d.x>0&&d.y<0);
  g.run('mouse.screenX=player.x-camera.x+canvas.width/2;mouse.screenY=player.y-camera.y+canvas.height/2;player.facing=-1');
  d=g.run('getDashDirection()');assert(d.x<0&&d.y>0);assert(Math.abs(Math.hypot(d.x,d.y)-1)<1e-10);
});
test('Last pressed A/D wins; releasing one alias preserves the other held key',()=>{
  const g=createGame();g.key('KeyA');g.key('KeyD');assert.equal(g.run('getMoveAxis()'),1);
  g.key('KeyD',false);assert.equal(g.run('getMoveAxis()'),-1);
  g.key('ArrowLeft');g.key('KeyA',false);assert.equal(g.run('getMoveAxis()'),-1);
  g.key('ArrowLeft',false);assert.equal(g.run('getMoveAxis()'),0);
});
test('Swing steering is continuous across anchor height',()=>{
  const results=[];
  for(const side of [-.01,.01]) {
    const g=createGame();g.run(`params.gravity=0;params.airDrag=1;params.chainBounce=0;Object.assign(player,{x:3200,y:-2000+${side},vx:0,vy:0,state:'GRAPPLING'});Object.assign(grapple,{active:true,mode:'WALL',anchorX:3000,anchorY:-2000,length:200});keys.d=true;updatePhysics(1/240);`);
    results.push(g.run('({vx:player.vx,vy:player.vy})'));
  }
  assert(Math.abs(results[0].vy-results[1].vy)<.01);assert(Math.abs(results[0].vx-results[1].vx)<.01);
});
test('Opposite A/D reverses tangential motion within 100ms at multiple angles and speeds',()=>{
  let longest=0;let cases=0;
  for(const angle of [-2.6,-1.57,-.8,0,.8,1.57,2.6]) for(const speed of [300,720,1400,1850]) for(const axis of [-1,1]) {
    const g=createGame(),r=400,rx=Math.sin(angle)*r,ry=Math.cos(angle)*r;
    const sign=axis;
    g.run(`Object.assign(player,{x:${3000+rx},y:${-2300+ry},vx:${Math.cos(angle)*speed*sign},vy:${-Math.sin(angle)*speed*sign},state:'GRAPPLING'});Object.assign(grapple,{active:true,mode:'WALL',anchorX:3000,anchorY:-2300,length:400});`);
    g.key(axis>0?'KeyD':'KeyA');g.run('updatePhysics(1/240)');g.key(axis>0?'KeyA':'KeyD');
    let step=0,along=0;
    do {
      g.run('updatePhysics(1/240)');step++;
      along=g.run('((player.y-grapple.anchorY)*player.vx-(player.x-grapple.anchorX)*player.vy)/Math.hypot(player.x-grapple.anchorX,player.y-grapple.anchorY)');
    } while(along*sign>=-100 && step<24);
    assert(along*sign < -100,`angle ${angle}, speed ${speed}, input ${axis}, tangent ${along}`);
    assert.equal(g.run('grapple.active'),true);
    assert(g.run('Math.hypot(player.x-grapple.anchorX,player.y-grapple.anchorY)')<=400.001);
    longest=Math.max(longest,step/240);cases++;
  }
  console.log(' ',cases,'reversals; slowest',Math.round(longest*1000),'ms');
});
test('A/D starts a stationary taut swing promptly',()=>{
  const g=createGame();g.run("params.gravity=0;Object.assign(player,{x:3000,y:-1900,vx:0,vy:0,state:'GRAPPLING'});Object.assign(grapple,{active:true,mode:'WALL',anchorX:3000,anchorY:-2300,length:400})");
  g.key('KeyD');g.run('for(let i=0;i<30;i++)updatePhysics(1/240)');assert(g.run('player.vx')>550);
});
test('Releasing A/D preserves natural swing momentum',()=>{
  const g=createGame();g.run("params.gravity=0;params.chainBounce=0;Object.assign(player,{x:3000,y:-1900,vx:800,vy:0,state:'GRAPPLING'});Object.assign(grapple,{active:true,mode:'WALL',anchorX:3000,anchorY:-2300,length:400})");
  g.key('KeyD');g.run('updatePhysics(1/240)');g.key('KeyD',false);
  g.run('for(let i=0;i<24;i++)updatePhysics(1/240)');assert(g.run('Math.hypot(player.vx,player.vy)')>760);
});
test('Held input does not automatically reverse rotation when crossing anchor height',()=>{
  const g=createGame();g.run("params.gravity=0;params.chainBounce=0;Object.assign(player,{x:3399,y:-2290,vx:0,vy:-300,state:'GRAPPLING'});Object.assign(grapple,{active:true,mode:'WALL',anchorX:3000,anchorY:-2300,length:400})");
  g.key('KeyD');g.run('for(let i=0;i<48;i++)updatePhysics(1/240)');
  assert(g.run('player.y')< -2300);assert(g.run('player.vy')< -500);
});
test('Opposite A/D also brakes a slack chain promptly',()=>{
  const g=createGame();g.run("Object.assign(player,{x:3000,y:-2100,vx:800,vy:0,state:'GRAPPLING'});Object.assign(grapple,{active:true,mode:'WALL',anchorX:3000,anchorY:-2300,length:480})");
  g.key('KeyA');g.run('for(let i=0;i<20;i++)updatePhysics(1/240)');assert(g.run('player.vx')< -200);
});
test('Fresh opposite input can reverse a chain dash without the old input lock',()=>{
  const g=createGame();g.run("Object.assign(player,{x:3000,y:-1900,vx:800,vy:0,state:'GRAPPLING'});Object.assign(grapple,{active:true,mode:'WALL',anchorX:3000,anchorY:-2300,length:400})");
  g.key('KeyD');g.key('ShiftLeft');g.key('KeyA');g.run('for(let i=0;i<24;i++)updatePhysics(1/240)');
  assert(g.run('player.vx')< -100);assert.equal(g.run('grapple.active'),true);
});
test('Ground input stops and reverses immediately, with zero released-key displacement',()=>{
  const g=createGame();g.run("Object.assign(player,{x:300,y:884,vx:440,vy:0,state:'GROUNDED',onGround:true})");
  g.key('KeyD');g.key('KeyD',false);assert.equal(g.run('player.vx'),0);
  g.run('for(let i=0;i<120;i++)updatePhysics(1/240)');assert.equal(g.run('player.x'),300);
  g.key('KeyD');assert.equal(g.run('player.vx'),440);g.key('KeyA');assert.equal(g.run('player.vx'),-440);
  g.run('updatePhysics(1/240)');assert.equal(g.run('player.vx'),-440);
});
test('Landing removes horizontal slide on that same step',()=>{
  const g=createGame();g.run("Object.assign(player,{x:300,y:883.9,vx:1500,vy:100,state:'AIRBORNE',onGround:false});updatePhysics(1/240)");
  assert.equal(g.run('player.onGround'),true);assert.equal(g.run('player.vx'),0);
  const x=g.run('player.x');g.run('for(let i=0;i<24;i++)updatePhysics(1/240)');assert.equal(g.run('player.x'),x);
});
test('Standing with a connected slack chain has no ground slide',()=>{
  const g=createGame();g.run("Object.assign(player,{x:300,y:884,vx:600,vy:0,state:'GRAPPLING',onGround:true});Object.assign(grapple,{active:true,mode:'WALL',anchorX:300,anchorY:528,length:440});updatePhysics(1/240)");
  assert.equal(g.run('player.x'),300);assert.equal(g.run('player.vx'),0);assert.equal(g.run('grapple.active'),true);
});
test('Held chain remains attached for 500ms after dash and releases only on mouse-up',()=>{
  const g=createGame();g.run("Object.assign(player,{x:3000,y:-2000,vx:200,vy:0,state:'GRAPPLING'});Object.assign(grapple,{active:true,mode:'WALL',anchorX:3000,anchorY:-2300,length:300});keys.d=true;executeSwingDash();for(let i=0;i<120;i++)updatePhysics(1/240)");
  assert.equal(g.run('grapple.active'),true);assert.equal(g.run('player.state'),'GRAPPLING');
  assert(g.run('Math.hypot(player.x-grapple.anchorX,player.y-grapple.anchorY)')<=300.001);
  g.event('mouseup',{button:0});assert.equal(g.run('grapple.active'),false);
});
test('Shift input keeps the chain connected; only left mouse release detaches it',()=>{
  const g=createGame();g.run("Object.assign(player,{x:3000,y:-2000,vx:200,vy:0,state:'GRAPPLING'});Object.assign(grapple,{active:true,mode:'WALL',anchorX:3000,anchorY:-2300,length:300})");
  g.key('KeyD');g.key('ShiftLeft');g.key('ShiftLeft',false);g.event('mouseup',{button:2});
  assert.equal(g.run('grapple.active'),true);assert.equal(g.run('player.swingDashReady'),false);
  g.event('mouseup',{button:0});assert.equal(g.run('grapple.active'),false);
});
test('Real starting-platform movement can attach, rise and chain-dash without detaching',()=>{
  const g=createGame();g.run('for(let i=0;i<144;i++)updatePhysics(1/240)');
  g.key('KeyD');g.run('for(let i=0;i<48;i++)updatePhysics(1/240)');g.key('KeyD',false);
  g.run('mouse.screenX=440-camera.x+canvas.width/2;mouse.screenY=527-camera.y+canvas.height/2;fireChainArm()');
  assert.equal(g.run('grapple.active'),true);
  g.key('KeyD');g.run('for(let i=0;i<480 && player.vy>=-180;i++)updatePhysics(1/240)');
  assert(g.run('player.vy')< -180);g.key('ShiftLeft');
  assert.equal(g.run('grapple.active'),true);assert(g.run('player.vx')>0);assert(g.run('player.vy')<0);
});
test('Anchor-height dash preserves the current rotation without a sudden flip',()=>{
  const dirs=[];for(const y of [-.01,.01]) {const g=createGame();g.run(`Object.assign(player,{x:3200,y:-2000+${y},vx:0,vy:-400});Object.assign(grapple,{active:true,anchorX:3000,anchorY:-2000});keys.d=true`);dirs.push(g.run('getDashDirection()'));}
  assert(dirs.every(d=>d.y<-.99));assert(Math.abs(dirs[0].x-dirs[1].x)<.001);
});
test('Dash cannot recharge repeatedly by remaining on the same ground contact',()=>{
  const g=createGame();g.run("Object.assign(player,{x:300,y:884,vx:0,vy:0,state:'GRAPPLING',onGround:true});Object.assign(grapple,{active:true,mode:'WALL',anchorX:300,anchorY:528,length:450});keys.d=true;executeSwingDash();updatePhysics(1/240)");
  assert.equal(g.run('player.swingDashReady'),false);
});
test('Same-side airborne steering preserves boosted speed; opposite input brakes',()=>{
  const g=createGame();g.run("Object.assign(player,{x:3000,y:-2000,vx:1500,vy:0,state:'AIRBORNE'});keys.d=true;for(let i=0;i<24;i++)updatePhysics(1/240)");assert(g.run('player.vx')>1400);
  g.run('keys.d=false;keys.a=true;for(let i=0;i<24;i++)updatePhysics(1/240)');assert(g.run('player.vx')<1300);
});
test('Dash input protection holds for 120ms',()=>{
  const g=createGame();g.run("Object.assign(player,{x:3000,y:-2000,vx:1000,vy:0,state:'AIRBORNE',dashControlLock:.12});keys.a=true;for(let i=0;i<20;i++)updatePhysics(1/240)");assert(g.run('player.vx')>980);
});
test('Jump buffering and coyote time work without granting infinite jumps',()=>{
  const g=createGame();g.run("Object.assign(player,{x:300,y:882,vx:0,vy:100,state:'AIRBORNE',jumpBuffer:.12});for(let i=0;i<12;i++)updatePhysics(1/240)");assert(g.run('player.vy')<0);assert.equal(g.run('player.jumpBuffer'),0);
  g.run("Object.assign(player,{state:'AIRBORNE',coyoteTimer:.06,vy:30,jumpBuffer:.12});tryJump()");assert.equal(g.run('player.vy'),-590);
  g.run('player.vy=20;player.jumpBuffer=.12;tryJump()');assert.equal(g.run('player.vy'),20);
});
test('Holding away from a wall detaches, with a re-grab cooldown',()=>{
  const g=createGame();g.run("Object.assign(player,{x:3200,y:-2000,vx:0,vy:0,state:'WALL_CLIMB',wallSide:1});keys.a=true;updatePhysics(1/240)");assert.equal(g.run('player.state'),'AIRBORNE');assert(g.run('player.vx')<0);assert(g.run('player.wallDetachTimer')>0);
});
test('High-speed wall and 32px hazard collisions are not skipped',()=>{
  const g=createGame();g.run("Object.assign(player,{x:2770,y:600,vx:1850,vy:0,state:'AIRBORNE'});for(let i=0;i<15;i++)updatePhysics(1/240)");assert(g.run('player.x')<=2804.01);
  g.run("Object.assign(player,{x:4420,y:-490,vx:0,vy:1850,state:'AIRBORNE',checkpointIndex:0});for(let i=0;i<10;i++)updatePhysics(1/240)");assert(g.run('player.x')<400);assert(g.run('player.y')>800);
});
test('30/60/144 Hz render rates produce the same physical movement',()=>{
  const states=[];
  for(const fps of [30,60,144]) {
    const g=createGame();g.run("Object.assign(player,{x:200,y:884,vx:0,vy:0,state:'GROUNDED'});keys.d=true");
    for(let frame=1;frame<=fps;frame++)g.run(`gameLoop(${frame*1000/fps})`);
    states.push(g.run('({x:player.x,y:player.y,vx:player.vx})'));
  }
  assert(Math.max(...states.map(s=>s.x))-Math.min(...states.map(s=>s.x))<2);
  console.log(' ',JSON.stringify(states));
});
test('Pause clears held input; respawn clears dash effects and resets camera',()=>{
  const g=createGame();g.key('KeyD');g.key('Escape');assert.equal(g.run('getMoveAxis()'),0);assert.equal(g.run('paused'),true);
  g.run('player.dashControlLock=.12;player.dashFlashTimer=.2;hitstopTimer=.1;respawnAtCheckpoint()');assert.equal(g.run('player.dashControlLock+player.dashFlashTimer+hitstopTimer'),0);assert.equal(g.run('camera.x'),180);
});
test('Body follows the chain around a full orbit without an angle-wrap snap',()=>{
  const g=createGame();g.run("Object.assign(player,{x:3000,y:-1900,vx:600,vy:0,state:'GRAPPLING'});Object.assign(grapple,{active:true,mode:'WALL',anchorX:3000,anchorY:-2300,length:400})");
  let previous=0;
  for(let step=1;step<=360;step++) {
    const angle=step*Math.PI/180;
    g.run(`player.x=${3000+Math.sin(angle)*400};player.y=${-2300+Math.cos(angle)*400};updatePlayerRotation(1/60)`);
    const rotation=g.run('player.rotation');
    assert(Math.abs(rotation-previous)<.03);previous=rotation;
  }
  assert(Math.abs(previous+2*Math.PI)<.03);
});
test('A/D flips the visor immediately without rotating the body or waiting for velocity reversal',()=>{
  const g=createGame();g.run("Object.assign(player,{x:3200,y:-2100,vx:500,vy:-500,state:'GRAPPLING'});Object.assign(grapple,{active:true,mode:'WALL',anchorX:3000,anchorY:-2300,length:300});for(let i=0;i<60;i++)updatePlayerRotation(1/240)");
  const angle=g.run('player.rotation');assert.equal(g.run('player.visualFacing'),1);
  g.key('KeyA');
  assert.equal(g.run('player.visualFacing'),-1);assert(Math.abs(g.run('player.rotation')-angle)<1e-10);
  assert.equal(g.run('player.vx'),500);
  g.key('KeyA',false);g.run('updatePlayerRotation(1/240)');assert.equal(g.run('player.visualFacing'),-1);
});
test('Successful hook attaches the body to the chain angle in the same event, even from the ground',()=>{
  const g=createGame();g.run("Object.assign(player,{x:280,y:884,vx:0,vy:0,state:'GROUNDED',onGround:true,rotation:2});mouse.screenX=440-camera.x+canvas.width/2;mouse.screenY=527-camera.y+canvas.height/2");
  g.key('KeyA');g.run('fireChainArm()');
  assert.equal(g.run('grapple.active'),true);assert.equal(g.run('player.visualFacing'),-1);
  assert(g.run('Math.abs(Math.atan2(Math.sin(player.rotation-getGrappleFrame().rotation),Math.cos(player.rotation-getGrappleFrame().rotation)))')<1e-12);
});
test('Rotation stays exactly on the chain after every physics step, including ground contact',()=>{
  const g=createGame();g.run("Object.assign(player,{x:300,y:884,vx:0,vy:0,state:'GRAPPLING',onGround:true});Object.assign(grapple,{active:true,mode:'WALL',anchorX:500,anchorY:528,length:480});updatePhysics(1/240)");
  assert(g.run('Math.abs(player.rotation)')>.4);
  g.run("Object.assign(player,{x:3200,y:-2100,vx:800,vy:-800,state:'GRAPPLING',onGround:false});Object.assign(grapple,{anchorX:3000,anchorY:-2300,length:300})");
  for(let i=0;i<120;i++) {
    g.run('updatePhysics(1/240)');
    assert(g.run('Math.abs(Math.atan2(Math.sin(player.rotation-getGrappleFrame().rotation),Math.cos(player.rotation-getGrappleFrame().rotation)))')<1e-12);
  }
});
test('A and D mean local left/right above, below and beside the anchor, including same-event dash',()=>{
  for(const [rx,ry] of [[0,300],[0,-300],[300,0],[-300,0],[200,200],[200,-200]]) for(const axis of [-1,1]) {
    const g=createGame();g.run(`Object.assign(player,{x:${3000+rx},y:${-2300+ry},vx:700,vy:-500,state:'GRAPPLING'});Object.assign(grapple,{active:true,mode:'WALL',anchorX:3000,anchorY:-2300,length:400})`);
    g.key(axis<0?'KeyA':'KeyD');
    const expected=g.run('({x:Math.cos(player.rotation)*player.visualFacing,y:Math.sin(player.rotation)*player.visualFacing})');
    assert.equal(g.run('player.visualFacing'),axis);
    g.key('ShiftLeft');
    assert(Math.abs(g.run('player.vx/Math.hypot(player.vx,player.vy)')-expected.x)<1e-10);
    assert(Math.abs(g.run('player.vy/Math.hypot(player.vx,player.vy)')-expected.y)<1e-10);
    assert.equal(g.run('grapple.active'),true);assert.equal(g.run('player.visualFacing'),axis);
  }
});
test('Landing and respawn restore an upright body on the same step',()=>{
  const g=createGame();g.run("Object.assign(player,{x:300,y:883.9,vx:0,vy:100,state:'AIRBORNE',rotation:1.2,onGround:false});updatePhysics(1/240)");
  assert.equal(g.run('player.rotation'),0);
  g.run('player.rotation=2.7;respawnAtCheckpoint()');assert.equal(g.run('player.rotation'),0);
});
test('Scarf attachment rotates with the neck and afterimages retain their pose',()=>{
  const g=createGame();g.run('Object.assign(player,{x:3000,y:-2000,rotation:Math.PI/2,visualFacing:1});updateScarfAndParticles(0)');
  assert(Math.abs(g.run('player.scarfNodes[0].x')-3004)<1e-9);
  assert(Math.abs(g.run('player.scarfNodes[0].y')+2006)<1e-9);
  g.run("Object.assign(player,{vx:900,vy:0,state:'AIRBORNE'});updatePhysics(1/240)");
  assert(g.run('player.afterimages.length')>0);assert(g.run('Number.isFinite(player.afterimages[0].rotation)'));
  const ghostAngle=g.run('player.afterimages[0].rotation');g.run('player.rotation=0');assert.equal(g.run('player.afterimages[0].rotation'),ghostAngle);
});
test('Body rotation follows free flight and stays finite at a zero-length chain',()=>{
  const g=createGame();g.run("Object.assign(player,{vx:700,vy:-700,state:'AIRBORNE'});for(let i=0;i<60;i++)updatePlayerRotation(1/240)");
  assert(Math.abs(g.run('player.rotation')+Math.PI/4)<.001);
  g.run("Object.assign(grapple,{active:true,mode:'WALL',anchorX:player.x,anchorY:player.y});updatePlayerRotation(1/240)");assert(g.run('Number.isFinite(player.rotation)'));
});
test('All four sectors load, settle and render with finite state',()=>{
  const g=createGame();for(let sector=0;sector<4;sector++){g.run(`teleportToSector(${sector});for(let i=0;i<240;i++)updatePhysics(1/240);updateAimRaycast();updateScarfAndParticles(1/60);render();updateHUD()`);assert(g.run('[player.x,player.y,player.vx,player.vy].every(Number.isFinite)'));}
});
console.log('\n'+tests.length+' checks passed.');
if (process.argv.includes('--render-pose')) {
  const {createCanvas}=require('C:/Users/TAEHYEON/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/@napi-rs/canvas');
  const canvas=createCanvas(1280,800), g=createGame(canvas);
  g.run("Object.assign(player,{x:800,y:740,vx:700,vy:-660,state:'GRAPPLING'});Object.assign(grapple,{active:true,mode:'WALL',anchorX:600,anchorY:528,length:292,shootProgress:1});camera.x=800;camera.y=640;for(let i=0;i<60;i++){updatePlayerRotation(1/240);updateScarfAndParticles(1/240)}updateAimRaycast();render()");
  fs.writeFileSync(__dirname+'/rotation-preview.png',canvas.toBuffer('image/png'));
  console.log('Rendered actual game canvas to rotation-preview.png');
}
