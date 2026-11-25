"""Restore atlas trim frames without resampling source pixels; update resource provenance."""
from pathlib import Path
import hashlib
import json
import sqlite3
import sys

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / 'research/python-libs'))
from PIL import Image

OUT = Path(__file__).parent
records = json.loads((OUT / 'resources.json').read_text('utf-8'))
frames = [(1, (214, 214), (0, 0)), (2, (214, 214), (70, 70)), (12, (37, 23), (2, 2))]
arrow_id = '70c957f799657c40487e66f692782ff0e2a1121bed7e22a5848e8987c27b6382'
if not any(r['source'].endswith('common_bg_12.png') for r in records):
    records.append(dict(source='extracted-resources/fairygui/Main/images/common_bg_12.png', destination='Assets/Data/Widgets/Actor/JoystickAssets/common_bg_12.png', target='client/Assets/Data/Widgets/Actor/JoystickAssets/common_bg_12.png', purpose='Original player direction arrow', sourceAssetIds=[arrow_id], sourceUiUrl='ui://1pyb9685bmzw4w'))
digest = lambda p: hashlib.sha256(p.read_bytes()).hexdigest()
for number, size, offset in frames:
    name = f'common_bg_{number}.png'
    record = next(r for r in records if r['source'].endswith(name))
    source = ROOT / record['source']
    pixels = Image.open(source).convert('RGBA')
    restored = Image.new('RGBA', size)
    restored.paste(pixels, offset)
    assert restored.crop((*offset, offset[0] + pixels.width, offset[1] + pixels.height)).tobytes() == pixels.tobytes()
    path = OUT / 'original-ui' / name
    path.parent.mkdir(exist_ok=True)
    restored.save(path)
    assert Image.open(ROOT / record['target']).convert('RGBA').tobytes() == restored.tobytes(), 'Import restored PNG through UGUI before indexing'
    record.update(sha256=digest(source), targetSha256=digest(ROOT / record['target']), conversion='Restore original FairyGUI atlas trim frame; no resampling or pixel modification.', originalSize=list(size), trimOffset=list(offset), packedSize=list(pixels.size), pixelsVerified=True)
nav_source = 'extracted-resources/assets/data/scenes/building_sdc/building_sdc/navmesh-mesh.asset.json'
nav_target = 'client/Assets/Data/Scenes/Town/OriginalTownNavigation.asset'
records = [r for r in records if r['target'] != nav_target]
records.append(dict(source=nav_source, target=nav_target, destination=nav_target.removeprefix('client/'), purpose='Original town Hero navigation, 30 unchanged tiles', sourceAssetIds=['b91da2e85b4f2fe39895830999b17d57914adc5cb70795f3ae2d627d3f2401dc'], sha256=digest(ROOT / nav_source), targetSha256=digest(ROOT / nav_target), conversion='MiniwowOriginalNavigation native serialization; each mesh tile and 16-byte hash verified against source.', verification='tools/player/original-navigation-import.json'))
(OUT / 'resources.json').write_text(json.dumps(records, ensure_ascii=False, indent=2) + '\n', 'utf-8')
with sqlite3.connect(ROOT / 'research/resource-catalog/catalog.sqlite') as db:
    for record in records:
        db.execute('INSERT OR REPLACE INTO town_imports(target_path,payload) VALUES(?,?)', (record['target'], json.dumps(record, ensure_ascii=False)))
print(f'Indexed {len(records)} source-to-target records; restored pixels verified.')
