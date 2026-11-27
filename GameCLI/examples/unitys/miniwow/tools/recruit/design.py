from pathlib import Path
import json,shutil,hashlib
R=Path(__file__).resolve().parents[2];O=R/'tools/recruit'
def node(n,k,x,y,w,h,c=None):return dict(id=n,name=n,element=k,rect=dict(x=x,y=y,width=w,height=h),children=c or [])
def label(n,t,x,y,w,h,size=32):
 a=node(n,'Text',x,y,w,h);a['text']=dict(content=t,fontSize=size,color='#F6E6C5',alignment='center');return a
imports=[]
def emit(name,folder,children,assets):
 layout=dict(schemaVersion='1',uiName=name,referenceResolution=dict(width=1080,height=1920),nodes=[node(name,'Container',0,0,1080,1920,children)])
 manifest=dict(schemaVersion='1',uiName=name,assets=[])
 for src,used in assets:
  src=R/src; dst=f'Assets/Data/Widgets/{folder}/Images/{src.name}'
  manifest['assets'].append(dict(id=src.stem,file='../../'+src.relative_to(R).as_posix(),path=dst,usedBy=used))
  imports.append(dict(source=src.relative_to(R).as_posix(),destination=dst,sha256=hashlib.sha256(src.read_bytes()).hexdigest()))
 (O/(name+'.json')).write_text(json.dumps(layout,ensure_ascii=False,indent=2),encoding='utf8');(O/(name+'.materials.json')).write_text(json.dumps(manifest,ensure_ascii=False,indent=2),encoding='utf8')
def fg(p,n):return f'extracted-resources/fairygui/{p}/{n}.png'
dialog=[node('ContinueClick','Button',0,0,1080,1920),node('Portrait','Image',-15,943.5,525,450),node('DialoguePanel','Image',9,1360.5,1062,505.5),node('NamePanel','Image',0,1341,475.5,99),label('Speaker','',60,1353,294,66),node('Line','Image',34.5,1453.5,988.5,33),label('Dialogue','',90,1503,912,283.5,36),node('Arrow','Image',900,1699.5,37.5,34.5)]
root=[node('DialogueVisible','Container',0,0,1080,1920,dialog),node('MovieVisible','Container',0,0,1080,1920,[node('MovieScreen','Image',0,0,1080,1920),node('RetryVisible','Container',0,0,1080,1920,[node('RetryClick','Button',400,1500,280,80,[label('RetryLabel','重试',400,1500,280,80)])])]),node('CloseClick','Button',822,100,249,79.5,[label('CloseLabel','返回',822,100,249,79.5)])]
emit('UIRecruit','Recruit',root,[(fg('Guide/Images','hub_bg_19'),['DialoguePanel']),(fg('Guide/Images','hub_title_1'),['NamePanel']),(fg('Guide/Images','hub_line_2'),['Line']),(fg('Common/images','common_icon_5_1'),['Arrow'])])
buttons=[]
for n,t in [('BrotherClick','对话'),('HornClick','吹哨'),('FirstClick','招募'),('SecondClick','招募')]:
 buttons.append(node(n,'Button',0,0,117,115.5,[label(n+'Label',t,0,31.5,117,48,29)]))
emit('UIHomeRecruitEntry','Home',buttons,[(fg('Building/images','hub_button_15_1'),[n['name'] for n in buttons])])
# Map marker is positioned from area 1 FixedBossLocation, within the original 720-wide canvas.
mapnodes=[node('MapBackground','Image',0,0,1080,1920),label('Title','',402,156,276,73.5,42),node('StageClick','Button',730.5,814.5,210,183,[node('BossIcon','Image',763.5,840,139.5,141)]),label('MapName','霍尔山',756,985,156,44)]
stage=[node('Panel','Image',31.5,79.5,1014,1750),label('DetailTitle','',402,106.5,276,73.5,42),node('DescriptionPanel','Image',97.5,228,887,187),label('Description','',118.5,243,841.5,123,25),node('ListPanel','Image',94.5,463.5,888,1323),node('SelectClick','Button',109.5,486,853.5,180,[label('StageName','',280,520,550,73.5,42)]),label('Power','',394.5,1525.5,600,44,32),node('GatherClick','Button',282,1570.5,514.5,186,[label('GatherLabel','集结',424.5,1615.5,222,73.5,48)])]
emit('UIBattleEntry','BattleEntry',[node('MapVisible','Container',0,0,1080,1920,mapnodes),node('StageVisible','Container',0,0,1080,1920,stage),label('Feedback','',100,1780,880,75,28),node('CloseClick','Button',3,1780,174,144)],[(str(Path('extracted-resources/assets/data/ui/background/map_1_0.png')),['MapBackground']),(fg('Level/icons','expl_icon_41_1'),['BossIcon']),(fg('Common/images','common_bg_9'),['Panel']),(fg('Common/images','common_bg_11_0'),['DescriptionPanel']),(fg('Level/icons','expl_bg_5'),['ListPanel']),(fg('Level/images','expl_bg_2'),['SelectClick']),(fg('Common/images','common_button_14'),['GatherClick']),(fg('Common/images','common_button_7'),['CloseClick'])])
# Preserve recovered texture/skeleton/video input and record every imported source.
for model in ['hero_20029','hero_20033','npc_brother_2']:
 for ext in ['.png','.atlas.txt','.skel.bytes']:
  src=R/'extracted-resources/assets/data/spines'/model/(model+ext);dst=R/'client/Assets/Data/Scenes/Town/Spine'/model/src.name;dst.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(src,dst);imports.append(dict(source=src.relative_to(R).as_posix(),destination=dst.relative_to(R/'client').as_posix(),sha256=hashlib.sha256(src.read_bytes()).hexdigest()))
for src in (R/'extracted-resources/fairygui/CharacterIcon/NPC').glob('*.png'):
 if src.stem in ['Conversation_brother_1','Conversation_doghouse_1','Conversation_hero_20029','Conversation_hero_20033']:
  dst=R/'client/Assets/Data/Widgets/Recruit/Portraits'/src.name;dst.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(src,dst);imports.append(dict(source=src.relative_to(R).as_posix(),destination=dst.relative_to(R/'client').as_posix(),sha256=hashlib.sha256(src.read_bytes()).hexdigest()))
src=next((R/'extracted-resources/videos').glob('heroDrawVideo_*.mp4'));dst=R/'client/Assets/Data/Widgets/Recruit/heroDrawVideo.mp4';shutil.copyfile(src,dst);imports.append(dict(source=src.relative_to(R).as_posix(),destination=dst.relative_to(R/'client').as_posix(),sha256=hashlib.sha256(src.read_bytes()).hexdigest()))
(O/'imports.json').write_text(json.dumps(imports,ensure_ascii=False,indent=2),encoding='utf8')
print('Generated three original-asset UI layouts and imported',len(imports),'assets')


