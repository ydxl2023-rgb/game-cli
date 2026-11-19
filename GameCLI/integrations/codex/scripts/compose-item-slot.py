"""Compose Reward from an ItemSlot using UnityCLI; never edit Unity YAML."""
import json
import pathlib
import re
import subprocess

project = pathlib.Path('G:/Works/GameCLI/GameCLI/examples/unitys/development/client')
cli = 'G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
report = pathlib.Path('reports/item-slot')

def run(args):
    result = subprocess.run([cli, '--project', str(project), '--format', 'json', *args], capture_output=True, encoding='utf-8-sig')
    value = json.loads(result.stdout)
    assert value['ok'], value
    return value

def field(asset, path, component, name, value):
    return run(['modify', '--asset', asset, '--path', path, '--component', component, '--property', name, '--value', str(value)])

def attach(parent, path, child, name):
    args = ['prefab', 'attach', '--parent-prefab', parent, '--parent-path', path, '--child-prefab', child, '--child-name', name, '--replace', '--no-scene-replace']
    run(args + ['--dry-run'])
    return run(args)

def bind(asset, root, vm, fields):
    guid = re.search(r'^guid: (\w+)', (project / ('Assets/Scripts/Systems/Common/UI/VM/'+vm+'.cs.meta')).read_text(), re.M)[1]
    field(asset, root, 'Metadata', 'className', vm)
    field(asset, root, 'Metadata', 'classGuid', guid)
    field(asset, root, 'Metadata', 'metadatas.Array.size', len(fields))
    for index, (name, sync, path, component) in enumerate(fields):
        prefix = 'metadatas.Array.data['+str(index)+']'
        for key, value in [('name', name), ('type', sync), ('targets.Array.size', 1), ('components.Array.size', 1), ('targets.Array.data[0]', '@'+path), ('components.Array.data[0]', '@'+path+'#'+component)]:
            field(asset, root, 'Metadata', prefix+'.'+key, value)

slot = 'Assets/Data/Widgets/Common/Micros/ItemSlot/ItemSlot.prefab'
legacy = 'Assets/Data/Widgets/Common/Micros/ItemSlot.prefab'
for name in ['Lock', 'Mask', 'Arrow']:
    part = 'Assets/Data/Widgets/Common/Micros/ItemSlot/Parts/'+name+'.prefab'
    args = ['prefab', 'extract', '--prefab', legacy, '--path', 'ItemSlot/'+name, '--dst', part]
    run(args+['--dry-run'])
    run(args)
    if name == 'Arrow':
        # The legacy prefab references a controller absent from the current project.
        run(['components','remove','--asset',part,'--path',name,'--component','Animator'])
    attach(slot, 'ItemSlot', part, name)
field(slot, 'ItemSlot/Mask', 'Image', 'm_FillAmount', 0)
prefix = 'ItemSlot/BagIcon1/IconVisible'
fields = [
    ('back', 'SyncSprite', prefix+'/ItemInfo/ItemHui', 'Image'),
    ('icon', 'SyncSprite', prefix+'/ItemInfo/ItemIcon', 'Image'),
    ('text', 'SyncText', prefix+'/NumVisible/ItemNumber', 'Text'),
    ('locked', 'SyncVisible', 'ItemSlot/Lock', 'RectTransform'),
    ('amount', 'SyncImageAmount', 'ItemSlot/Mask', 'Image'),
    ('select', 'SyncVisible', prefix+'/ItemIconSelect', 'RectTransform'),
    ('arrow', 'SyncVisible', 'ItemSlot/Arrow', 'RectTransform'),
    ('edge', 'SyncColor', prefix+'/ItemInfo/ItemRare', 'Image'),
    ('iconVisible', 'SyncVisible', prefix+'/ItemInfo/ItemIcon', 'RectTransform')
]
bind(slot, 'ItemSlot', 'VMItemSlot', fields)
# Retain the old skin and its original GUID, including its TMP label binding.
legacyFields = [(n,s,'ItemSlot/'+p,c) for n,s,p,c in [
    ('back','SyncImage','Back','Image'),('icon','SyncSprite','Icon','Image'),('text','SyncTextMeshPro','Text','TMPro.TextMeshProUGUI'),
    ('locked','SyncVisible','Lock','RectTransform'),('amount','SyncImageAmount','Mask','Image'),('select','SyncVisible','Select','RectTransform'),
    ('arrow','SyncVisible','Arrow','RectTransform'),('edge','SyncColor','Edge','Image'),('iconVisible','SyncVisible','Icon','RectTransform')
]]
bind(legacy, 'ItemSlot', 'VMItemSlot', legacyFields)
reward = 'Assets/Data/Widgets/Common/Micros/Reward/RewardEntry.prefab'
attach(reward, 'RewardEntry', slot, 'BagIconNew')
bind(reward, 'RewardEntry', 'VMRewardEntry', [
    ('item','SyncMetadata','RewardEntry/BagIconNew','Oathx.Widget.UIItemSlot'),
    ('claimed','SyncVisible','RewardEntry/HadReward','RectTransform')
])
(report/'bindings.json').write_text(json.dumps({'itemSlot':fields,'legacy':legacyFields,'reward':['item','claimed']},indent=2),encoding='utf-8')
print('ItemSlot and nested Reward bindings updated.')
