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
archive = site / "downloads" / "YUG-Civilization-Windows.zip"
archive.parent.mkdir(exist_ok=True)
with ZipFile(archive, "w", ZIP_DEFLATED, compresslevel=9, strict_timestamps=False) as out:
    for source in sorted(build.rglob("*")):
        if not source.is_file() or source.suffix.lower() in {".json", ".log", ".tmp"}:
            continue
        out.write(source, Path("YUG Civilization") / source.relative_to(build))
print(f"Prepared {site} with {archive.stat().st_size / 1024 / 1024:.1f} MiB Windows archive")
