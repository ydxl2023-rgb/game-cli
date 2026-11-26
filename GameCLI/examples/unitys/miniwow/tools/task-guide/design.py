import json
from pathlib import Path
p=Path(__file__).resolve().parent
payload={'schemaVersion':'1','uiName':'UITaskGuideFinger','referenceResolution':{'width':1080,'height':1920},'nodes':[{'id':'UITaskGuideFinger','name':'UITaskGuideFinger','element':'Container','rect':{'x':0,'y':0,'width':1080,'height':1920},'children':[{'id':'Finger','name':'Finger','element':'Image','rect':{'x':480,'y':900,'width':120,'height':120},'children':[]}]}]}
manifest={'schemaVersion':'1','uiName':'UITaskGuideFinger','assets':[{'id':'finger','file':'../../extracted-resources/fairygui/Guide/Images/e_ui_shouzhi_1.png','path':'Assets/Data/Widgets/Task/Images/e_ui_shouzhi_1.png','usedBy':['Finger']}]}
(p/'finger.json').write_text(json.dumps(payload),encoding='utf8')
(p/'finger.materials.json').write_text(json.dumps(manifest),encoding='utf8')
