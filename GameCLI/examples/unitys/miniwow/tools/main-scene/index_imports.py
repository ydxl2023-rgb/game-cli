"""Index native scene imports and their exact source lineage without changing annotations."""
from pathlib import Path
import json,sqlite3,hashlib,collections
ROOT=Path(__file__).resolve().parents[2];OUT=Path(__file__).parent
scene=json.loads((OUT/'scene.json').read_text('utf8'));receipts=json.loads((OUT/'asset-manifest.json').read_text('utf8'))
by={n['id']:n for n in scene['nodes']}
def hierarchy(n):
 return (hierarchy(by[n['parent']])+'/' if n['parent']!='0' else '')+n['name']
usage=collections.defaultdict(list)
for n in scene['nodes']:
 for k in [n.get('mesh'),*n.get('materials',[])]:
  if k:usage[k].append(hierarchy(n))
for m in scene['materials']:
 if m['texture']:usage[m['texture']]+=usage[m['id']]
records=[]
db=sqlite3.connect(ROOT/'research/resource-catalog/catalog.sqlite')
db.execute('CREATE TABLE IF NOT EXISTS scene_imports (import_id TEXT PRIMARY KEY, source_asset_id TEXT, scene TEXT, name TEXT, type TEXT, unity_path TEXT, sha256 TEXT, usage_json TEXT, evidence_json TEXT)')
lines=['# 主场景资源索引','','来源：Building_sdc 和 tbbuilding.InitialModelPath。用途由场景节点引用确认；不代表业务功能已实现。','','| 名称 | 类型 | Unity 文件 | 场景用途 |','|---|---|---|---|']
for r in receipts:
 suffix={'Mesh':'.asset','Material':'.mat','Texture2D':'.png'}[r['type']]
 relative='client/Assets/Data/Scenes/Home/Recovered/'+r['id']+suffix
 path=ROOT/relative
 assert path.exists(),path
 assert db.execute('select 1 from assets where id=?',(r['sourceAssetId'],)).fetchone(),r['sourceAssetId']
 digest=hashlib.sha256(path.read_bytes()).hexdigest();uses=sorted(set(usage[r['id']]))
 record=dict(**r,unityPath=relative,unitySha256=digest,uses=uses)
 records.append(record)
 db.execute('INSERT OR REPLACE INTO scene_imports VALUES(?,?,?,?,?,?,?,?,?)',(r['id'],r['sourceAssetId'],'Home / Building_sdc',r['name'],r['type'],relative,digest,json.dumps(uses,ensure_ascii=False),json.dumps(r,ensure_ascii=False)))
 labels=', '.join(sorted(set(p.split('/')[-1] for p in uses)))
 lines.append(f"| {r['name']} | {r['type']} | [{r['id']}{suffix}](../../{relative}) | {labels} |")
db.commit();db.close()
(OUT/'unity-imports.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),'utf8')
(OUT/'RESOURCE-INDEX.md').write_text('\n'.join(lines)+'\n','utf8')
print(json.dumps(dict(indexed=len(records),sourceIdsValidated=True),ensure_ascii=False))
