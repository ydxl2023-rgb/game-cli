"""Finalize a CLI-restored Settings prefab without direct asset serialization."""
import json
import subprocess
from pathlib import Path

base=Path(__file__).resolve().parents[1]
folder=base/'.data/imports/sv-settings'
project=base.parents[1]/'examples/unitys/development/client'
cli='G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
asset='Assets/Data/Widgets/Settings/Settings.prefab'
temp='Assets/Editor/GameCLI/SettingsBuild'
log=[]
def invoke(*args):
    r=subprocess.run([cli,'--project',str(project),'--format','json',*map(str,args)],capture_output=True,encoding='utf-8-sig')
    data=json.loads(r.stdout)
    assert data['ok'],(args,data)
    log.append({'args':list(map(str,args)),'result':data})
    (base/'reports/settings-template/native-finalization.json').write_text(json.dumps(log,ensure_ascii=False,indent=2),encoding='utf8')
    return data['data']

invoke('components','add','--asset',asset,'--path','Settings','--component','UISettings')
args=['widget-metadata','UISettings','--vm','VMSettingsBindingMap','--prefab',asset]
invoke(*args,'--dry-run')
invoke(*args)
subprocess.run(['python',str(base/'scripts/bind-settings-native.py')],check=True)
for a in json.loads((folder/'animation-attachments.json').read_text(encoding='utf8')):
    invoke('components','add','--asset',asset,'--path',a['path'],'--component','Animator')
    for prop,value in [('m_Controller',a['controller']),('m_UpdateMode','2')]:
        invoke('modify','--asset',asset,'--path',a['path'],'--component','Animator','--property',prop,'--value',value)
invoke('modify','--asset',asset,'--path','Settings','--component','UISettings','--property','soundAnimation','--value','@Settings/Animator#Animator')

guid=(project/(asset+'.meta')).read_bytes()
invoke('prefab','create','--json',folder/'layer-wrapper.json','--prefab',temp+'/LayerWrapper.prefab','--dry-run')
invoke('prefab','create','--json',folder/'layer-wrapper.json','--prefab',temp+'/LayerWrapper.prefab')
args=['prefab','attach','--parent-prefab',temp+'/LayerWrapper.prefab','--parent-path','LayerWrapper','--child-prefab',asset,'--child-name','Settings','--layer','UI','--no-scene-replace']
invoke(*args,'--dry-run');invoke(*args)
args=['prefab','extract','--prefab',temp+'/LayerWrapper.prefab','--path','LayerWrapper/Settings','--dst',temp+'/Settings.prefab']
invoke(*args,'--dry-run');invoke(*args)
assert '--- !u!1001' not in (project/temp/'Settings.prefab').read_text(encoding='utf8'),'Unexpected nested variant'
invoke('asset','import','--src',project/temp/'Settings.prefab','--dst',asset)
assert (project/(asset+'.meta')).read_bytes()==guid,'Main GUID changed'
inspection=invoke('prefab','inspect','--prefab',asset,'--recursive')
assert all(n['layer']==5 for n in inspection['nodes'])
design=json.loads((folder/'main/template.json').read_text(encoding='utf8'))['design']
assert sum(not n['activeSelf'] for n in inspection['nodes'])==sum(not n.get('active',True) for n in design['nodes'])
assert (project/temp).resolve().is_relative_to((project/'Assets/Editor/GameCLI').resolve())
invoke('asset','delete','--src',temp)
invoke('asset','delete','--src','Assets/Scripts/VMSettingsBindingMap.cs')
(base/'reports/settings-template/native-finalization.json').write_text(json.dumps(log,ensure_ascii=False,indent=2),encoding='utf8')
print('Final native prefab: source activation and UI layers verified',flush=True)
