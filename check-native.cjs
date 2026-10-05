const {chromium}=require('C:/Users/lenovo/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const assert=require('node:assert/strict');
(async()=>{
 const browser=await chromium.launch({headless:true,channel:'msedge'});
 const page=await browser.newPage({viewport:{width:1440,height:900},reducedMotion:'reduce'});
 const errors=[];page.on('pageerror',e=>errors.push(e.message));
 await page.goto('http://127.0.0.1:4180/index.html');
 await page.click('#startButton');await page.click('#selectChola');
 await page.waitForFunction(()=>!document.getElementById('launchNativeBtn').disabled);
 const response=await page.request.post('http://127.0.0.1:4180/api/game/launch',{data:{runtime:'civilization'}});assert.equal(response.status(),403);
 await page.click('#launchNativeBtn');
 await page.waitForFunction(()=>document.getElementById('nativeStatus').textContent.includes('running in a separate'),{},{timeout:20000});
 const status=await (await page.request.get('http://127.0.0.1:4180/api/game/status')).json();assert(status.running);assert.equal(status.runtime,'civilization');
 await page.screenshot({path:'preview-native-launch.png',fullPage:true});
 await page.setViewportSize({width:390,height:844});assert(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth));
 await page.screenshot({path:'preview-native-launch-mobile.png',fullPage:true});assert.deepEqual(errors,[]);
 console.log('PASS: intro to Chola selection to actual Unity process, protected launch endpoint, running status, desktop/mobile layout. Native game left open for the user.');
 await browser.close();
})().catch(e=>{console.error(e);process.exit(1)});

