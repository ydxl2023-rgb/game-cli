"""Prepare reviewed Settings bindings and initial source files, never native assets."""
import json
import re
from pathlib import Path

base = Path(__file__).resolve().parents[1]
folder = base / '.data/imports/sv-settings'
source = json.loads((folder / 'source/main/unity-prefab.json').read_text(encoding='utf8'))
design = json.loads((folder / 'main/template.json').read_text(encoding='utf8'))['design']
nodes = {n['id']: n for n in design['nodes']}
output = base.parents[1] / 'examples/unitys/development/client/Assets/Scripts/Systems/Settings'
# Maintenance must preserve explicit persisted enum values, including gaps after removals.
existingTypes=(output/'SettingsTypes.cs').read_text(encoding='utf8') if (output/'SettingsTypes.cs').exists() else ''
stableValues={}
for enumName in ['SettingsOption','SettingsAction']:
    match=re.search(r'public enum '+enumName+r'\s*\{(.*?)\n\}',existingTypes,re.S)
    stableValues[enumName]=dict((name,int(value)) for name,value in re.findall(r'\b(\w+)\s*=\s*(\d+)\s*,',match[1])) if match else {}

def enumEntry(enumName,name,index):
    values=stableValues[enumName]
    if values and name not in values:raise ValueError('Assign a stable numeric enum value before regeneration: '+name)
    return name+' = '+str(values.get(name,index))
for sub in ['UI', 'VM']:
    (output / sub).mkdir(parents=True, exist_ok=True)

def find(name):
    matches = [n for n in source['nodes'] if n['name'] == name and 'Settings'+n['path'].replace('/', '_') in nodes]
    assert len(matches) == 1, (name, len(matches))
    return matches[0]['path']

def native(path):
    n = nodes['Settings' + path.replace('/', '_')]
    return native(n['parent'][8:].replace('_', '/')) + '/' + n['name'] if n['parent'] else n['name']

fields, bind, refresh, handled, preferences = [], [], [], set(), []
def field(name, kind, sync, paths, component):
    paths = paths if isinstance(paths, list) else [paths]
    fields.append(dict(name=name, type=kind, sync=sync, paths=paths, targets=[native(p) for p in paths], component=component))
    return name[0].upper() + name[1:]

def visible(name, paths, expression):
    prop = field(name, 'bool', 'SyncVisible', paths, 'RectTransform')
    refresh.append(f'        vm.{prop} = {expression};')

def click(name, path, action):
    prop = field(name, 'ButtonClick', 'SyncClick', path, 'Button')
    handled.add(path)
    bind.append(f'                vm.{prop} = (sender) => {action};')

tabs = [('Screen', 'ScreenTog', 'ScreenSet'), ('Sound', 'VoiceTog', 'SouneSet'), ('Function', 'FunctionTog', 'FunctionSet'), ('Privacy', 'PrivateTog', 'PrivateSet'), ('Download', 'DownloadTog', 'DownloadSet')]
for tab, button, panel in tabs:
    path = find(button)
    click(tab.lower()+'Click', path, f'SelectTab(SettingsTab.{tab})')
    visible(tab.lower()+'Visible', [find(panel), path+'/1'], f'widget.selectedTab == SettingsTab.{tab}')
    visible(tab.lower()+'Unselected', path+'/0', f'widget.selectedTab != SettingsTab.{tab}')

switches = [
    ('Audience','ViewerBtn','Viewer'), ('PlayerOutline','PlayerOutlineBtn','PlayerOutline'), ('TapEffect','ClickEffBtn','ClickEff'),
    ('SoundEnabled','SoundBtn','Sound'), ('MusicEnabled','MusicBtn','Music'), ('VoiceEnabled','VoiceBtn','Voice'),
    ('ShowFps','FpsShowrBtn','FpsShow'), ('Vibration','ShakeBtn','Shake'), ('MatchEffect','MatchEffectBtn','MatchEffect'),
    ('MatchAnimation','MatchAnimationBtn','MatchAnimation'),
   
    ('FriendInvite','FriendInviteBtn','FriendInvite'), ('AssociationInvite','AssociationInviteBtn','AssociationInvite'),
    ('PersonalInfo','PersonalInfoBtn','PersonalInfo'), ('HorseRaceLamp','HorseRaceLampBtn','HorseRaceLamp'),
    ('InGamePush','InGamePushBtn','InGamePush'), ('NightPush','NightPushBtn','NightPush')]
for key, button, prefix in switches:
    preferences.append(dict(name=key, kind='Switch', default=0 if key in ['ShowFps','NightPush'] else 1))
    click(key[0].lower()+key[1:]+'Click',find(button),f'TogglePreference(SettingsOption.{key})')
    visible(key[0].lower()+key[1:]+'On',find(prefix+'Open'),f'entity.IsEnabled(SettingsOption.{key})')
    visible(key[0].lower()+key[1:]+'Off',find(prefix+'Close'),f'!entity.IsEnabled(SettingsOption.{key})')
for key in ['ChatWorld', 'ChatAssociation']:
    preferences.append(dict(name=key, kind='Switch', default=0))
    click(key[0].lower()+key[1:]+'Click',find(key),f'TogglePreference(SettingsOption.{key})')
    visible(key[0].lower()+key[1:]+'Selected',find(key+'Choose'),f'entity.IsEnabled(SettingsOption.{key})')
preferences.append(dict(name='WifiOnly', kind='Switch', default=1))
for name, value in [('OnlyWifi', True), ('WifiAndNet', False)]:
    click(name[0].lower()+name[1:]+'Click',find(name),f'ChangeNetwork({str(value).lower()})')
    visible(name[0].lower()+name[1:]+'Selected',find(name+'Choose'),('' if value else '!')+'entity.IsEnabled(SettingsOption.WifiOnly)')

qualities = [('Resolution','Resolution','Res'),('FrameRate','Fps','Fps'),('Reflection','Ref','Ref'),('Scene','Scene','Scene'),('Texture','Tex2d','Tex2d'),('Volume','Vol','Vol'),('AntiAliasing','Msaa','Msaa'),('Model','Model','Model')]
for key, prefix, marker in qualities:
    preferences.append(dict(name=key, kind='Quality', default=2))
    for level in ['Low', 'Mid', 'High']:
        if key == 'Model' and level == 'Mid': continue
        quality = 'Medium' if level == 'Mid' else level
        click(prefix[0].lower()+prefix[1:]+level+'Click',find(prefix+level),f'ChooseQuality(SettingsOption.{key}, SettingsQuality.{quality})')
        visible(prefix[0].lower()+prefix[1:]+level+'Selected',find(marker+level+'Choose'),f'entity.ReadQuality(SettingsOption.{key}) == SettingsQuality.{quality}')
for level in ['Low','Mid','High']:
    quality = 'Medium' if level == 'Mid' else level
    click('preset'+level+'Click',find(level),f'SelectPreset(SettingsQuality.{quality})')
    visible('preset'+level+'Selected',find(level+'Choose'),f'entity.qualityPreset == SettingsQuality.{quality}')
click('customClick',find('Custom'),'ShowCustom()')
visible('customSelected',find('CustomChoose'),'entity.qualityPreset == SettingsQuality.Custom')
for channel in ['Music','Sound','Voice']:
    key = channel+'Volume'
    preferences.append(dict(name=key,kind='Volume',default=1))
    prop=field(channel.lower()+'Volume','float','SyncSlider',find(channel+'Slider'),'Slider')
    refresh.append(f'        vm.{prop} = entity.ReadVolume(SettingsOption.{key});')
    prop=field(channel.lower()+'Changed','SliderChange','SyncSliderChange',find(channel+'Slider'),'Slider')
    bind.append(f'                vm.{prop} = (sender, value) => ChangeVolume(SettingsOption.{key}, value);')
    prop=field(channel.lower()+'Percent','string','SyncText',find(channel+'VolumeText'),'Text')
    refresh.append(f'        vm.{prop} = UnityEngine.Mathf.RoundToInt(entity.ReadVolume(SettingsOption.{key}) * 100f).ToString() + "%";')

click('closeClick',find('CloseBtn'),'Close()')
services=[]
for n in source['nodes']:
    if 'Settings'+n['path'].replace('/','_') not in nodes:continue
    if any(c['type'].startswith('UnityEngine.UI.Button,') for c in n['components']) and n['path'] not in handled:
        key=n['name'].strip('_')
        services.append(key)
        click(key[0].lower()+key[1:]+'Click',n['path'],f'RequestService(SettingsAction.{key})')
field('serverName','string','SyncText',find('ServerName'),'Text')

contract=dict(specs=[dict(name='Settings',page='main',root='0',nativeRoot='Settings',fields=fields)],grids=[],preferences=preferences,tabs=tabs,services=services)
(folder/'bindings.json').write_text(json.dumps(contract,ensure_ascii=False,indent=2),encoding='utf8')
(folder/'bind-body.txt').write_text('\n'.join(bind),encoding='utf8')
(folder/'refresh-body.txt').write_text('\n'.join(refresh),encoding='utf8')
vm='using Oathx.Widget;\n\n/// <summary>Dynamic values and interactions for the original five settings tabs.</summary>\npublic partial class VMSettings : VM\n{\n'
vm+='\n\n'.join(f'    /// <summary>Settings {x["name"]} binding.</summary>\n    [Binding'+('' if x['type']=='ButtonClick' else f'(typeof({x["sync"]}))')+f']\n    {x["type"]} {x["name"]};' for x in fields)+'\n}\n'
(output/'VM/VMSettings.cs').write_text(vm,encoding='utf8')
enum='/// <summary>Supported local preference identifiers.</summary>\npublic enum SettingsOption\n{\n'+'\n\n'.join(f'    /// <summary>{x["name"]} preference.</summary>\n    '+enumEntry('SettingsOption',x['name'],i)+',' for i,x in enumerate(preferences))+'\n}\n'
enum+='\n/// <summary>Original page categories.</summary>\npublic enum SettingsTab\n{\n'+ '\n\n'.join(f'    /// <summary>{t[0]} page.</summary>\n    {t[0]},' for t in tabs)+'\n}\n'
enum+='\n/// <summary>Quality tiers; Custom describes mixed values.</summary>\npublic enum SettingsQuality\n{\n    /// <summary>Low quality.</summary>\n    Low,\n\n    /// <summary>Medium quality.</summary>\n    Medium,\n\n    /// <summary>High quality.</summary>\n    High,\n\n    /// <summary>Mixed quality options.</summary>\n    Custom\n}\n'
enum+='\n/// <summary>Product service requests; the template never executes these services.</summary>\npublic enum SettingsAction\n{\n'+'\n\n'.join(f'    /// <summary>Request the original {s} entry.</summary>\n    '+enumEntry('SettingsAction',s,i)+',' for i,s in enumerate(services))+'\n}\n'
(output/'SettingsTypes.cs').write_text(enum,encoding='utf8')
print(dict(preferences=len(preferences),bindings=len(fields),services=len(services)))
