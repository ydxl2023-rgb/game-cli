"""Index decoded original data, preserve provenance, and audit explicit relationships."""
from pathlib import Path
import hashlib
import json
import re
import sqlite3
from collections import Counter, defaultdict

ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / 'research/config-study'
SOURCE = ROOT / 'research/code-study/managed/decompiled/HotUpdate'


def read(path):
    return json.loads(path.read_text(encoding='utf-8'))


def numeric(value):
    return value.get('value') if isinstance(value, dict) else value


def category(name):
    if name.startswith(('tbheroskill', 'tbskill', 'tbbuff')):
        return '技能与表现'
    if name.startswith(('tbbattle', 'tbmonster', 'tbdungeon', 'tbadventure', 'tbexploration', 'tbrankpvp', 'tbmysticrealm')):
        return '战斗与探索'
    if name.startswith(('tbhero', 'tbattribute', 'tbtotalattribute', 'tbequipment', 'tbrelic', 'tbpet', 'tbbook', 'tbtotem', 'tbaura', 'tbbless', 'tbsoldier', 'tbstatue')):
        return '角色与成长'
    if name.startswith(('tbactivity', 'tbactivepass', 'tbpay', 'tbfirstrecharge', 'tbcontinuerecharge', 'tbfund', 'tbday3', 'tbprivilege', 'tblucky', 'tbmagicnight', 'tbpushgift', 'tbnewserver')):
        return '活动与商业化'
    return '系统与经济'


inventory = read(OUT / 'inventory.json')
data = {t['name']: read(OUT / 'tables' / (t['name'] + '.json')) for t in inventory['tables']}
keys = {}
for table in inventory['tables']:
    name = table['name']
    table_source = next(p for p in (SOURCE / 'Zombie.Table').glob('Tb*.cs') if p.stem.lower() == name)
    source_text = table_source.read_text(encoding='utf-8-sig')
    match = re.search(r'_dataMap.Add\(\w+\.(\w+),', source_text)
    keys[name] = match[1] if match else None
    table['keyField'] = keys[name]
    table['category'] = category(name)
    table['readerSource'] = str(table_source.relative_to(ROOT)).replace('\\', '/')

lookups = {name: {numeric(row[keys[name]]): row for row in rows} for name, rows in data.items() if keys[name]}
references = []


def relation(source, target, field, values, evidence, optional=False):
    for row in data[source]:
        for value in values(row):
            value = numeric(value)
            if optional and value in (None, 0, -1):
                continue
            references.append(dict(source=source, sourceKey=numeric(row[keys[source]]), field=field,
                                   target=target, targetKey=value, exists=value in lookups[target], evidence=evidence))


relation('tbhero', 'tbheroskill', 'RecommendedSkills', lambda r: r['RecommendedSkills'], 'CfgHero.RecommendedSkills; HeroData.SetData')
relation('tbheroskill', 'tbheroskilleffect', 'Effects', lambda r: r['Effects'], 'CfgHeroSkill.ResolveRef')
relation('tbheroskill', 'tbheroskill', 'NextLevelId', lambda r: [r['NextLevelId']], 'CfgHeroSkill.NextLevelId', True)
relation('tbheroskill', 'tbskillplay', 'PlayId', lambda r: [r['PlayId']], 'BattleCharacterEntity.GetSkillPlay', True)
relation('tbhero', 'tbskillplay', 'AttackPlayId', lambda r: [r['AttackPlayId']], 'BattleCharacterEntity.GetNormalPlayId', True)
relation('tbhero', 'tbherolevel', 'LevelGroup*1000+level', lambda r: [r['LevelGroup']*1000+i for i in range(1,r['MaxLevel']+1)], 'HeroData.LevelCfgId')
relation('tbhero', 'tbherostar', 'StarGroup*1000+star', lambda r: [r['StarGroup']*1000+i for i in range(r['InitStar'],r['MaxStar']+1)], 'HeroData.StarCfgId')
relation('tbherostarmaptoskilllist', 'tbheroskill', 'StarMapToSkillList', lambda r: [v for a in r['StarMapToSkillList'].values() for v in a], 'HeroProxy.GetSkillUnlockStar')
relation('tbbattlestage', 'tbbattlearea', 'UnlockArea', lambda r: r['UnlockArea'], 'CfgBattleStage.UnlockArea')
relation('tbbattle', 'tbmonstergroup', 'NormalMonsterGroup', lambda r: r['NormalMonsterGroup'], 'CfgBattle.NormalMonsterGroup')
relation('tbbattle', 'tbmonstergroup', 'EliteMonsterGroup', lambda r: r['EliteMonsterGroup'], 'CfgBattle.EliteMonsterGroup')
relation('tbmonstergroup', 'tbmonster', 'Monster.Id', lambda r: [x['Id'] for x in r['Monster']], 'CfgMonsterGroup.Monster')
relation('tbmainfunctions', 'tbfeatureunlock', 'Feature', lambda r: [r['Feature']], 'CfgMainFunctions.Feature', True)
relation('tbbuilding', 'tbfeatureunlock', 'Feature', lambda r: [r['Feature']], 'CfgBuilding.Feature', True)
relation('tbherolevel', 'tbbuilding', 'NeedBuilding.Type', lambda r: [r['NeedBuilding']['Type']] if r['NeedBuilding'] else [], 'CfgHeroLevel.NeedBuilding (candidate relationship)', True)

missing = [r for r in references if not r['exists']]
consumers = defaultdict(list)
for path in SOURCE.rglob('*.cs'):
    if path.parent.name.startswith('Zombie.Table'):
        continue
    text = path.read_text(encoding='utf-8-sig')
    used = set(re.findall(r'\bTb\w+\b', text))
    for token in used:
        name = token.lower()
        if name not in data: continue
        for number,line in enumerate(text.splitlines(),1):
            if re.search(r'\b'+token+r'\b',line):
                consumers[name].append(dict(path=str(path.relative_to(ROOT)).replace('\\','/'),line=number,text=line.strip()))

provenance = {}
for path in sorted(SOURCE.glob('Zombie.Table*/*.cs')):
    provenance[str(path.relative_to(ROOT)).replace('\\','/')] = hashlib.sha256(path.read_bytes()).hexdigest()
for path in [ROOT/'extracted-resources/assets/data/config/confighead.txt',ROOT/'extracted-resources/assets/data/config/configs.bytes', ROOT/'research/code-study/managed/input/hotupdate.dll']:
    provenance[str(path.relative_to(ROOT)).replace('\\','/')] = hashlib.sha256(path.read_bytes()).hexdigest()
packed=(ROOT/'extracted-resources/assets/data/config/configs.bytes').read_bytes()
end=0
for t in inventory['tables']:
    assert t['offset']==end and t['consumed']==t['size'], t['name']
    chunk=packed[t['offset']:t['offset']+t['size']]
    assert chunk==(ROOT/'extracted-resources/config-tables'/f"{t['name']}.bytes").read_bytes(),t['name']
    assert hashlib.sha256(chunk).hexdigest()==t['sha256']
    end+=t['size']
assert end==len(packed)

db=sqlite3.connect(OUT/'configs.sqlite')
db.executescript('''DROP TABLE IF EXISTS config_tables; DROP TABLE IF EXISTS config_rows; DROP TABLE IF EXISTS config_fields;
DROP TABLE IF EXISTS config_references; DROP TABLE IF EXISTS config_consumers;
CREATE TABLE config_tables(name TEXT PRIMARY KEY,category TEXT,row_count INTEGER,key_field TEXT,sha256 TEXT,source TEXT);
CREATE TABLE config_rows(table_name TEXT,row_key TEXT,label TEXT,payload TEXT,PRIMARY KEY(table_name,row_key));
CREATE TABLE config_fields(table_name TEXT,name TEXT,type TEXT,is_reference INTEGER);
CREATE TABLE config_references(source TEXT,source_key TEXT,field TEXT,target TEXT,target_key TEXT,resolved INTEGER,evidence TEXT);
CREATE TABLE config_consumers(table_name TEXT,source TEXT,line INTEGER,code TEXT);''')
lines=['# 原作配置全表目录','','由 tools/config-study/index_config.py 生成。分类是研究导航，非原作系统边界。完整字段包含在字段字典及 schema.json；代码命中不等于运行覆盖。','','|表|分类|行数|主键|直接代码引用位置数|','|---|---|---:|---|---:|']
dictionary=['# 原作配置字段字典','','自动反射原作生成读取器；字段名保留原名。业务释义与案例见 README.md，不能以字段英文猜测单位。','']
for t in inventory['tables']:
    n=t['name']
    db.execute('INSERT INTO config_tables VALUES(?,?,?,?,?,?)',(n,t['category'],t['rows'],keys[n],t['sha256'],t['readerSource']))
    for index,row in enumerate(data[n]):
        db.execute('INSERT INTO config_rows VALUES(?,?,?,?)',(n,str(numeric(row[keys[n]])) if keys[n] else str(index),str(row.get('Name',row.get('EquipmentName',''))),json.dumps(row,ensure_ascii=False)))
    for field in t['fields']:
        db.execute('INSERT INTO config_fields VALUES(?,?,?,?)',(n,field['name'],field['type'],field['reference']))
    for c in consumers[n]:db.execute('INSERT INTO config_consumers VALUES(?,?,?,?)',(n,c['path'],c['line'],c['text']))
    lines.append(f"|[{n}](tables/{n}.json)|{t['category']}|{t['rows']}|{keys[n]}|{len(consumers[n])}|")
    dictionary += [f'## {n}', '', f"读取器：`{t['readerSource']}`；行类型：`{t['rowType']}`；条目：{t['rows']}。", '', '|字段|类型|生成引用字段|','|---|---|---|']
    dictionary += [f"|{f['name']}|`{f['type']}`|{f['reference']}|" for f in t['fields']]
    dictionary += ['','代码消费样本：']+[f"- `{c['path']}:{c['line']}`" for c in consumers[n][:4]]+['']
for r in references:db.execute('INSERT INTO config_references VALUES(?,?,?,?,?,?,?)',(r['source'],str(r['sourceKey']),r['field'],r['target'],str(r['targetKey']),r['exists'],r['evidence']))
db.commit();db.close()
summary=dict(tableCount=len(data),rowCount=sum(len(r) for r in data.values()),packedBytes=len(packed),
             categoryCounts=dict(Counter(t['category'] for t in inventory['tables'])),explicitReferenceChecks=len(references),
             unresolvedReferences=len(missing),unresolvedByField=dict(Counter(r['source']+'.'+r['field'] for r in missing)),
             sourceFileCount=len(provenance),consumerLocations=sum(map(len,consumers.values())))
for name,value in [('inventory.json',inventory),('provenance.json',provenance),('references.json',references),('unresolved-references.json',missing),('consumers.json',consumers),('summary.json',summary)]:
    (OUT/name).write_text(json.dumps(value,ensure_ascii=False,indent=2),encoding='utf-8')
(OUT/'CATALOG.md').write_text('\n'.join(lines)+'\n',encoding='utf-8')
(OUT/'FIELDS.md').write_text('\n'.join(dictionary)+'\n',encoding='utf-8')
print(json.dumps(summary,ensure_ascii=False))
