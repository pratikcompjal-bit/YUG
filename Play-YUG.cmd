@echo off
setlocal
cd /d "%~dp0"
echo Open http://127.0.0.1:4180/index.html in your browser.
echo Keep this launcher running while using YUG.
python serve.py --port 4180
pause
