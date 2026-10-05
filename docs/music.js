/* Original, synthesized Indian-heritage-inspired score. No recordings or samples.
   A softly plucked tonic/fifth drone under a pentatonic flute-like melody. */
class HeritageScore {
  constructor() { this.enabled=false; this.timer=null; this.notes=new Set(); }
  initialize() {
    this.context=new AudioContext();
    this.master=this.context.createGain();this.master.gain.value=0;
    const limiter=this.context.createDynamicsCompressor();
    limiter.threshold.value=-18;limiter.ratio.value=6;
    this.master.connect(limiter);limiter.connect(this.context.destination);
    const delay=this.context.createDelay(1);delay.delayTime.value=.29;
    const echo=this.context.createGain();echo.gain.value=.19;
    this.reverb=this.context.createGain();this.reverb.gain.value=.25;
    this.reverb.connect(delay);delay.connect(echo);echo.connect(delay);delay.connect(this.master);
  }
  voice(frequency, time, length, level, flute=false) {
    const c=this.context, env=c.createGain();
    env.gain.setValueAtTime(0,time);
    env.gain.linearRampToValueAtTime(level,time+(flute?.18:.012));
    env.gain.exponentialRampToValueAtTime(.0001,time+length);
    env.connect(this.master);env.connect(this.reverb);
    const harmonics=flute?[1,.12,.035]:[1,.3,.17,.09,.04];
    let live=harmonics.length;
    harmonics.forEach((amplitude,index)=>{
      const osc=c.createOscillator(), partial=c.createGain();
      osc.type='sine';osc.frequency.setValueAtTime(frequency*(index+1),time);
      if(flute){osc.frequency.linearRampToValueAtTime(frequency*(index+1)*1.006,time+.13);osc.frequency.linearRampToValueAtTime(frequency*(index+1),time+.4);}
      else osc.detune.value=index%2?1.8:-1.8;
      partial.gain.value=amplitude;osc.connect(partial);partial.connect(env);
      this.notes.add(osc);osc.onended=()=>{this.notes.delete(osc);osc.disconnect();partial.disconnect();if(--live===0)env.disconnect();};
      osc.start(time);osc.stop(time+length+.05);
    });
  }
  schedule() {
    const c=this.context;
    const melody=[0,4,7,9,7,4,2,0, null,2,4,7,12,9,7,4,null,7,9,12,9,7,4,2];
    while(this.next<c.currentTime+.3){
      const step=this.step++, time=this.next;
      this.voice([146.832,110,110,220][step%4],time,3.7,.055);
      if(step%2===0){const note=melody[(step/2)%melody.length];if(note!==null)this.voice(440*Math.pow(2,note/12),time+.08,1.9,.08,true);}
      this.next+=.85;
    }
  }
  async toggle() {
    if(!this.context)this.initialize();
    await this.context.resume();this.enabled=!this.enabled;
    const now=this.context.currentTime;
    this.master.gain.cancelScheduledValues(now);
    this.master.gain.setTargetAtTime(this.enabled?.65:0,now,.22);
    if(this.enabled){this.next=now+.08;this.step=0;this.schedule();this.timer=setInterval(()=>this.schedule(),100);}
    else {clearInterval(this.timer);this.timer=null;for(const osc of this.notes){try{osc.stop(now+.9);}catch{}}}
    return this.enabled;
  }
  playSfx(type='bell') {
    if(!this.context) try { this.initialize(); } catch(e){ return; }
    if(this.context.state==='suspended') { this.context.resume().catch(()=>{}); }
    const now = this.context.currentTime;
    if(type==='bell') {
      [220, 440, 660, 880].forEach((f, i) => {
        const osc = this.context.createOscillator();
        const g = this.context.createGain();
        osc.type = i === 0 ? 'triangle' : 'sine';
        osc.frequency.setValueAtTime(f, now);
        g.gain.setValueAtTime(0.08 / (i + 1), now);
        g.gain.exponentialRampToValueAtTime(0.0001, now + 2.2);
        osc.connect(g); g.connect(this.master);
        osc.start(now); osc.stop(now + 2.3);
      });
    } else if(type==='wood') {
      const osc = this.context.createOscillator();
      const g = this.context.createGain();
      osc.type = 'triangle';
      osc.frequency.setValueAtTime(320, now);
      osc.frequency.exponentialRampToValueAtTime(140, now + 0.08);
      g.gain.setValueAtTime(0.12, now);
      g.gain.exponentialRampToValueAtTime(0.001, now + 0.09);
      osc.connect(g); g.connect(this.master);
      osc.start(now); osc.stop(now + 0.1);
    } else if(type==='chime') {
      [587.33, 739.99, 880, 1174.66].forEach((f, i) => {
        const osc = this.context.createOscillator();
        const g = this.context.createGain();
        osc.type = 'sine';
        osc.frequency.setValueAtTime(f, now + i * 0.07);
        g.gain.setValueAtTime(0, now + i * 0.07);
        g.gain.linearRampToValueAtTime(0.06, now + i * 0.07 + 0.02);
        g.gain.exponentialRampToValueAtTime(0.0001, now + i * 0.07 + 1.2);
        osc.connect(g); g.connect(this.master);
        osc.start(now + i * 0.07); osc.stop(now + i * 0.07 + 1.3);
      });
    }
  }
}
