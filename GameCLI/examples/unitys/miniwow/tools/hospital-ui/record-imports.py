"""Verify installed source bytes and retain source asset IDs for the hospital resource ledger."""
from pathlib import Path
import hashlib
import json
import sqlite3

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent
resources = json.loads((OUT/'resource-index.json').read_text(encoding='utf-8'))['resources']
index = {r['png'].replace('\\','/'):r for r in resources if r['png']}
imports = json.loads((OUT/'imports.json').read_text(encoding='utf-8'))
for name in ['UIHomeHospital','UIHomeHospitalEntry']:
    manifest = json.loads((OUT/(name+'.materials.json')).read_text())
    for asset in manifest['assets']:
        source = (OUT/asset['file']).resolve().relative_to(ROOT).as_posix()
        imports.append(dict(source=source,destination=asset['path'],uses=asset['usedBy']))
db = sqlite3.connect(ROOT/'research/resource-catalog/catalog.sqlite')
exports = [json.loads(row[0]) for row in db.execute('SELECT payload FROM exports')]
records = {}
for item in imports:
    source = item['source'].replace('\\','/')
    destination = item['destination'].replace('\\','/')
    original = ROOT/source
    installed = ROOT/'client'/destination
    digest = hashlib.sha256(original.read_bytes()).hexdigest()
    assert digest == hashlib.sha256(installed.read_bytes()).hexdigest(), destination
    asset_id = index[source]['sourceAssetID'] if source in index else next(
        row['asset_id'] for row in exports if row.get('original_layout_path') == source)
    identity = 'hospital-ui-' + hashlib.sha256(destination.encode()).hexdigest()[:20]
    record = records.setdefault(identity,dict(id=identity,sourceAssetId=asset_id,
        source=source,unityPath=destination,sha256=digest,uses=[]))
    record['uses'] = sorted(set(record['uses']+item.get('uses',['Hospital interface dependency'])))
(OUT/'unity-imports.json').write_text(json.dumps(list(records.values()),ensure_ascii=False,indent=2),encoding='utf-8')
print(f'Verified {len(records)} original imports with source asset identities.')
