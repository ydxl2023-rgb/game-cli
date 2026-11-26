"""Verify e2cs task output against all recovered source fields before installation."""
import argparse
import collections
import hashlib
import json
from pathlib import Path
import re

ROOT = Path(__file__).resolve().parents[2]
parser = argparse.ArgumentParser()
parser.add_argument('--output', required=True, type=Path)
args = parser.parse_args()


def read(path):
    return json.loads(path.read_text(encoding='utf-8-sig'))


def source(name):
    return read(ROOT / 'research/config-study/tables' / (name + '.json'))


def output(name):
    document = read(args.output / 'json' / (name + 'Config.bytes'))
    assert document['name'] == name[0].lower() + name[1:] + 'Config', name
    return document['data']


enum_source = (args.output / 'csharp/client/ExcelEnumDefine.cs').read_text(encoding='utf-8-sig')
enums = {}
for name, body in re.findall(r'public enum (\w+)\s*\{([^}]+)\}', enum_source):
    pairs = re.findall(r'(\w+)\s*=\s*(-?\d+)', body)
    assert len(set(value for _, value in pairs)) == len(pairs), name
    enums[name] = {int(value): key for key, value in pairs}


def enum(field, value):
    return enums['E' + field][value]


languages = output('Language')


def text(value, expected):
    return languages[value] if re.search('[\u4e00-\u9fff]', expected) else value


def pairs(values):
    return [{'Id': value['Id'], 'Num': value['Num']} for value in values]


tables = {name: output(name) for name in ['Task','TaskChapter','TaskUnlock','TaskGuide']}
for name in tables:
    generated_code = (args.output / 'csharp/client' / (name + 'Config.cs')).read_text(encoding='utf-8-sig')
    assert not re.search(r'public const int TASK_', generated_code), name + ': use numeric IDs, not aliases'
    for key, row in tables[name].items():
        assert type(row['ID']) is int and row['ID'] == int(key) == row['NumID'], (name, key)

for name, original, key in [('Task','tbtask','TaskID'),('TaskChapter','tbchaptertask','ID'),
                            ('TaskUnlock','tbtaskunlock','ID'),('TaskGuide','tbguidetask','ID')]:
    assert set(tables[name]) == {str(row[key]) for row in source(original)}, name

for original in source('tbtask'):
    row = tables['Task'][str(original['TaskID'])]
    assert row['ID'] == row['NumID'] == original['TaskID']
    for target, field in [('Name','TaskName'),('Description','TaskDescription'),('Icon','Icon')]:
        assert text(row[target], original[field]) == original[field], (row['ID'], field)
    for field in ['FinishValue','ShowTips','QuestValue','NPCTalkID','Priority','TipsDelayTime',
                  'GuideConversationDelayTime','GuideConversationID']:
        assert row[field] == original[field], (row['ID'], field)
    assert row['GuideID'] == original['JumpType']
    assert pairs(row['Rewards']) == pairs(original['Reward'])
    for field, source_field, numeric in [('TaskConditionType','FinishConditionType','SourceConditionType'),
                                       ('TaskBuildingType','BuildingType','SourceBuildingType')]:
        assert enum(field, row[field]) == original[source_field]['name']
        assert row[numeric] == original[source_field]['value']
    condition = original['FinishConditionValues']
    assert enum('TaskConditionShape',row['TaskConditionShape']) == condition['$type'].split('.')[-1]
    for field in ['Param1','Param2','Param3']:
        assert row[field] == condition.get(field,0)
    assert pairs(row['ConditionItems']) == pairs(condition.get('List',[]))

for original in source('tbchaptertask'):
    assert tables['TaskChapter'][str(original['ID'])]['ID'] == original['ID']
    assert tables['TaskChapter'][str(original['ID'])]['TaskIDs'] == original['TaskId']

for original in source('tbtaskunlock'):
    row = tables['TaskUnlock'][str(original['ID'])]
    assert row['TaskID'] == original['TaskId']
    assert enum('TaskCategory', row['TaskCategory']) == original['TaskType']['name']
    assert row['SourceTaskType'] == original['TaskType']['value']
    assert row['Conditions'] == [{'Type': c['Type']['value'], 'Value': c['Value']} for c in original['UnlockCondition']]
    assert pairs(row['TriggerRewards']) == pairs(original['TriggerReward'])

manifest = read(ROOT/'tools/task-config/manifest.json')
for original in source('tbguidetask'):
    row = tables['TaskGuide'][str(original['ID'])]
    for field in ['GroupID','NeedSave','NextID']:
        assert row[field] == original[field], (row['ID'],field)
    assert row['FailureNextID'] == original['FNextID']
    assert enum('TaskGuideType',row['TaskGuideType']) == original['GuideType']['name']
    assert row['SourceGuideType'] == original['GuideType']['value']
    assert isinstance(row['ConditionExpression'], str) and row['ConditionExpression'].strip(), row['ID']
    command = {'Navigation': 'Navigate', 'Conversation': 'ShowDialogue'}.get(original['Action'], original['Action'])
    assert row['ActionExpression'].startswith(command + '(')
    assert 'TaskGuideAction' not in row

assert 'ETaskGuideAction' not in enums
assert not (args.output/'json/TaskGuideParamConfig.bytes').exists()
tips = [row['ActionParam'][0] for row in source('tbguidetask') if row['Action']=='Tips']
common = output('CommonText')[str(manifest['tipId'])]
assert languages[common['Context']] == tips[0]
retired_language = []
old_language = ROOT / 'client/Assets/Data/Jsons/Excels/LanguageConfig.bytes'
if old_language.exists():
    for key,value in read(old_language)['data'].items():
        if languages.get(key) != value:
            assert value in tips and key not in languages, ('Existing language changed',key)
            retired_language.append(key)

missing = []
def ref(owner, field, value, target):
    if value > 0 and str(value) not in tables[target]:
        missing.append({'owner':owner,'field':field,'target':target,'id':value})
for row in tables['Task'].values(): ref(row['ID'],'GuideID',row['GuideID'],'TaskGuide')
for row in tables['TaskChapter'].values():
    for value in row['TaskIDs']: ref(row['ID'],'TaskIDs',value,'Task')
for row in tables['TaskUnlock'].values(): ref(row['ID'],'TaskID',row['TaskID'],'Task')
for row in tables['TaskGuide'].values():
    for field in ['NextID','FailureNextID']: ref(row['ID'],field,row[field],'TaskGuide')

report = {'counts':{name:len(rows) for name,rows in tables.items()},
          'sourceFieldRoundTrip':'passed except actions verified by expression runner','enumValuesUnique':True,
          'retiredTipLanguageKeys':retired_language,'existingNonTipLanguagePreserved':True,
          'unresolvedInternalReferences':missing,
          'outputHashes':{p.name:hashlib.sha256(p.read_bytes()).hexdigest()
                          for p in sorted((args.output/'json').glob('*Config.bytes'))}}
(ROOT/'tools/task-config/validation.json').write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
print(json.dumps({k:v for k,v in report.items() if k!='outputHashes'},ensure_ascii=False))
