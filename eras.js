const CHOLA_PLACES = {
  thanjavur:{name:'Thanjavur',point:[79.131,10.783],label:[205,357],kicker:'01 / THE TEMPLE CITY',description:'Rajaraja I’s Brihadisvara temple was completed in 1010. Its granite tower remains one of the defining monuments of the Chola period.',source:'https://whc.unesco.org/en/list/250/'},
  gangaikonda:{name:'Gangaikonda Cholapuram',point:[79.45,11.206],label:[369,287],kicker:'02 / RAJENDRA’S CAPITAL',description:'Rajendra I established a new capital at Gangaikonda Cholapuram. Its Brihadisvara temple belongs to the 11th century and continues the Chola tradition of monumental stone architecture.',source:'https://whc.unesco.org/en/list/250/'},
  nagapattinam:{name:'Nagapattinam',point:[79.843,10.791],label:[412,376],kicker:'03 / THE COASTAL PORT',description:'On the Bay of Bengal, Nagapattinam was a port town of the Chola realm. It places the maritime side of Chola history alongside the inland temple cities.',source:'https://nagapattinam.nic.in/about-district/history/'}
};
const svgNS='http://www.w3.org/2000/svg';
const mapXY=([lon,lat])=>[(lon-73)*52,(18.5-lat)*45];
const ringPath=ring=>ring.map((p,i)=>`${i?'L':'M'}${mapXY(p).map(n=>n.toFixed(2)).join(',')}`).join(' ')+'Z';
function svgNode(tag,attributes,parent,text){const el=document.createElementNS(svgNS,tag);for(const [k,v] of Object.entries(attributes))el.setAttribute(k,v);if(text)el.textContent=text;parent.appendChild(el);return el;}
const land=document.getElementById('historicalLand');
for(const feature of window.CHOLA_GEOGRAPHY.features){const polys=feature.geometry.type==='Polygon'?[feature.geometry.coordinates]:feature.geometry.coordinates;svgNode('path',{d:polys.map(p=>p.map(ringPath).join(' ')).join(' '),fill:'#eeecdf',stroke:'#a8ad97','stroke-width':.9},land);}
function selectPlace(id){
 const place=CHOLA_PLACES[id];if(!place)return;
 document.getElementById('placeTitle').textContent=place.name;document.getElementById('placeKicker').textContent=place.kicker;document.getElementById('placeDescription').textContent=place.description;document.getElementById('placeSource').href=place.source;
 document.querySelectorAll('[data-place]').forEach(b=>b.setAttribute('aria-pressed',String(b.dataset.place===id)));
 const labels=document.getElementById('historicalLabels');labels.replaceChildren();
 for(const [key,p] of Object.entries(CHOLA_PLACES)){
  const [x,y]=mapXY(p.point),[lx,ly]=p.label,selected=key===id;
  svgNode('path',{d:`M${x},${y} L${lx+(lx<x?65:0)},${ly+5}`,fill:'none',stroke:'#848d74','stroke-width':.7},labels);
  if(selected)svgNode('circle',{cx:x,cy:y,r:10,fill:'none',stroke:'#936548','stroke-width':1},labels);
  svgNode('circle',{cx:x,cy:y,r:selected?4.5:3.5,fill:selected?'#8b563a':'#969f84',stroke:'#f7f5e8','stroke-width':1},labels);
  const text=svgNode('text',{x:lx,y:ly,class:'region-label'},labels);
  const lines=p.name==='Gangaikonda Cholapuram'?['Gangaikonda','Cholapuram']:[p.name];
  lines.forEach((line,i)=>svgNode('tspan',{x:lx,dy:i?17:0},text,line));
 }
}
document.querySelectorAll('[data-place]').forEach(b=>b.addEventListener('click',()=>selectPlace(b.dataset.place)));
selectPlace('thanjavur');
document.getElementById('startButton').addEventListener('click',()=>{document.getElementById('landing').hidden=true;document.getElementById('eraScreen').hidden=false;document.body.dataset.screen='eras';window.scrollTo(0,0);document.getElementById('backHome').focus({preventScroll:true});});
document.getElementById('backHome').addEventListener('click',()=>{document.getElementById('eraScreen').hidden=true;document.getElementById('landing').hidden=false;delete document.body.dataset.screen;window.scrollTo(0,0);document.getElementById('startButton').focus({preventScroll:true});});
document.getElementById('selectChola').addEventListener('click',e=>{e.currentTarget.setAttribute('aria-pressed','true');e.currentTarget.textContent='Chola dynasty selected ✓';document.getElementById('selectionSummary').textContent='Southern India · 11th century CE';document.getElementById('prototypeNote').hidden=false;});
const credits=document.getElementById('creditsDialog');
document.getElementById('creditsButton').addEventListener('click',()=>credits.showModal());document.getElementById('closeCredits').addEventListener('click',()=>credits.close());
const eraScreenEl=document.getElementById('eraScreen');
const gameScreenEl=document.getElementById('gameScreen');
const launchGameBtnEl=document.getElementById('browserGameBtn');
const gameBackToMapEl=document.getElementById('gameBackToMap');
if(launchGameBtnEl){
 launchGameBtnEl.addEventListener('click',()=>{
  eraScreenEl.hidden=true;gameScreenEl.hidden=false;document.body.dataset.screen='game';window.scrollTo(0,0);
  if(window.initCholaGameUI)window.initCholaGameUI();
  if(window.score?.playSfx)window.score.playSfx('bell');
 });
}
if(gameBackToMapEl){
 gameBackToMapEl.addEventListener('click',()=>{
  gameScreenEl.hidden=true;eraScreenEl.hidden=false;document.body.dataset.screen='eras';window.scrollTo(0,0);
 });
}
