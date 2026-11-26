from pathlib import Path
import json
import subprocess

root = Path(__file__).resolve().parents[2]
checker = root.parents[2] / 'clis/code-verify/bin/Release/net8.0/CodeVerify.exe'
files = [{'path': str(path.relative_to(root / 'client')).replace('\\', '/'),
          'content': path.read_text(encoding='utf-8-sig')}
         for path in sorted((root / 'client/Assets/Scripts').rglob('*.cs'))
         if 'Config' not in path.relative_to(root / 'client/Assets/Scripts').parts]
failed = False
for mode in ['code', 'architecture']:
    payload = {'files': files, 'mode': mode}
    result = subprocess.run([str(checker)], input=json.dumps(payload), text=True,
                            encoding='utf-8', capture_output=True, check=True)
    report = json.loads(result.stdout)
    (Path(__file__).parent / (mode + '-report.json')).write_text(
        json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    print(json.dumps({'mode':mode, 'issues':report.get('issues', []),
                      'statistics':report.get('statistics')}, ensure_ascii=False))

    failed |= any(issue.get('severity') == 'error' for issue in report.get('issues', []))
raise SystemExit(1 if failed else 0)
