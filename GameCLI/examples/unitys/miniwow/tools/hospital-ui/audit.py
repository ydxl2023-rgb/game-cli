"""Record original hospital layout and resource identities before UGUI authoring."""
from pathlib import Path
import hashlib
import json
import sys

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent
sys.path.insert(0,str(ROOT/'tools/opening'))
from audit_original_ui import package

groups = [('building',['BuildingOpBubble','BuildingEntranceBubble']),
          ('consumable',['ConsumableShopPage','ShopScene','Material','Consumable']),
          ('common',['BuildingPanel'])]
layout = [package(ROOT/f'extracted-resources/assets/data/ui/atlas/{name}/{name}_fui.bytes',names)
          for name,names in groups]
(OUT/'original-ui-audit.json').write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf-8')
selected = {'common_bg_8_0','common_bg_18','hub_bg_7','common_bg_11','shop_bg_1'}
borders = []
for group in layout:
    for item in group['images']:
        if item['name'] in selected:
            x,y,w,h = item['scale9Grid']
            borders.append(dict(name=item['name'],border=[x,item['height']-y-h,item['width']-x-w,y]))
assert len(borders) == len(selected)
(OUT/'slice-borders.json').write_text(json.dumps(borders,indent=2),encoding='utf-8')
identities = {}
by_name = {}
for file in (ROOT/'extracted-resources/fairygui').rglob('*.json'):
    row = json.loads(file.read_text(encoding='utf-8'))
    if 'package_id' in row and 'metadata' in row:
        identities[(row['package_id'],row['metadata']['id'])] = (file,row)
        by_name[f"ui://{row['package_name']}/{row['name']}"] = (file,row)
uses = {}
for entry in layout:
    for component in entry['components']:
        for child in component['children']:
            if child['sourceId']:
                key = (child['packageId'] or entry['packageId'],child['sourceId'])
                uses.setdefault(key,[]).append(f"{entry['package']}/{component['name']}/{child['name']}")
            if child.get('buttonOverrides', [None,None,None])[2]:
                url = child['buttonOverrides'][2]
                uses.setdefault((url[5:13],url[13:]),[]).append(f"{entry['package']}/{component['name']}/{child['name']}/icon")
records = []
for key,owners in uses.items():
    file,row = identities[key]
    records.append(dict(sourceAssetID=row['id'],source=str(file.relative_to(ROOT)),name=row['name'],uses=owners,
                        png=str(file.with_suffix('.png').relative_to(ROOT)) if file.with_suffix('.png').exists() else None,
                        status='recovered reference; not imported or runtime verified'))
source = json.loads((OUT/'configuration-source.json').read_text(encoding='utf-8'))
for item in next(s['rows'] for s in source['sheets'] if s['name']=='HomeBuildingItem'):
    file,row = by_name[item[3]]
    records.append(dict(sourceAssetID=row['id'],source=str(file.relative_to(ROOT)),name=row['name'],
                        uses=[f'HomeBuildingItem/{item[0]}/IconSource'],
                        png=str(file.with_suffix('.png').relative_to(ROOT)),status='recovered; not imported'))
for row in records:
    target = ROOT/(row['png'] or row['source'])
    row['sha256'] = hashlib.sha256(target.read_bytes()).hexdigest()
spine = ROOT/'extracted-resources/assets/data/spines/e_ui_bg_priest_1'
spine_files = [dict(path=str(p.relative_to(ROOT)),sha256=hashlib.sha256(p.read_bytes()).hexdigest())
               for p in spine.rglob('*') if p.is_file() and '_subassets' not in p.parts]
(OUT/'resource-index.json').write_text(json.dumps(dict(resources=records,spine=spine_files,
    limitation='Direct authored references only; nested components, controller gears and Spine conversion remain UI work.'),
    ensure_ascii=False,indent=2),encoding='utf-8')
print(f'Audited {sum(len(p["components"]) for p in layout)} components, {len(records)} direct references.')
