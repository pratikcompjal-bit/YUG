"""Create the static share page and a clean Windows game archive."""
from pathlib import Path
from zipfile import ZipFile, ZIP_DEFLATED
from shutil import copy2, copytree

root = Path(__file__).resolve().parent
site = root / "docs"
site.mkdir(exist_ok=True)
for name in ("index.html", "styles.css", "script.js", "eras.js", "game.js", "music.js", "native-launcher.js"):
    copy2(root / name, site / name)
copytree(root / "assets", site / "assets", dirs_exist_ok=True)
(site / ".nojekyll").touch()

build = root / "civilization" / "Build"
archive = site / "downloads" / "YUG-Civilization-Windows-v1.2.zip"
archive.parent.mkdir(exist_ok=True)
with ZipFile(archive, "w", ZIP_DEFLATED, compresslevel=9, strict_timestamps=False) as out:
    out.writestr("YUG Civilization/READ ME FIRST.txt", (
        "YUG: THE KAVERI SETTLEMENT\n\n"
        "This 3D edition runs on 64-bit Windows.\n"
        "1. Right-click the ZIP and choose Extract All.\n"
        "2. Open the extracted YUG Civilization folder.\n"
        "3. Double-click YUG Civilization.exe.\n\n"
        "Keep the EXE, UnityPlayer.dll, and YUG Civilization_Data folder together.\n"
        "For a game that runs in a browser, return to the YUG website and\n"
        "choose Play online strategy game.\n"
    ))
    for source in sorted(build.rglob("*")):
        if not source.is_file() or source.suffix.lower() == ".tmp":
            continue
        # Only omit local progress and logs. Unity's Data/*.json files are
        # runtime dependencies and must ship with the executable.
        if source.parent == build and source.name in {
            "construction-journey.json", "construction-test-save.json",
            "settlement.json", "settlement-test.json", "Player.log",
        }:
            continue
        out.write(source, Path("YUG Civilization") / source.relative_to(build))
print(f"Prepared {site} with {archive.stat().st_size / 1024 / 1024:.1f} MiB Windows archive")
