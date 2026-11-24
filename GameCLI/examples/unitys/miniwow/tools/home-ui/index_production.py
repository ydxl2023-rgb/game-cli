"""Index production outputs without changing existing asset annotations."""
from pathlib import Path
import json,sqlite3
ROOT=Path(__file__).resolve().parents[2]
items=json.loads((Path(__file__).parent/'production-resources.json').read_text('utf8'))
db=sqlite3.connect(ROOT/'research/resource-catalog/catalog.sqlite')
db.execute('CREATE TABLE IF NOT EXISTS home_ui_production_assets (resource_key TEXT PRIMARY KEY, source_asset_id TEXT, source_path TEXT, source_sha256 TEXT, unity_path TEXT, output_sha256 TEXT, conversion_json TEXT)')
for x in items:
 db.execute('INSERT INTO home_ui_production_assets VALUES(?,?,?,?,?,?,?) ON CONFLICT(resource_key) DO UPDATE SET source_asset_id=excluded.source_asset_id,source_path=excluded.source_path,source_sha256=excluded.source_sha256,unity_path=excluded.unity_path,output_sha256=excluded.output_sha256,conversion_json=excluded.conversion_json',(x['key'],x['sourceAssetId'],x['source'],x['sourceSha256'],x['destination'],x['sha256'],json.dumps(x,ensure_ascii=False)))
db.commit();print('Indexed production resources:',len(items));db.close()
