import {readFile,writeFile} from 'node:fs/promises';
const base=new URL('./',import.meta.url);const adaptations=[];
for(const name of ['Launch','Login','Notice','Loading']){
 const url=new URL(`inputs/${name}/input.json`,base);const input=JSON.parse(await readFile(url,'utf8'));const nodes=input.design.nodes,map=new Map(nodes.map(n=>[n.id,n]));
 const nodePath=n=>n.parent&&map.has(n.parent)?nodePath(map.get(n.parent))+'/'+n.name:n.name;
 const inventory=nodes.map(n=>({id:n.id,path:nodePath(n),type:n.type,text:n.text,action:n.action}));
 for(const n of nodes){if(n.action?.target&&!map.has(n.action.target)){adaptations.push({system:name,node:n.id,path:nodePath(n),originalAction:n.action,reason:'Cross-system target is outside standalone prefab; runtime coordinator owns this operation.'});delete n.action;}}
 for(const a of input.design.animations??[])for(const c of a.clip?.curves??[])if(!map.has(c.targetNodeId))throw Error('Cross-root animation '+name+' '+c.targetNodeId);
 await writeFile(new URL(`inputs/${name}/generation-input.json`,base),JSON.stringify(input,null,2));
 await writeFile(new URL(`inputs/${name}/node-paths.json`,base),JSON.stringify(inventory,null,2));
}
await writeFile(new URL('input-adaptations.json',base),JSON.stringify(adaptations,null,2));console.log(JSON.stringify(adaptations));
