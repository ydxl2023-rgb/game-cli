"""Apply the existing GameCLI rules directly to package sources otherwise excluded by project scans."""
from pathlib import Path
import json
import subprocess

root = Path(__file__).resolve().parents[2]
package = root / 'client/LocalPackages/com.oathx.expression/Runtime'
checker = root.parents[2] / 'clis/code-verify/bin/Release/net8.0/CodeVerify.exe'
payload = {'files': [{'path': str(p), 'content': p.read_text(encoding='utf-8')}
                     for p in sorted(package.glob('*.cs'))]}
process = subprocess.run([str(checker)], input=json.dumps(payload), text=True,
                         encoding='utf-8', capture_output=True, check=True)
result = json.loads(process.stdout)
Path(__file__).with_name('style-report.json').write_text(json.dumps(result, indent=2) + '\n', encoding='utf-8')
issues = result['issues']
print(json.dumps({'policy': result['policy'], 'files': len(payload['files']), 'issues': issues}))
raise SystemExit(1 if any(issue['severity'] == 'error' for issue in issues) else 0)
