from pathlib import Path
import json,hashlib
r=Path(__file__).resolve().parents[2];records=[]
for name in ['hotel','hospital-burned','hospital-rescued']:
 for m in json.loads((r/f'tools/main-scene/{name}/asset-manifest.json').read_text(encoding='utf8')):
  suffix={'Mesh':'.asset','Material':'.mat','Texture2D':'.png'}[m['type']]
  dest=f'Assets/Data/Scenes/Home/Opening/{name}/'+m['id']+suffix
  records.append(dict(m,id='second-rescue:'+name+':'+m['id'],unityPath=dest,unitySha256=hashlib.sha256((r/'client'/dest).read_bytes()).hexdigest(),uses=[f'Task 60002 original {name} model dependency']))
manifest=json.loads((r/'extracted-resources/path-manifest.json').read_text(encoding='utf8'))['files']
p='extracted-resources/fairygui/CharacterIcon/NPC/Conversation_hotel.png';found=[x for x in manifest if x['restored_path']==p];m=found[0]
dest='Assets/Data/Widgets/Common/Icons/Conversation_hotel.png'
records.append(dict(id='second-rescue:hotel-portrait',sourceAssetId=m['asset_id'],source=p,originalPath=m['original_path'],pathBasis=m['path_basis'],sourceSha256=m['sha256'],unityPath=dest,unitySha256=hashlib.sha256((r/'client'/dest).read_bytes()).hexdigest(),uses=['Temporary NPC icon portrait for original dialogue 3053; full-body Conversation_hotel_2 presentation remains pending']))
(r/'tools/opening/second-imports.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf8');print('indexed',len(records))
