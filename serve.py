"""YUG loopback-only frontend and fixed-target native game launcher."""
from http.server import ThreadingHTTPServer, SimpleHTTPRequestHandler
from pathlib import Path
import argparse
import json
import secrets
import subprocess
import threading
import time

ROOT = Path(__file__).resolve().parent
TOKEN = secrets.token_urlsafe(32)
LOCK = threading.Lock()
PROCESS = None
STARTED = 0
ACTIVE = None

def targets():
    config = json.loads((ROOT / 'launcher-config.json').read_text(encoding='utf-8-sig'))
    engine = Path(config['unreal_engine'])
    project = Path(config['unreal_project'])
    unity = ROOT / 'native-game' / 'The Stone Remembers.exe'
    civilization = ROOT / 'civilization' / 'Build' / 'YUG Civilization.exe'
    return {
        'civilization': {'available': civilization.is_file(), 'command': [str(civilization), '-screen-fullscreen', '0', '-screen-width', '1280', '-screen-height', '720', '-logFile', str(civilization.parent / 'Player.log')], 'cwd': str(civilization.parent)},
        'unreal': {'available': engine.is_file() and project.is_file(),
                   'command': [str(engine), str(project), '/Game/Chola/Maps/HeritageCourtyard', '-game', '-windowed', '-ResX=1280', '-ResY=720', '-nosplash', '-abslog=' + str(ROOT / 'native-game' / 'unreal-session.log')],
                   'cwd': str(project.parent)},
        'unity': {'available': unity.is_file(), 'command': [str(unity), '-screen-fullscreen', '0', '-screen-width', '1280', '-screen-height', '720', '-logFile', str(ROOT / 'native-game' / 'Player.log')], 'cwd': str(unity.parent)}
    }

def status():
    available = targets()
    running = PROCESS is not None and PROCESS.poll() is None
    return {'available': {key: value['available'] for key, value in available.items()},
            'running': running, 'runtime': ACTIVE,
            'exitCode': PROCESS.poll() if PROCESS is not None and not running else None,
            'earlyExit': PROCESS is not None and not running and time.monotonic() - STARTED < 15}

class Handler(SimpleHTTPRequestHandler):
    def __init__(self, *args, **kwargs):
        super().__init__(*args, directory=str(ROOT), **kwargs)

    def trusted(self):
        allowed = {f'127.0.0.1:{self.server.server_port}', f'localhost:{self.server.server_port}'}
        return self.headers.get('Host') in allowed

    def reply(self, value, code=200):
        data = json.dumps(value).encode()
        self.send_response(code)
        self.send_header('Content-Type', 'application/json')
        self.send_header('Cache-Control', 'no-store')
        self.send_header('Content-Length', str(len(data)))
        self.end_headers()
        self.wfile.write(data)

    def do_GET(self):
        if not self.trusted():
            return self.reply({'error': 'Invalid host'}, 403)
        if self.path == '/api/game/status':
            return self.reply(dict(status(), token=TOKEN))
        if self.path.startswith('/api/'):
            return self.reply({'error': 'Not found'}, 404)
        if not Path(self.translate_path(self.path)).resolve().is_relative_to(ROOT):
            return self.reply({'error': 'Outside site directory'}, 403)
        super().do_GET()

    def do_POST(self):
        origin = self.headers.get('Origin')
        if not self.trusted() or origin != 'http://' + self.headers.get('Host', '') or not secrets.compare_digest(self.headers.get('X-YUG-Token', ''), TOKEN):
            return self.reply({'error': 'Open the launcher from its local YUG page.'}, 403)
        if self.path != '/api/game/launch':
            return self.reply({'error': 'Not found'}, 404)
        try:
            size = int(self.headers.get('Content-Length', '0'))
            if size < 1 or size > 256:
                raise ValueError()
            runtime = json.loads(self.rfile.read(size)).get('runtime')
            target = targets().get(runtime)
            if target is None:
                return self.reply({'error': 'Unknown game version.'}, 400)
            if not target['available']:
                return self.reply({'error': 'This game version is not installed. Check its build files or launcher-config.json.'}, 409)
            global PROCESS, STARTED, ACTIVE
            with LOCK:
                if PROCESS is not None and PROCESS.poll() is None:
                    return self.reply(dict(status(), alreadyRunning=True))
                PROCESS = subprocess.Popen(target['command'], cwd=target['cwd'], shell=False)
                STARTED = time.monotonic()
                ACTIVE = runtime
            return self.reply(status(), 202)
        except (ValueError, TypeError, AttributeError):
            return self.reply({'error': 'Invalid launch request.'}, 400)
        except OSError:
            return self.reply({'error': 'Windows could not start the game. Check its installation and Player.log.'}, 500)

if __name__ == '__main__':
    parser = argparse.ArgumentParser()
    parser.add_argument('--port', type=int, default=4180)
    args = parser.parse_args()
    server = ThreadingHTTPServer(('127.0.0.1', args.port), Handler)
    print(f'YUG is ready at http://127.0.0.1:{args.port}/index.html', flush=True)
    server.serve_forever()
