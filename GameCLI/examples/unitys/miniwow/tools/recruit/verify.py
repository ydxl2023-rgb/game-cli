"""Check source-to-export fidelity and recruitment prerequisites before installing tables."""
import argparse
import json
from pathlib import Path

parser = argparse.ArgumentParser()
parser.add_argument('--output', required=True, type=Path)
args = parser.parse_args()
root = Path(__file__).resolve().parents[2]
source = json.loads((Path(__file__).parent / 'configuration-source.json').read_text(encoding='utf8'))
tables = {}
for sheet in source['sheets']:
    data = json.loads((args.output / 'json' / (sheet['name'] + 'Config.bytes')).read_text(encoding='utf8'))['data']
    tables[sheet['name']] = data
    assert len(data) == len(sheet['rows'])
    for row in sheet['rows']:
        for column, value in zip(sheet['columns'],row):
            assert data[str(row[0])][column[0]] == value, (sheet['name'],row[0],column[0])
tasks = json.loads((args.output / 'json/TaskConfig.bytes').read_text(encoding='utf8'))['data']
groups = {row['GroupID'] for row in tables['RecruitDialogue'].values()}
for row in tables['RecruitStep'].values():
    assert str(row['ID']) in tasks and str(row['RequiredTaskID']) in tasks
    assert row['DialogueGroupID'] in groups
    assert row['Kind'] in [1,2,3]
    if row['Kind'] == 3:
        assert str(row['HeroID']) in tables['RecruitHero']
        assert tasks[str(row['ID'])]['Param1'] == row['HeroID']
print(json.dumps({'passed':True,'tables':{key:len(value) for key,value in tables.items()}}))
