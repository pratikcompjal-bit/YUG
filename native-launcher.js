(() => {
  const button = document.getElementById('launchNativeBtn');
  const unreal = document.getElementById('launchUnrealBtn');
  const message = document.getElementById('nativeStatus');
  const retry = document.getElementById('retryNativeStatus');
  const localBridge = ['127.0.0.1', 'localhost'].includes(location.hostname) && location.port === '4180';
  const download = 'downloads/YUG-Civilization-Windows-v1.2.zip';
  if (!localBridge) {
    button.textContent = 'Download 3D civilization · Windows';
    button.addEventListener('click', () => { location.href = download; });
    unreal.hidden = true;
    retry.hidden = true;
    message.textContent = 'The 3D settlement runs on 64-bit Windows. Extract the entire ZIP, open the YUG Civilization folder, then run YUG Civilization.exe. Keep all extracted files together. The online strategy game plays here in your browser.';
    return;
  }
  let token, polling, launching = false, wasRunning = false;
  async function status() {
    const response = await fetch('/api/game/status', {cache:'no-store', signal:AbortSignal.timeout(4000)});
    if (!response.ok) throw new Error('Launcher unavailable');
    const state = await response.json();
    token = state.token;
    button.disabled = launching || state.running || !state.available.civilization;
    unreal.disabled = launching || state.running || !state.available.unreal;
    unreal.textContent = state.available.unreal ? 'Play latest Unreal prototype' : 'Latest Unreal version unavailable';
    button.textContent = state.running ? 'Chola civilization is running' : 'Play Chola civilization';
    retry.hidden = true;
    if (state.running) {
      wasRunning = true;
      message.textContent = 'The game is running in a separate window. Close it to return here.';
    } else if (wasRunning) {
      message.textContent = state.earlyExit || state.exitCode !== 0 ? 'The game exited early. See civilization/Build/Player.log for details, then try again.' : 'Game closed. You can launch it again or return to the map.';
      wasRunning = false;
      clearInterval(polling);
    } else if (!state.available.civilization) {
      message.textContent = 'The civilization build is missing from civilization/Build.';
    }
    return state;
  }
  function offline() {
    message.textContent = 'To launch the Windows game, run Play-YUG.cmd and open http://127.0.0.1:4180. A static preview cannot start it.';
    button.disabled = true; unreal.disabled = true; retry.hidden = false;
    clearInterval(polling);
  }
  async function launch(runtime) {
    if (launching) return;
    launching = true; button.disabled = true; unreal.disabled = true;
    message.textContent = 'Starting the Chola settlement…';
    try {
      const current = await status();
      if (current.running) return;
      const response = await fetch('/api/game/launch', {method:'POST', headers:{'Content-Type':'application/json','X-YUG-Token':token}, body:JSON.stringify({runtime}), signal:AbortSignal.timeout(10000)});
      const result = await response.json();
      if (!response.ok) throw new Error(result.error || 'Unable to launch game.');
      if (document.getElementById('soundToggle').getAttribute('aria-pressed') === 'true') document.getElementById('soundToggle').click();
      wasRunning = true;
      clearInterval(polling);polling = setInterval(()=>status().catch(offline), 2000);
      message.textContent = 'Starting in a separate game window. Loading may take a moment.';
    } catch(error) {
      message.textContent = error.message;
      retry.hidden = false;
    } finally {
      launching = false;
      await status().catch(offline);
    }
  }
  button.addEventListener('click',()=>launch('civilization'));
  unreal.addEventListener('click',()=>launch('unreal'));
  retry.addEventListener('click',()=>status().catch(offline));
  status().catch(offline);
})();

