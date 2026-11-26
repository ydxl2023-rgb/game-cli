"""Produce formal UGUI inputs; native bindings are installed by MiniwowOpeningSetup."""
from pathlib import Path
import json, shutil, hashlib
root=Path(__file__).resolve().parents[2]
def node(name, kind, x,y,w,h, children=None, text=None):
    n=dict(id=name,name=name,element=kind,rect=dict(x=x,y=y,width=w,height=h),children=children or [])
    if text is not None:n['text']=dict(content=text,fontSize=34,alignment='center',color='#F4E6C7')
    return n
def label(name,x,y,w,h):return node(name,'Text',x,y,w,h,text='')
def button(name,labelname,x,y,w=320,h=90):return node(name,'Button',x,y,w,h,[label(labelname,x,y,w,h)])
movie=node('MovieVisible','Image',0,0,1080,1920,[node('VideoScreen','Image',0,0,1080,1920),button('SkipClick','SkipLabel',730,75,290,80),node('ErrorVisible','Image',140,750,800,350,[label('ErrorText',170,780,740,140),button('RetryClick','RetryLabel',380,970)])])
objective=node('ObjectiveVisible','Image',45,350,990,160,[label('Objective',75,365,930,90),label('WaterCount',160,455,760,40),node('WaterIcon','Image',90,445,60,60)])
rescue=node('RescueVisible','Container',380,1280,320,100,[button('RescueClick','RescueLabel',380,1280)])
claim=node('ClaimVisible','Container',260,1280,560,100,[button('ClaimClick','ClaimLabel',260,1280,560)])
dialogue=node('DialogueVisible','Image',35,1190,1010,480,[node('Portrait','Image',65,1160,210,230),label('Speaker',290,1220,600,70),label('Dialogue',95,1320,890,210),button('ContinueClick','ContinueLabel',680,1550)])
confirm=node('ConfirmVisible','Image',0,0,1080,1920,[node('ConfirmPanel','Image',100,680,880,500,[label('ConfirmText',150,750,780,150),button('ConfirmClick','ConfirmLabel',150,1000),button('CancelClick','CancelLabel',600,1000)])])
design=dict(schemaVersion='1',uiName='UIHomeOpening',referenceResolution=dict(width=1080,height=1920),nodes=[node('UIHomeOpening','Container',0,0,1080,1920,[objective,rescue,claim,dialogue,movie,confirm])])
(root/'tools/opening/ui.json').write_text(json.dumps(design,ensure_ascii=False,indent=2),encoding='utf-8')
assets=[]
for name,source,destination in [('WaterIcon','fairygui/CommonIcon/big_water.png','Assets/Data/Widgets/Common/Icons/big_water.png'),('Portrait','fairygui/CharacterIcon/NPC/Conversation_priest.png','Assets/Data/Widgets/Common/Icons/Conversation_priest.png')]:
    assets.append(dict(id=name,file='../../extracted-resources/'+source,path=destination,usedBy=[name]))
assets.extend([
    dict(id='DialogueVisible',file='../../extracted-resources/fairygui/Guide/Images/hub_bg_19.png',path='Assets/Data/Widgets/Home/Opening/dialogue.png',usedBy=['DialogueVisible','ConfirmPanel']),
    dict(id='ContinueClick',file='../../extracted-resources/fairygui/Guide/Images/hub_bg_23.png',path='Assets/Data/Widgets/Home/Opening/button.png',usedBy=['ContinueClick','RescueClick','ClaimClick','ConfirmClick','CancelClick','RetryClick','SkipClick'])
])
(root/'tools/opening/materials.json').write_text(json.dumps(dict(schemaVersion='1',uiName='UIHomeOpening',assets=assets),indent=2),encoding='utf-8')
resources=[('videos/startVideo_a4476169985aaf668ab78b7e6f711507.mp4','Assets/Data/Widgets/Home/Opening/startVideo.mp4','Original opening movie'),('assets/data/effects/textures/ef_tex_fire_sequence_4x4_05.png','Assets/Data/Scenes/Home/Opening/fire.png','Recovered fire texture for native particle adaptation'),('fairygui/CharacterIcon/NPC/Conversation_auction.png','Assets/Data/Widgets/Common/Icons/Conversation_auction.png','Merchant portrait')]
records=[]
for source,destination,use in resources:
    src=root/'extracted-resources'/source;dst=root/'client'/destination;dst.parent.mkdir(parents=True,exist_ok=True);shutil.copyfile(src,dst)
    records.append(dict(source=source,destination=destination,use=use,sha256=hashlib.sha256(src.read_bytes()).hexdigest()))
(root/'tools/opening/resources.json').write_text(json.dumps(records,indent=2),encoding='utf-8')
