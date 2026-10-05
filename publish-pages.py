"""Enable GitHub Pages from main/docs using the existing Git credential.

The credential stays in memory and is sent only to api.github.com.
"""
import json
import subprocess
import urllib.error
import urllib.request

api = "https://api.github.com/repos/pratikcompjal-bit/YUG/pages"
filled = subprocess.run(
    ["git", "credential", "fill"],
    input="protocol=https\nhost=github.com\n\n",
    text=True,
    capture_output=True,
    check=True,
    timeout=20,
)
fields = dict(line.split("=", 1) for line in filled.stdout.splitlines() if "=" in line)
token = fields["password"]
headers = {
    "Accept": "application/vnd.github+json",
    "Authorization": "Bearer " + token,
    "X-GitHub-Api-Version": "2022-11-28",
    "User-Agent": "YUG-Pages-Publisher",
}
payload = json.dumps({"source": {"branch": "main", "path": "/docs"}}).encode()
request = urllib.request.Request(api, data=payload, headers=headers, method="POST")
try:
    with urllib.request.urlopen(request, timeout=30) as response:
        result = json.load(response)
        print("Pages created:", result.get("html_url"), "status:", result.get("status"))
except urllib.error.HTTPError as error:
    data = json.loads(error.read().decode())
    print("Pages request status:", error.code, "message:", data.get("message"))
    if error.code != 409:
        raise SystemExit(1)
