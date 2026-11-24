from pathlib import Path
import sys, json

root = Path(__file__).resolve().parent
sys.path.insert(0, str(root / 'python-libs'))
import UnityPy

source = root.parent / 'wxecf0e106809a477d/game-cache/StreamingAssets'
records = []
for pattern in ['*config*.bundle', '*hotupdate*.bundle']:
    for path in source.glob(pattern):
        env = UnityPy.load(str(path))
        for obj in env.objects:
            if obj.type.name == 'TextAsset':
                data = obj.read()
                name = Path(data.m_Name).name
                output = root / 'bundle-text' / path.stem / name
                output.parent.mkdir(parents=True, exist_ok=True)
                raw = data.m_Script
                if isinstance(raw, str):
                    raw = raw.encode('utf-8', errors='surrogateescape')
                output.write_bytes(raw)
                records.append({'bundle': path.name, 'name': name, 'bytes': len(raw)})
print(json.dumps(records, ensure_ascii=False, indent=2))
(root / 'bundle-text-index.json').write_text(json.dumps(records, ensure_ascii=False, indent=2), encoding='utf-8')
