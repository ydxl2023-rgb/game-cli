"""Inventory reference source and detect drift; never change the reference project."""
import argparse
import hashlib
import json
import os
import re
from pathlib import Path

HERE = Path(__file__).resolve().parent
ROOT = HERE.parents[3] / 'examples/unitys/development'
SKIP = {'Library', 'Temp', 'Logs', 'obj', 'bin', '.git', 'node_modules', 'LocalMcps'}
EXTENSIONS = {'.cs', '.py', '.ps1', '.js', '.mjs', '.ts', '.shader', '.hlsl', '.cginc', '.bat', '.cmd', '.sh', '.lua', '.compute'}


def category(path):
    if '/Plugins/' in path or '/com.unity.' in path or '/TextMesh Pro/' in path:
        return 'third-party'
    if path.startswith(('client/Assets/Scripts/Config/', 'config/data/', 'server/http/server/resmng/config/')):
        return 'generated-config'
    if path.startswith('client/Assets/Scripts/'):
        return 'application'
    if path.startswith('client/LocalPackages/com.oathx.'):
        return 'framework'
    if path.startswith('server/'):
        return 'server'
    return 'authoring-tool'


def inventory():
    result = []
    for base, dirs, files in os.walk(ROOT):
        dirs[:] = sorted(d for d in dirs if d not in SKIP)
        for name in sorted(files):
            path = Path(base) / name
            if path.suffix.lower() not in EXTENSIONS:
                continue
            data = path.read_bytes()
            encoding = 'utf-8-sig'
            try:
                source = data.decode(encoding)
            except UnicodeDecodeError:
                encoding = 'utf-16' if data.startswith((b'\xff\xfe', b'\xfe\xff')) else 'gb18030'
                source = data.decode(encoding)
            relative = path.relative_to(ROOT).as_posix()
            declarations = []
            for line, text in enumerate(source.splitlines(), 1):
                if re.search(r'\b(class|interface|enum|struct)\s+\w+', text) and not text.lstrip().startswith('//'):
                    declarations.append({'line': line, 'text': text.strip()})
            result.append({'path': relative, 'category': category(relative),
                           'sha256': hashlib.sha256(data).hexdigest(), 'encoding': encoding,
                           'lines': len(source.splitlines()), 'declarations': declarations})
    return sorted(result, key=lambda item: item['path'])


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--write', action='store_true', help='Update baseline only after source review and documentation updates.')
    args = parser.parse_args()
    current = inventory()
    manifest = HERE / 'source-index.json'
    if args.write:
        manifest.write_text(json.dumps({'schemaVersion': 1, 'referenceRoot': 'examples/unitys/development',
                                       'date': '2026-11-25', 'excludedDirectories': sorted(SKIP),
                                       'note': 'Inventory and declaration locations are not evidence of full implementation review.',
                                       'files': current}, ensure_ascii=False, indent=2) + '\n', 'utf8')
        rows = ['# Development 源码目录索引', '',
                '自动生成的路径、类别及定位信息；阅读深度以 COVERAGE.md 为准。', '',
                '|文件|分类|行数|', '|---|---|---|']
        for item in current:
            target = '../../../../examples/unitys/development/' + item['path']
            rows.append(f"|[{item['path']}]({target})|{item['category']}|{item['lines']}|")
        (HERE / 'SOURCE-INDEX.md').write_text('\n'.join(rows) + '\n', 'utf8')
        print(json.dumps({'files': len(current), 'lines': sum(item['lines'] for item in current)}))
        return
    previous = {item['path']: item['sha256'] for item in json.loads(manifest.read_text('utf8'))['files']}
    actual = {item['path']: item['sha256'] for item in current}
    changed = sorted(path for path in previous.keys() | actual.keys() if previous.get(path) != actual.get(path))
    print(json.dumps({'status': 'stale' if changed else 'current', 'files': len(current), 'changed': changed}))
    raise SystemExit(1 if changed else 0)


if __name__ == '__main__':
    main()
