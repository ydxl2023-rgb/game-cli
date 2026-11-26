from pathlib import Path
import json,hashlib
r=Path(__file__).resolve().parents[2];records=[]
for name in ['hospital-cleared']:
 for m in json.loads((r/f'tools/main-scene/{name}/asset-manifest.json').read_text(encoding='utf8')):
  suffix={'Mesh':'.asset','Material':'.mat','Texture2D':'.png'}[m['type']]
  dest=f'Assets/Data/Scenes/Home/Opening/{name}/'+m['id']+suffix
  records.append(dict(m,id='third-rescue:'+name+':'+m['id'],unityPath=dest,unitySha256=hashlib.sha256((r/'client'/dest).read_bytes()).hexdigest(),uses=[f'Task 60003 original {name} model dependency']))
manifest=json.loads((r/'extracted-resources/path-manifest.json').read_text(encoding='utf8'))['files']
p='extracted-resources/assets/data/effects/textures/ef_tex_smoke_01.png';found=[x for x in manifest if x['restored_path']==p];m=found[0]
dest='Assets/Data/Scenes/Home/Opening/hospital-cleared/smoke.png'
records.append(dict(id='third-rescue:smoke-texture',sourceAssetId=m['asset_id'],source=p,originalPath=m['original_path'],pathBasis=m['path_basis'],sourceSha256=m['sha256'],unityPath=dest,unitySha256=hashlib.sha256((r/'client'/dest).read_bytes()).hexdigest(),uses=['Original smoke texture used by a native local clearing-dust adaptation; not the original e_jianzhu_jiesuo_priest_smoke_1 animation']))
(r/'tools/opening/third-imports.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf8');print('indexed',len(records))
