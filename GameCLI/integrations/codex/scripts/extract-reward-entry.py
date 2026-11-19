"""Move the extracted reward's dependency closure through UnityCLI, preserving GUIDs."""
import json
import pathlib
import re
import subprocess

project = pathlib.Path('G:/Works/GameCLI/GameCLI/examples/unitys/development/client')
report = pathlib.Path('reports/reward-entry')
cli = 'G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'

def run(args):
    result = subprocess.run([cli, '--project', str(project), '--format', 'json', *args], capture_output=True, encoding='utf-8-sig')
    value = json.loads(result.stdout)
    assert value['ok'], value
    return value

guids = {}
for meta in (project / 'Assets/Data/Widgets').rglob('*.meta'):
    match = re.search(r'^guid: (\w+)', meta.read_text(encoding='utf-8-sig'), re.M)
    if match:
        guids[match[1]] = meta.with_suffix('')
seen = set()
pending = [project / 'Assets/Data/Widgets/Common/Micros/Reward/RewardEntry.prefab']
while pending:
    path = pending.pop()
    if path in seen or not path.is_file():
        continue
    seen.add(path)
    if path.suffix not in ['.prefab', '.controller', '.anim', '.mat', '.asset']:
        continue
    for guid in re.findall(r'guid: (\w+)', path.read_text(encoding='utf-8-sig')):
        if guid in guids:
            pending.append(guids[guid])
moves = {}
run(['asset', 'create-folder', '--dst', 'Assets/Data/Widgets/Common/Micros/Reward/Resources'])
for path in sorted(seen):
    source = path.relative_to(project).as_posix()
    if not source.startswith('Assets/Data/Widgets/Mail/'):
        continue
    destination = 'Assets/Data/Widgets/Common/Micros/Reward/Resources/' + path.name
    assert not (project / destination).exists(), destination
    before = pathlib.Path(str(path)+'.meta').read_bytes()
    run(['asset', 'move', '--src', source, '--dst', destination])
    assert pathlib.Path(str(project / destination)+'.meta').read_bytes() == before
    moves[source] = destination
(report / 'resource-moves.json').write_text(json.dumps(moves, indent=2), encoding='utf-8')
run(['modify', '--asset', 'Assets/Data/Widgets/Common/Micros/Reward/RewardEntry.prefab', '--path', 'RewardEntry', '--component', 'Metadata', '--property', 'className', '--value', 'VMRewardEntry'])
attach = ['prefab', 'attach', '--parent-prefab', 'Assets/Data/Widgets/Mail/Mail.prefab', '--parent-path', 'Mail/Animator/MailInfoShow/LetterGather/MailAccessory', '--child-prefab', 'Assets/Data/Widgets/Common/Micros/Reward/RewardEntry.prefab', '--child-name', 'Cell', '--replace', '--no-scene-replace']
run(attach + ['--dry-run'])
result = run(attach)
(report / 'attach.json').write_text(json.dumps(result, indent=2), encoding='utf-8')
cell = 'Mail/Animator/MailInfoShow/LetterGather/MailAccessory/Cell'
for path, component, field, value in [
    ('Mail/Animator/MailInfoShow/LetterGather/MailAccessory', 'FancyGridScrollView', 'cellPrefab', '@'+cell+'#FancyGridScrollViewCell'),
    ('Mail', 'Metadata', 'metadatas.Array.data[9].targets.Array.data[0]', '@'+cell),
    ('Mail', 'Metadata', 'metadatas.Array.data[9].components.Array.data[0]', '@'+cell+'#RectTransform')
]:
    run(['modify', '--asset', 'Assets/Data/Widgets/Mail/Mail.prefab', '--path', path, '--component', component, '--property', field, '--value', value])
example = project / 'Assets/Data/Widgets/Mail/MailExample.json'
text = example.read_text(encoding='utf-8-sig')
for source, destination in moves.items():
    text = re.sub(re.escape(source), lambda match: destination, text, flags=re.I)
example.write_text(text, encoding='utf-8')
print(json.dumps({'moved': len(moves), 'attached': result['ok']}))
