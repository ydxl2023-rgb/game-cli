"""Recover introductory recruitment and first-battle access configuration."""
from pathlib import Path
import json
import hashlib
import sys

ROOT = Path(__file__).resolve().parents[2]
OUT = Path(__file__).parent
TABLES = ROOT / 'research/config-study/tables'

def read(name):
    return json.loads((TABLES / (name + '.json')).read_text(encoding='utf8'))

tasks = {row['TaskID']: row for row in read('tbtask')}
heroes = {row['ID']: row for row in read('tbhero')}
languages = []
sheets = []

def sheet(name, columns, rows):
    sheets.append(dict(name=name, columns=columns, rows=rows))

sheet('RecruitStep', [['ID','int'],['Kind','int'],['BuildingID','int'],['RequiredTaskID','int'],
    ['DialogueGroupID','int'],['HeroID','int'],['CostItemID','int'],['CostCount','int'],['AutoClaim','int']], [
    [60106,1,60021,60105,3028,0,0,0,0],
    [60112,2,60004,60106,3029,0,83010,1,1],
    [60113,3,60004,60112,3031,129,0,0,1],
    [60114,3,60004,60113,3030,133,0,0,1]])
dialogues = []
for row in read('tbconversation'):
    if row['GroupID'] not in [3028,3029,3030,3031]:
        continue
    speaker = 10000000 + row['ID'] * 2
    content = speaker + 1
    portrait = row['CharacterImg'].split('/')[-1]
    dialogues.append([row['ID'],row['GroupID'],speaker,content,
        'Assets/Data/Widgets/Recruit/Portraits/' + portrait + '.png',row['Direction']])
    languages.extend([[speaker,'TEXT_RECRUIT_SPEAKER_' + str(row['ID']),row['CharacterName']],
                      [content,'TEXT_RECRUIT_DIALOGUE_' + str(row['ID']),row['Content']]])
sheet('RecruitDialogue', [['ID','int'],['GroupID','int'],['SpeakerTextID','int'],['ContentTextID','int'],
    ['PortraitPath','string'],['Direction','int']], dialogues)
hero_rows = []
for hero_id, actor, task, end in [(129,6,60113,[-24.15,35.3,38.49]),(133,7,60114,[-25.37,35.3,40.38])]:
    hero = heroes[hero_id]
    model = hero['Model'][0].split('/')[0]
    text_id = 307000 + hero_id
    languages.append([text_id,'TEXT_RECRUIT_HERO_' + str(hero_id),hero['Name']])
    hero_rows.append([hero_id,actor,task,text_id,model,
        'Assets/Data/Scenes/Town/Spine/' + model + '/Skeleton.asset',
        -29.77,35.3,44.71,*end,100,500000])
sheet('RecruitHero', [['ID','int'],['ActorID','int'],['TaskID','int'],['NameTextID','int'],['Model','string'],
    ['SkeletonPath','string'],['StartX','float'],['StartY','float'],['StartZ','float'],
    ['EndX','float'],['EndY','float'],['EndZ','float'],['InitialHealthPercent','int'],['InitialGold','int']], hero_rows)
battle = next(row for row in read('tbbattle') if row['ID'] == 101001)
area = next(row for row in read('tbbattlearea') if row['ID'] == battle['AreaId'])
feature = next(row for row in read('tbfeatureunlock') if row['ID']['value'] == 1011)
sheet('BattleEntry', [['ID','int'],['AreaID','int'],['RequiredTaskID','int'],['TutorialTaskID','int'],
    ['RecommendPower','int'],['TitleTextID','int'],['NameTextID','int'],['DescriptionTextID','int']],
    [[battle['ID'],battle['AreaId'],feature['UnlockNeedCondition'][0]['Value'],60111,battle['RecommendPower'],
      308001,308002,308003]])
languages.extend([[308001,'TEXT_BATTLE_ENTRY_TITLE',battle['SceneName']],
    [308002,'TEXT_BATTLE_ENTRY_NAME',battle['BattleName']],
    [308003,'TEXT_BATTLE_ENTRY_DESCRIPTION',area['AreaDesc']],
    [308004,'TEXT_BATTLE_ENTRY_GATHER','集结'],[308005,'TEXT_BATTLE_ENTRY_READY','首战入口已解锁'],
    [308006,'TEXT_BATTLE_ENTRY_REQUIRES_HERO','请先招募英雄'],
    [308007,'TEXT_BATTLE_ENTRY_LOCKED','请先完成医院修复任务'],
    [308008,'TEXT_BATTLE_ENTRY_POWER','推荐战力 {0}'],
    [308009,'TEXT_BATTLE_ENTRY_PREPARED','已选择霍尔山首关，战斗执行将在后续接入'],
    [308010,'TEXT_RECRUIT_TALK','对话'],[308011,'TEXT_RECRUIT_RECEIVE','招募'],
    [308012,'TEXT_RECRUIT_HORN','吹响哨子'],[308013,'TEXT_RECRUIT_MISSING','需要残破的狮鹫哨'],
    [308014,'TEXT_RECRUIT_NEXT','点击继续'],[308015,'TEXT_RECRUIT_RETRY','重试'],
    [308016,'TEXT_RECRUIT_BROTHER','兄弟雷恩'],[308017,'TEXT_RECRUIT_HORN_NPC','狮鹫管理员']])
sources = [TABLES / (name + '.json') for name in ['tbtask','tbconversation','tbhero','tbbattle','tbbattlearea','tbfeatureunlock']]
source = dict(sheets=sheets,languages=languages,
    sources=[dict(path=p.relative_to(ROOT).as_posix(),sha256=hashlib.sha256(p.read_bytes()).hexdigest()) for p in sources],
    adaptations=['New recruits use the existing local normalized health and starting hero currency model.',
        'Battle entry exposes original first-stage availability; no combat outcome or victory reward is fabricated.'])
(OUT / 'configuration-source.json').write_text(json.dumps(source,ensure_ascii=False,indent=2),encoding='utf8')
sys.path.insert(0,str(ROOT / 'tools/opening'))
from audit_original_ui import package
layouts = [package(ROOT / f'extracted-resources/assets/data/ui/atlas/{name}/{name}_fui.bytes', names)
    for name,names in [('guide',['ConversationPage']),('building',['BuildingEntranceBubble','TalkButton','HornAnimationPage']),
                      ('level',['BossListPage','BossListGatherButton','BossListItem'])]]
(OUT / 'original-layouts.json').write_text(json.dumps(layouts,ensure_ascii=False,indent=2),encoding='utf8')
print(json.dumps({'sheets': {s['name']:len(s['rows']) for s in sheets},'texts':len(languages)}))
