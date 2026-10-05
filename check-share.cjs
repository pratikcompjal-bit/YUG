const {chromium}=require('C:/Users/lenovo/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const assert=require('node:assert/strict');
(async()=>{
 const base=process.env.YUG_SHARE_URL||'http://127.0.0.1:4181/docs/';
 const browser=await chromium.launch({headless:true,channel:'msedge'});
 const page=await browser.newPage({viewport:{width:1280,height:800},reducedMotion:'reduce'});
 const errors=[];page.on('pageerror',e=>errors.push(e.message));
 await page.goto(new URL('index.html',base).href);
 await page.waitForFunction(()=>document.body.dataset.phase==='ready');
 await page.click('#startButton');await page.click('#selectChola');
 assert.equal(await page.locator('#launchNativeBtn').innerText(),'Download 3D civilization · Windows');
 assert(await page.locator('#launchNativeBtn').isEnabled());
 assert(await page.locator('#browserGameBtn').isVisible());
 await page.click('#browserGameBtn');assert(await page.locator('#gameScreen').isVisible());
 assert.equal((await page.request.head(new URL('downloads/YUG-Civilization-Windows.zip',base).href)).status(),200);
 assert.deepEqual(errors,[]);
 await page.screenshot({path:'preview-shareable-game.png',fullPage:true});
 await browser.close();console.log('PASS: publishable site, online game, Windows archive, no browser errors.');
})().catch(e=>{console.error(e);process.exit(1)});

