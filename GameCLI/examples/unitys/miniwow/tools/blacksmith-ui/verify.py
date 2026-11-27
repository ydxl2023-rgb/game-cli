"""Verify every exported forge value and its immutable recovered-source digest."""
from pathlib import Path
import json,hashlib,argparse
ROOT=Path(__file__).resolve().parents[2]; OUT=Path(__file__).parent
parser=argparse.ArgumentParser();parser.add_argument('--output',type=Path,required=True);args=parser.parse_args()
manifest=json.loads((OUT/'configuration-source.json').read_text(encoding='utf8'))
for source in manifest['sources']:
    assert hashlib.sha256((ROOT/source['path']).read_bytes()).hexdigest()==source['sha256']
tables={}
for sheet in manifest['sheets']:
    data=json.loads((args.output/'json'/f'{sheet["name"]}Config.bytes').read_text(encoding='utf-8-sig'))['data']
    assert len(data)==len(sheet['rows'])
    for row in sheet['rows']:
        for column,value in zip(sheet['columns'],row):
            assert data[str(row[0])][column[0]]==value,(sheet['name'],row[0],column[0])
    tables[sheet['name']]=data
for row in tables['BlacksmithCost'].values():
    assert str(row['RecipeID']) in tables['BlacksmithRecipe']
    assert str(row['ItemID']) in tables['BlacksmithMaterial']
    assert row['Amount']>0
assert tables['BlacksmithSetting']['60002']['RequiredTaskID']==60329
assert tables['BlacksmithSetting']['60002']['RepairCost']==1100
assert tables['BlacksmithHero']['125']['Trade']==0
assert len(tables['BlacksmithRecipe'])==5
common=json.loads((args.output/'json/CommonTextConfig.bytes').read_text(encoding='utf-8-sig'))['data']
language=json.loads((args.output/'json/LanguageConfig.bytes').read_text(encoding='utf-8-sig'))['data']
for id,alias,text in manifest['languages']:
    context=common[str(id)]['Context']
    assert (language[context] if context.isdecimal() else context)==text,alias
report=dict(passed=True,tables={name:len(rows) for name,rows in tables.items()},
    sourceHashes=True,allFieldsRoundTrip=True,allLocalizedTexts=True)
(OUT/'configuration-validation.json').write_text(json.dumps(report,indent=2),encoding='utf8')
print(json.dumps(report))

