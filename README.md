# YUG — Chola prototype

Public game page: https://pratikcompjal-bit.github.io/YUG/ . It offers a browser strategy game and the downloadable Windows 3D civilization. For a presentation, add this link and `assets/yug-ppt-qr.png`; `assets/yug-ppt-gameplay.png` is a gameplay screenshot. The QR opens the same public page. The static site is published from `main/docs` on GitHub Pages. Run `python package.py` before pushing future web or native builds, then commit the updated `docs/` files.

Run `Play-YUG.cmd`, keep its window open, and visit http://127.0.0.1:4180/index.html. Python is required for the local native-game bridge. Opening index.html directly still shows the intro but cannot launch a Windows application. The opening retains the cloud → Earth → India sequence, then reveals a photograph-led Chola start screen. Click to Start opens the Chola heartland map.

Only the Chola dynasty is selectable. Three location controls show Thanjavur, Gangaikonda Cholapuram, and Nagapattinam. The map uses detailed Natural Earth geography and location markers, not speculative political borders. 

A separate browser strategy prototype is also available under Controls & other versions: **Rajendra's Realm (The Chola Chronicle)**. The player guides the 11th-century empire through 10 regnal years (1014–1025 CE) across four dimensions: Temple Grandeur, Hydraulic Wealth, Maritime Reach, and Assembly Harmony. Features include provincial monumental construction across Thanjavur, Gangaikonda Cholapuram, and Nagapattinam, annual historical dilemmas, an interactive Kudavolai democratic palm-leaf ballot drawer, a Srivijayan naval trade expedition planner, and a concluding epigraphic stone inscription summarizing the sovereign's reign.

Design: real temple photography, restrained charcoal/stone/green colors, Libre Caslon Display and DM Sans, with Tamil lettering. Google Fonts are optional; system fallbacks are provided. All imagery and geographic data are local.

## Credits
- Photograph: Marcllendr, “Brihadisvara Temple Thanjavur Full View.jpg”, https://commons.wikimedia.org/wiki/File:Brihadisvara_Temple_Thanjavur_Full_View.jpg
- Photo license: CC BY-SA 4.0, https://creativecommons.org/licenses/by-sa/4.0/ . Cropped and shaded in the interface. The adapted photograph presentation is offered under the same license. The original photo is retained in assets/chola-temple.jpg. This is a contemporary image, not a reconstruction of medieval buildings.
- Historical context: UNESCO Great Living Chola Temples, https://whc.unesco.org/en/list/250/ ; Nagapattinam district history, https://nagapattinam.nic.in/about-district/history/ . References also appear in the interface.
- Detailed geographic outlines: Natural Earth 1:50m country geography, public domain, https://github.com/nvkelso/natural-earth-vector/blob/master/geojson/ne_50m_admin_0_countries.geojson . India and Sri Lanka are extracted into assets/chola-geography.js.
- Opening globe outlines: https://github.com/johan/world.geo.json .
- Music remains an original synthesized accompaniment, enabled explicitly with Music on.

`check.cjs` verifies desktop/mobile screens, intro/replay, photograph loading, location selection, Chola selection, credits, music and reduced motion with the locally bundled Playwright. The previous multi-era data is preserved in archive/eras-before-chola.js. Original pre-edit files are preserved under original/.


## Playable native Chola settlement

Flow: opening → Click to Start → Select Chola dynasty → Play Chola civilization.

The primary action opens **YUG: The Kaveri Settlement** in a separate Windows window. This is a newly compiled Unity 6000.3.25f1 project at `civilization/`, adapted from the supplied Unity source. The original project remains untouched; the earlier executable is retained in `native-game/`. The browser strategy game remains available under Controls & other versions.

The settlement contains twelve homes, six market stalls, thirteen residents, a well, rice paddies, timber and stone yards, an irrigation channel, and the existing temple complex. The playable objective is to meet the steward, gather 4 timber and 4 stone, repair irrigation, deliver 9 grain to the granary, then recover and assemble the temple monument and open its sanctuary. Market trading and stone purchases offer extra resource options. Resource patches recover over time. Completion is saved automatically.

Controls: WASD move, Shift run, Space jump, right mouse drag/arrows orbit, F village action, E temple action, J inscription, Tab journal, G high/performance graphics, Esc pause. Nearby labels identify service and resource locations; the journal explains their positions.

The visuals use weathered materials, real-time shadows, antialiasing, moving water, palms, animated residents, and the supplied architectural details. This is a compact stylized prototype with fictional settlement layout, not photorealistic art or a surveyed historical reconstruction. Residents follow simple routes; this is not a full population simulation.

`civilization/Build/YUG Civilization.exe` also runs directly. Saves are `construction-journey.json` and `settlement.json` beside it. New settlement resets both after confirmation; Continue restores resources and temple progress, returning the player to the southern entrance. Resource cooldowns restart when the application restarts. Player logs are in `civilization/Build/Player.log` when launched through YUG. Test saves have separate names.

To rebuild, open `civilization/` in Unity 6000.3.25f1 and run `PrototypeBuild.CreateAndBuild` in batch mode; the editor script is in `Assets/Editor/PrototypeBuild.cs`. Build diagnostics and verification artifacts are in `civilization/work/`. `-verifySettlement` exercises village rules, save/reload and actual controller movement. `-verifyTemple` exercises recovery, construction order, animated assembly, door collisions, sanctuary entry, saving and reset. Run native checks with the game visible; Windows may severely throttle an obscured window.

The supplied Unreal project still requires the absent engine configured in `launcher-config.json`. Its optional launcher becomes available when that configuration points to a working engine and project.

`serve.py` binds only to 127.0.0.1, checks Host/Origin and a per-session token, and launches only fixed targets. No client-supplied executable or command is accepted. Duplicate clicks reuse the running session. Intro music mutes on successful launch; it can be re-enabled manually after returning. The game is a Windows application, not embedded browser content.

`check-native.cjs` verifies the intro-to-civilization launch flow, unauthorized launch rejection, and desktop/mobile launcher layout. It leaves the native game open.
