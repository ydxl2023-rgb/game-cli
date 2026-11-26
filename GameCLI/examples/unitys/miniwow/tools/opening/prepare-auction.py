from pathlib import Path
import json,hashlib,shutil
root=Path(__file__).resolve().parents[2]
source=root/'extracted-resources/assets/data/spines/npc_auction_2'
target=root/'client/Assets/Data/Scenes/Town/Spine/npc_auction_2';target.mkdir(parents=True,exist_ok=True)
manifest=json.loads((root/'extracted-resources/path-manifest.json').read_text(encoding='utf8'))['files']
records=[]
for suffix in ['.png','.atlas.txt','.skel.bytes']:
 p=source/('npc_auction_2'+suffix); dest=target/p.name;shutil.copyfile(p,dest)
 m=next(x for x in manifest if x['restored_path']==p.relative_to(root).as_posix())
 records.append(dict(id='auction-merchant:'+suffix,sourceAssetId=m['asset_id'],source=p.relative_to(root).as_posix(),unityPath=dest.relative_to(root/'client').as_posix(),sha256=hashlib.sha256(dest.read_bytes()).hexdigest(),uses=['Actor 5: merchant revealed after task 60001 rescue, original NPC_auction_2 anchor']))
for m in json.loads((root/'tools/main-scene/auction/asset-manifest.json').read_text(encoding='utf8')):
 suffix={'Mesh':'.asset','Material':'.mat','Texture2D':'.png'}[m['type']]
 records.append(dict(m,id='auction-damaged:'+m['id'],unityPath='Assets/Data/Scenes/Home/Opening/Auction/'+m['id']+suffix,uses=['Original auction_initial_1, before rescue; replaces existing auction_initial until HomeOpening.Thanks']))
for record in records:
 native=root/'client'/record['unityPath']
 if native.exists():record['unitySha256']=hashlib.sha256(native.read_bytes()).hexdigest()
 if record['id'].startswith('auction-merchant:'):record['sourceSha256']=hashlib.sha256((root/record['source']).read_bytes()).hexdigest()
(root/'tools/opening/auction-imports.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf8')
