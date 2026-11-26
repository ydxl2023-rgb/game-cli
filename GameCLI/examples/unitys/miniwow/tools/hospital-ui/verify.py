"""Verify e2cs output against original tables, including unresolved source evidence."""
from pathlib import Path
import argparse
import hashlib
import json

ROOT = Path(__file__).resolve().parents[2]
DIRECTORY = Path(__file__).resolve().parent


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    manifest = json.loads((DIRECTORY/'configuration-source.json').read_text(encoding='utf-8'))
    actual = {}
    for source in manifest['sources']:
        assert hashlib.sha256((ROOT/source['path']).read_bytes()).hexdigest() == source['sha256'], source['path']
    for sheet in manifest['sheets']:
        data = json.loads((args.output/'json'/f'{sheet["name"]}Config.bytes').read_text(encoding='utf-8-sig'))['data']
        assert len(data) == len(sheet['rows']), sheet['name']
        for row in sheet['rows']:
            exported = data[str(row[0])]
            for column, value in zip(sheet['columns'], row):
                assert exported[column[0]] == value, (sheet['name'], row[0], column[0])
        actual[sheet['name']] = data
    level = actual['HomeBuildingLevel']
    items = actual['HomeBuildingItem']
    for row in actual['HomeBuildingCost'].values():
        assert str(row['LevelID']) in level and str(row['ItemID']) in items
    for row in actual['HomeBuildingGoods'].values():
        assert str(row['LevelID']) in level and str(row['MaterialID']) in items and str(row['ProductID']) in items
    for row in level.values():
        assert row['ID'] == row['BuildingID']*1000+row['Level']
    source = ROOT/'research/config-study/tables'
    original = [r for r in json.loads((source/'tbbuildinglevel.json').read_text()) if r['ID']//1000==60018]
    assert len(level) == len(original) == 200
    response = json.loads((source/'tbglobal.json').read_text())[0]['ResponseEffect']
    failures = []
    for row in original:
        goods = sorted((g for g in actual['HomeBuildingGoods'].values() if g['LevelID']==row['ID']),
                       key=lambda g:g['Order'])
        assert len(goods) == len(row['Effect']['Goods'])
        for index,(product,raw) in enumerate(zip(goods,row['Effect']['Goods'])):
            assert (product['MaterialID'],product['ProductID'],product['ServiceBasePrice']) == (
                raw['Cost'],raw['Craft'],raw['CraftPrice'])
            position = len(response)-len(goods)+index
            if position < 0:
                failures.append(product['ID'])
                assert product['RecoveryPercent']==-1
            else:
                assert product['RecoveryPercent']==response[position]
    assert failures == [r['goodsID'] for r in manifest['unresolved']]
    first = actual['HomeBuildingGoods']['600180011']
    assert first['RecoveryPercent']==100 and first['ServiceBasePrice']==50
    assert items['77101']['UseSeconds']-level['60018001']['CutSeconds']==4
    assert actual['HomeBuildingCost']['600180011']['Amount']==280
    feature = actual['HomeBuildingCondition']['10101']
    assert (feature['OwnerKind'],feature['OwnerID'],feature['Type'],feature['Value'])==(1,1010,6,60004)
    common = json.loads((args.output/'json/CommonTextConfig.bytes').read_text(encoding='utf-8-sig'))['data']
    languages = json.loads((args.output/'json/LanguageConfig.bytes').read_text(encoding='utf-8-sig'))['data']
    for id,alias,text in manifest['languages']:
        assert str(id) in common, alias
        assert languages[common[str(id)]['Context']] == text, alias
    report = dict(exportStatus='passed',scope='configuration-only',
                  tables={name:len(rows) for name,rows in actual.items()},
                  firstLevel=dict(repairGold=280,serviceGold=50,useSeconds=4,recoveryPercent=100),
                  unresolvedOriginalGoods=len(failures),
                  limitation='Level 51-200 first good has negative original ResponseEffect index; not executable.')
    (DIRECTORY/'configuration-validation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2),encoding='utf-8')
    print(json.dumps(report,ensure_ascii=False))


if __name__ == '__main__':
    main()
