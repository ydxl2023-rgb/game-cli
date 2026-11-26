"""Check exported village values against original tables and declared offline adaptations."""
from pathlib import Path
import hashlib
import json

root = Path(__file__).resolve().parents[2]
def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))
def source(name):
    return read(root / f'research/config-study/tables/{name}.json')
def output(name):
    return read(root / f'client/Assets/Data/Jsons/Excels/{name}Config.bytes')['data']

setting = output('VillageSetting')['1']
steps = output('VillageTask')
assert list(map(int, steps)) == [60004, 60101, 60102, 60103, 60104]
assert [r['RequiredTaskID'] for r in steps.values()] == [60003, 60004, 60101, 60102, 60103]
hospital = next(r for r in source('tbbuildinglevel') if r['ID'] == 60018001)
offer = next(r for r in source('tbshop') if r['Id'] == 6001)
potion = next(r for r in source('tbitem') if r['Id'] == 77101)
globals_ = source('tbglobal')[0]
assert setting['BuildingID'] == 60018
assert setting['RepairCost'] == hospital['BuildingUpCost'][0]['Num'] == 280
assert setting['GoldID'] == hospital['BuildingUpCost'][0]['Id'] == offer['Price'][0]['Id']
assert setting['MaterialID'] == offer['Goods']['Id'] == 77001
assert offer['Goods']['Num'] == 1
assert setting['PurchasePrice'] == offer['Price'][0]['Num'] == 30
effect = hospital['Effect']
goods = effect['Goods'][0]
assert setting['PotionID'] == goods['Craft'] == 77101
assert setting['MaterialID'] == goods['Cost']
assert setting['ServicePrice'] == goods['CraftPrice'] == 50
assert setting['UseSeconds'] == potion['UseEffect']['Time'] - effect['CutCdSec'] == 4
assert setting['HealPercent'] == globals_['ResponseEffect'][-len(effect['Goods'])] == 100
assert setting['HeroGold'] == globals_['HeroInitialGold'] == 500000
assert setting['HeroID'] == 125 and setting['InitialHealthPercent'] == 20
assert setting['MaterialCost'] == setting['OutputCount'] == 1
text = output('CommonText')
language = output('Language')
for step in steps.values():
    for field in ['TitleTextID', 'DetailTextID', 'ActionTextID']:
        assert language[text[str(step[field])]['Context']]
evidence = {
    'status': 'passed', 'tasks': list(map(int, steps)),
    'verified': ['repair 280', 'herb purchase 30', 'service price 50, not craft price',
                 'treatment duration 5-1=4', 'first hospital tier recovery 100 percent',
                 'task order, prerequisites and localized fields'],
    'offlineAdaptations': [
        'Starting player gold is inherited from existing Home configuration, not a proven retirement grant.',
        'Normalized starting health 20/100 adapts Story4 presentation; it is not original combat HP.',
        'One herb to one potion, instant production: explicit local recipe; missing server rules.',
        'Hero starting gold adopts tbglobal.HeroInitialGold; normal server initialization is unproven.',
        'UseCostItem is reported after treatment completion locally; original server counting time is unavailable.'
    ],
    'sourceSha256': {name: hashlib.sha256((root / f'research/config-study/tables/{name}.json').read_bytes()).hexdigest()
                     for name in ['tbbuildinglevel', 'tbshop', 'tbitem', 'tbglobal', 'tbtask', 'tbhero']}
}
(root / 'tools/opening/village-config-evidence.json').write_text(
    json.dumps(evidence, ensure_ascii=False, indent=2), encoding='utf-8')
print(json.dumps(evidence, ensure_ascii=False))
