// Extend the generated Home source with explicit local presentation state.
import {readFile,writeFile} from 'node:fs/promises';
import {createHash} from 'node:crypto';
const folder=process.env.HOME_OUTPUT??'.data/imports/sv-home/fidelity-candidate';
const base=folder+'/source/Assets/Scripts/Systems/Home/';
const contract=JSON.parse(await readFile(folder+'/presentation.json','utf8'));
const redDots=JSON.parse(await readFile('config/home-red-dots.json','utf8'));
const redDotNodes=new Set(redDots.map(s=>s.node));
contract.redDots=redDots;
const states=contract.states.filter(s=>s.sourcePath!=='0/6/5/0/4'&&!redDotNodes.has(s.node));
const names=new Set();
for(const state of states){
  let name=state.name.replace(/[^a-zA-Z0-9]/g,'');
  name=name[0].toUpperCase()+name.slice(1);
  if(names.has(name))name+=state.sourcePath.replaceAll('/','');
  names.add(name);state.key=name;
  state.value=createHash('sha256').update(state.sourcePath).digest().readUInt32BE(0)&0x7fffffff;
}
// C# remains maintained source. Extracted nodes are presentation evidence, not business API definitions.
const badges=JSON.parse(await readFile('config/home-entry-badges.json','utf8'));
contract.entryBadges=badges;
// Source visibility is evidence only; runtime decoration state is keyed by entry IDs.
contract.states=states.filter(state=>state.name==='TaskEffectTips');
await writeFile(folder+'/presentation.json',JSON.stringify(contract,null,2));
console.log(JSON.stringify({entries:badges.entries.length,mounts:badges.mounts.length,redDots:redDots.length}));
