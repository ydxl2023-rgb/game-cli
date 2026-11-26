"""Apply the same C# conventions to the new native authoring and QA helpers."""
from pathlib import Path
import json
import subprocess

root = Path(__file__).resolve().parents[2]
checker = root.parents[2] / 'clis/code-verify/bin/Release/net8.0/CodeVerify.exe'
files = []
for name in ['MiniwowVillageSetup', 'MiniwowVillageProbe']:
    path = root / f'client/Assets/Editor/{name}.cs'
    files.append({'path': f'Assets/Editor/{name}.cs', 'content': path.read_text(encoding='utf-8-sig')})
result = subprocess.run([str(checker)], input=json.dumps({'mode': 'code', 'files': files}),
                        text=True, encoding='utf-8', capture_output=True, check=True)
report = json.loads(result.stdout)
(root / 'tools/opening/village-editor-code.json').write_text(
    json.dumps(report, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps({'issues': report.get('issues', []), 'statistics': report.get('statistics')}, ensure_ascii=False))
raise SystemExit(any(row.get('severity') == 'error' for row in report.get('issues', [])))
