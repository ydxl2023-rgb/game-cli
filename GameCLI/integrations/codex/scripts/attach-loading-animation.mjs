import {readFile,writeFile,mkdir} from 'node:fs/promises';
import {isDeepStrictEqual} from 'node:util';
import {createStore} from '../server/store.mjs';
import {validateDesign} from '../shared/design.mjs';
const evidence=JSON.parse(await readFile('.data/imports/sv-loading/loading-animation.json','utf8'));
const store=createStore(),snapshot=await store.snapshot(),design=structuredClone(snapshot.design);
const roots=design.nodes.filter(n=>n.parent===null&&n.name==='Loading');
if(roots.length!==1)throw new Error('Expected exactly one Loading instance');
const root=roots[0],hosts=design.nodes.filter(n=>n.parent===root.id&&n.animationHost);
if(hosts.length!==1)throw new Error('Expected one Animator host');
const host=hosts[0];
function resolve(path){let id=host.id;for(const name of path.split('/')){const matches=design.nodes.filter(n=>n.parent===id&&n.name===name);if(matches.length!==1)throw new Error('Missing/ambiguous binding: '+path);id=matches[0].id;}return id;}
const a=structuredClone(evidence.designAnimationsProposal[0]);
delete a.hostSelector;a.hostNodeId=host.id;a.id=host.id+'LoadingLoop';a.source=evidence.source;
for(const curve of a.clip.curves){curve.targetNodeId=resolve(curve.bindingPathPreview);delete curve.bindingPathPreview;}
design.animations=[...(design.animations??[]).filter(old=>old.id!==a.id),a];
const normalized=validateDesign(design);
if(!isDeepStrictEqual(normalized.nodes,snapshot.design.nodes))throw new Error('Animation attachment would change layout');
await mkdir('.data/template-history',{recursive:true});
await writeFile(`.data/template-history/draft-before-animation-${snapshot.revision}.json`,JSON.stringify(snapshot,null,2));
const saved=await store.save(normalized,snapshot.revision);
console.log(JSON.stringify({revision:saved.revision,animations:saved.design.animations.length,curves:a.clip.curves.length,layoutUnchanged:true}));
