"""Promote only ItemSlot-owned resources out of Reward, preserving asset GUIDs."""
import json
import pathlib
import re
import subprocess

project=pathlib.Path('G:/Works/GameCLI/GameCLI/examples/unitys/development/client')
cli='G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
def run(args):
    p=subprocess.run([cli,'--project',str(project),'--format','json',*args],capture_output=True,encoding='utf-8-sig')
    result=json.loads(p.stdout)
    assert result['ok'],result
    return result

guids={}
for meta in (project/'Assets').rglob('*.meta'):
    m=re.search(r'^guid: (\w+)',meta.read_text(encoding='utf-8-sig'),re.M)
    if m:guids[m[1]]=meta.with_suffix('')
pending=[project/'Assets/Data/Widgets/ItemSlot/ItemSlot.prefab'];seen=set()
while pending:
    path=pending.pop()
    if path in seen or not path.is_file():continue
    seen.add(path)
    if path.suffix not in ['.prefab','.controller','.anim','.mat','.asset']:continue
    for guid in re.findall(r'guid: (\w+)',path.read_text(encoding='utf-8-sig')):
        if guid in guids:pending.append(guids[guid])
run(['asset','create-folder','--dst','Assets/Data/Widgets/ItemSlot/Resources'])
moves={}
for path in sorted(seen):
    src=path.relative_to(project).as_posix()
    if not src.startswith('Assets/Data/Widgets/Reward/Resources/'):continue
    dst='Assets/Data/Widgets/ItemSlot/Resources/'+path.name
    before=pathlib.Path(str(path)+'.meta').read_bytes()
    run(['asset','move','--src',src,'--dst',dst])
    assert pathlib.Path(str(project/dst)+'.meta').read_bytes()==before
    moves[src]=dst
example=project/'Assets/Data/Widgets/Mail/MailExample.json'
text=example.read_text(encoding='utf-8-sig')
for src,dst in moves.items():text=re.sub(re.escape(src),lambda _:dst,text,flags=re.I)
example.write_text(text,encoding='utf-8')
closure=[moves.get(p.relative_to(project).as_posix(),p.relative_to(project).as_posix()) for p in sorted(seen)]
pathlib.Path('reports/item-slot/resource-moves.json').write_text(json.dumps(moves,indent=2),encoding='utf-8')
pathlib.Path('reports/item-slot/dependencies.json').write_text(json.dumps(closure,indent=2),encoding='utf-8')
print(json.dumps({'moved':len(moves),'dependencies':len(closure)}))
