/* Geographic outlines: johan/world.geo.json (Natural Earth, public domain). */
const canvas = document.getElementById('world');
const ctx = canvas.getContext('2d');
const reducedMotion = matchMedia('(prefers-reduced-motion: reduce)');
const duration = 9000;
let width, height, countries, india, started = 0, frame, ready = false;
const rad = Math.PI / 180;
const clamp = n => Math.max(0, Math.min(1, n));
const smooth = n => { n = clamp(n); return n * n * (3 - 2 * n); };
const mix = (a,b,t) => a + (b-a)*t;
const rings = feature => feature.geometry.type === 'Polygon' ? feature.geometry.coordinates : feature.geometry.coordinates.flat();
function project(point, lon) {
  const l = (point[0] - lon)*rad, p = point[1]*rad, center = 23*rad;
  return [Math.cos(p)*Math.sin(l), -(Math.cos(center)*Math.sin(p)-Math.sin(center)*Math.cos(p)*Math.cos(l)), Math.sin(center)*Math.sin(p)+Math.cos(center)*Math.cos(p)*Math.cos(l)];
}
function resize() {
  width = innerWidth; height = Math.max(innerHeight,760);
  const dpr = Math.min(devicePixelRatio || 1,2);
  canvas.width=width*dpr; canvas.height=height*dpr;
  ctx.setTransform(dpr,0,0,dpr,0,0);
  if (countries) draw(ready ? 1 : clamp((performance.now()-started)/duration));
}
function path(feature, lon, scale, cx, cy) {
  ctx.beginPath();
  for (const ring of rings(feature)) {
    let open = false;
    for (const point of ring) {
      const [x,y,z] = project(point,lon);
      if(z < 0) { open=false; continue; }
      if(!open) {ctx.moveTo(cx+x*scale,cy+y*scale);open=true;} else ctx.lineTo(cx+x*scale,cy+y*scale);
    }
    ctx.closePath();
  }
}
function draw(t) {
  ctx.clearRect(0,0,width,height);
  const bg=ctx.createRadialGradient(width/2,height*.37,0,width/2,height*.4,height*.85);
  bg.addColorStop(0,'#26332f');bg.addColorStop(.5,'#131e21');bg.addColorStop(1,'#0c1118');ctx.fillStyle=bg;ctx.fillRect(0,0,width,height);

  const zoom=smooth((t-.43)/.49), reveal=smooth((t-.65)/.27), lon=mix(49,79,smooth(t/.8));
  const initial=Math.min(width*.34,height*.28),target=Math.min(height*.84,width*1.02);
  const scale=mix(initial,target,zoom),cx=width/2,cy=mix(height*.49,height*.385,zoom);
  ctx.save();ctx.globalAlpha=1-reveal;
  ctx.shadowColor='#64b8ed';ctx.shadowBlur=26;
  const ocean=ctx.createRadialGradient(cx-scale*.4,cy-scale*.45,scale*.1,cx,cy,scale);
  ocean.addColorStop(0,'#34617a');ocean.addColorStop(.55,'#123953');ocean.addColorStop(1,'#040e1b');
  ctx.fillStyle=ocean;ctx.beginPath();ctx.arc(cx,cy,scale,0,Math.PI*2);ctx.fill();ctx.shadowBlur=0;
  ctx.save();ctx.beginPath();ctx.arc(cx,cy,scale,0,Math.PI*2);ctx.clip();
  for(const f of countries) {path(f,lon,scale,cx,cy);ctx.fillStyle=f.id==='IND'?'#bca574':'#66827a';ctx.fill();ctx.strokeStyle='#c5dbc226';ctx.lineWidth=.5;ctx.stroke();}
  const shade=ctx.createRadialGradient(cx-scale*.4,cy-scale*.4,scale*.2,cx+scale*.25,cy+scale*.15,scale*1.3);shade.addColorStop(0,'#06132100');shade.addColorStop(.6,'#030b1720');shade.addColorStop(1,'#01050bee');ctx.fillStyle=shade;ctx.fillRect(cx-scale,cy-scale,scale*2,scale*2);ctx.restore();
  ctx.strokeStyle='#80cafa60';ctx.lineWidth=1;ctx.beginPath();ctx.arc(cx,cy,scale,0,Math.PI*2);ctx.stroke();ctx.restore();
  if(reveal>0){ctx.save();ctx.globalAlpha=reveal;path(india,lon,scale,cx,cy);ctx.shadowColor='transparent';ctx.shadowBlur=0;const gold=ctx.createLinearGradient(cx,cy-scale*.2,cx,cy+scale*.25);gold.addColorStop(0,'#c0c4a9');gold.addColorStop(.45,'#909b7e');gold.addColorStop(1,'#697961');ctx.fillStyle=gold;ctx.fill();ctx.shadowBlur=0;ctx.strokeStyle='#c3cbb5';ctx.lineWidth=1;ctx.stroke();

    const [px,py]=project([79.131,10.783],lon);ctx.beginPath();ctx.arc(cx+px*scale,cy+py*scale,4,0,Math.PI*2);ctx.fillStyle='#fff0c3';ctx.shadowColor='#ffda89';ctx.shadowBlur=15;ctx.fill();ctx.shadowBlur=0;ctx.strokeStyle='#ffdf9c70';ctx.beginPath();ctx.arc(cx+px*scale,cy+py*scale,12,0,Math.PI*2);ctx.stroke();ctx.restore();}
}
function finish() {
  cancelAnimationFrame(frame);ready=true;document.body.dataset.phase='ready';
  document.getElementById('landing').inert=false;document.getElementById('replayButton').hidden=false;
  document.getElementById('skipIntro').disabled=true;draw(1);
}
function tick(now) {
  const t=clamp((now-started)/duration);draw(t);document.getElementById('progress').style.width=`${t*100}%`;
  document.getElementById('introText').textContent=t>.7?'The Chola heartland.':t>.4?'India. Eleventh century.':'A journey to the south.';
  if(t>=1) finish();else frame=requestAnimationFrame(tick);
}
function play() {
  if(!countries)return;document.getElementById('eraScreen').hidden=true;document.getElementById('landing').hidden=false;delete document.body.dataset.screen;window.scrollTo(0,0);cancelAnimationFrame(frame);ready=false;
  document.body.dataset.phase='loading';document.getElementById('landing').inert=true;
  document.getElementById('replayButton').hidden=true;document.getElementById('skipIntro').disabled=false;
  void document.body.offsetWidth;
  if(reducedMotion.matches){finish();return;}
  started=performance.now();document.body.dataset.phase='intro';frame=requestAnimationFrame(tick);
}
function load() {
  try{countries=window.WORLD_GEOGRAPHY.features;india=countries.find(f=>f.id==='IND');if(!india)throw new Error('Missing map');resize();play();}
  catch{document.getElementById('loadError').hidden=false;document.getElementById('skipIntro').hidden=true;}
}
window.addEventListener('resize',resize);
document.getElementById('retryButton').addEventListener('click',()=>location.reload());
document.getElementById('skipIntro').addEventListener('click',()=>{finish();document.getElementById('startButton').focus({preventScroll:true});});
document.getElementById('replayButton').addEventListener('click',()=>{play();if(!reducedMotion.matches)document.getElementById('skipIntro').focus({preventScroll:true});});
reducedMotion.addEventListener('change',()=>{if(reducedMotion.matches&&countries)finish();});
const score = new HeritageScore();
let sound = false;
document.getElementById('soundToggle').addEventListener('click', async () => {
  try {
    sound = await score.toggle();
    document.getElementById('soundToggle').setAttribute('aria-pressed', String(sound));
    document.getElementById('soundLabel').textContent = sound ? 'Music on' : 'Music off';
  } catch { document.getElementById('soundLabel').textContent = 'Music unavailable'; }
});
load();
