const {chromium}=require('C:/Users/lenovo/.cache/codex-runtimes/codex-primary-runtime/dependencies/node/node_modules/playwright');
const assert=require('node:assert/strict');
(async()=>{
 const browser=await chromium.launch({headless:true,channel:'msedge'});
 const page=await browser.newPage({viewport:{width:1440,height:900}});
 const errors=[];page.on('pageerror',e=>errors.push(e.message));
 await page.goto('file:///C:/Users/lenovo/Desktop/YUG/index.html');
 await page.waitForFunction(()=>document.body.dataset.phase==='ready');await page.waitForTimeout(1200);
 assert(await page.locator('.temple-photograph img').evaluate(i=>i.complete&&i.naturalWidth>1000));
 await page.screenshot({path:'preview-desktop.png',fullPage:true});
 await page.click('#startButton');assert(await page.locator('#eraScreen').isVisible());
 assert.equal(await page.locator('.era-tabs, .civilization-card, #loginButton, #signupButton').count(),0);
 for(const id of ['gangaikonda','nagapattinam','thanjavur']){await page.locator(`[data-place=${id}]`).click();assert.equal(await page.locator(`[data-place=${id}]`).getAttribute('aria-pressed'),'true');assert.equal(await page.locator('[data-place][aria-pressed=true]').count(),1);}
 await page.click('#selectChola');assert(await page.locator('#prototypeNote').isVisible());assert.equal(await page.locator('#selectChola').getAttribute('aria-pressed'),'true');
 await page.screenshot({path:'preview-chola-map.png',fullPage:true});
 await page.click('#creditsButton');assert(await page.locator('#creditsDialog').isVisible());await page.keyboard.press('Escape');
 await page.setViewportSize({width:390,height:844});await page.screenshot({path:'preview-chola-map-mobile.png',fullPage:true});
 assert(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth));
 await page.click('#backHome');await page.screenshot({path:'preview-mobile.png',fullPage:true});
 assert(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth));
 await page.click('#replayButton');await page.click('#skipIntro');assert.equal(await page.locator('body').getAttribute('data-phase'),'ready');
 await page.click('#soundToggle');assert.equal(await page.locator('#soundToggle').getAttribute('aria-pressed'),'true');await page.click('#soundToggle');
 await page.emulateMedia({reducedMotion:'reduce'});await page.reload();assert.equal(await page.locator('body').getAttribute('data-phase'),'ready');

 // Verify full game flow
 await page.setViewportSize({width:1440,height:900});
 await page.click('#startButton');
 await page.click('#selectChola');
 assert(await page.locator('#launchNativeBtn').isVisible());
 await page.locator('.native-help summary').click();
 await page.click('#browserGameBtn');
 assert(await page.locator('#gameScreen').isVisible());
 assert(await page.locator('#resGrain').isVisible());
 assert.equal(await page.locator('#gameRegnalKicker').textContent(),'REGNAL YEAR 01');

 // Test province switching & project construction in Thanjavur
 await page.click('#tabThanjavur');
 assert.equal(await page.locator('#provName').textContent(),'Thanjavur');
 const initialGranite = parseInt(await page.locator('#resGranite').textContent(),10);
 const firstProjectBtn = page.locator('#projectsList .project-build-btn').first();
 await firstProjectBtn.click();
 const postGranite = parseInt(await page.locator('#resGranite').textContent(),10);
 assert(postGranite < initialGranite, 'Granite should be deducted upon commissioning project');

 // Test royal decree resolution (Year 1)
 const firstDecreeOption = page.locator('#decreeOptions .decree-choice-btn').first();
 await firstDecreeOption.click();
 assert.equal(await page.locator('#gameRegnalKicker').textContent(),'REGNAL YEAR 02');

 // Test interactive Kudavolai ballot drawer (Year 2)
 assert(await page.locator('#openKudavolaiBtn').isVisible());
 await page.click('#openKudavolaiBtn');
 assert(await page.locator('#kudavolaiDialog').isVisible());
 await page.locator('.palm-leaf-card:has(.leaf-status.valid)').first().click();
 assert.equal(await page.locator('#confirmKudavolaiBtn').isEnabled(), true);
 await page.click('#confirmKudavolaiBtn');
 assert.equal(await page.locator('#kudavolaiDialog').isVisible(), false);
 assert.equal(await page.locator('#gameRegnalKicker').textContent(),'REGNAL YEAR 03');

 await page.screenshot({path:'preview-game-desktop.png',fullPage:true});

 // Verify mobile responsive gameplay view
 await page.setViewportSize({width:390,height:844});
 assert(await page.evaluate(()=>document.documentElement.scrollWidth<=innerWidth));
 await page.screenshot({path:'preview-game-mobile.png',fullPage:true});

 // Test returning back to map
 await page.click('#gameBackToMap');
 assert(await page.locator('#eraScreen').isVisible());
 assert(await page.locator('#gameScreen').isHidden());

 assert.deepEqual(errors,[]);
 console.log('PASS: intro, photograph loading, Chola-only selection, all three map locations, credits, back, replay, sound, mobile overflow, reduced motion, game HUD, province construction, decree resolution, Kudavolai ballot, mobile game view, no page errors.');
 await browser.close();
})().catch(e=>{console.error(e);process.exit(1)});
