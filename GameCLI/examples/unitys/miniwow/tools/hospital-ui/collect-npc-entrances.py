"""Record restored residents' original oriented entrance volumes for native scene binding."""
from pathlib import Path
import hashlib
import json
ROOT = Path(__file__).resolve().parents[2]
scene = json.loads((ROOT/'tools/main-scene/scene.json').read_text(encoding='utf8'))
rows = []
for building, npc, title in [('WeaponStore_60002','NPC_forgingtable_2','ForgeTitle'),
                             ('Hotel_60017','NPC_hotel','HotelTitle'),
                             ('AuctionHouse_60019','NPC_auction_2','AuctionTitle')]:
    owner = next(node for node in scene['nodes'] if node['name'] == building)
    collider = next(node for node in scene['nodes'] if node['parent'] == owner['id'] and node['name'] == 'Collider')
    matches = []
    for file in (ROOT/'research/resource-catalog/exports/BoxCollider').glob('*.json'):
        data = json.loads(file.read_text())
        if str(data.get('m_GameObject',{}).get('m_PathID')) == collider['sourceGameObjectId']:
            matches.append((file,data))
    assert len(matches) == 1, building
    file, data = matches[0]
    rows.append(dict(building=building,npc=npc,title=title,source=file.relative_to(ROOT).as_posix(),
                     sha256=hashlib.sha256(file.read_bytes()).hexdigest(),center=data['m_Center'],size=data['m_Size']))
(Path(__file__).parent/'npc-entrances.json').write_text(json.dumps(rows,ensure_ascii=False,indent=2),encoding='utf8')
print('Recorded three original NPC entrance volumes.')
