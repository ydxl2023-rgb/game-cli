"""Index selected original hospital dependencies and the verified Garen portrait."""
from pathlib import Path
import json
import hashlib

root = Path(__file__).resolve().parents[2]
records = []
def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()
for item in json.loads((root / 'tools/main-scene/hospital-built/asset-manifest.json').read_text(encoding='utf-8')):
    suffix = {'Mesh': '.asset', 'Material': '.mat', 'Texture2D': '.png'}[item['type']]
    target = 'Assets/Data/Scenes/Home/Opening/hospital-built/' + item['id'] + suffix
    records.append(dict(item, id='village:hospital-built:' + item['id'], unityPath=target,
                        unitySha256=digest(root / 'client' / target),
                        uses=['Task 60101: original priest/priest constructed hospital; not extinguished ruins']))
source = 'extracted-resources/fairygui/CharacterIcon/Hero/hero_20025.png'
metadata = json.loads((root / source).with_suffix('.json').read_text(encoding='utf-8'))
target = 'Assets/Data/Widgets/Common/Icons/GarenPortrait.png'
records.append(dict(id='village:garen-portrait', sourceAssetId=metadata['id'], source=source,
                    originalPath='ui://CharacterIcon/hero_20025', pathBasis='declared FairyGUI package item',
                    sourceSha256=digest(root / source), unityPath=target,
                    unitySha256=digest(root / 'client' / target),
                    uses=['Garen recruitment and treatment portrait; local substitute for missing FullBody variant'],
                    limitations=['FullBody/Conversation_hero_20025_2 is not in the recovered files.',
                                 'Conversation_hero_20025 was visually rejected because it depicts another character.']))
(root / 'tools/opening/village-imports.json').write_text(
    json.dumps(records, ensure_ascii=False, indent=2), encoding='utf-8')
print('Indexed', len(records), 'selected original resource dependencies; no binaries uploaded.')
