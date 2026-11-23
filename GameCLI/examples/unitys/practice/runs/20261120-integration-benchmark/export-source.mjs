import {openDatabase} from '../../../../../integrations/codex/server/database.mjs';
import {mkdir,writeFile} from 'node:fs/promises';
import {fileURLToPath} from 'node:url';
import path from 'node:path';
import {createHash} from 'node:crypto';
const out=path.dirname(fileURLToPath(import.meta.url));
const save=async(p,data)=>{await mkdir(path.dirname(p),{recursive:true});await writeFile(p,typeof data==='string'||Buffer.isBuffer(data)?data:JSON.stringify(data,null,2));};
const hash=b=>createHash('sha256').update(b).digest('hex');
const db=await openDatabase();
try{
 const project=(await db.pool.query('SELECT version,payload,updated_at FROM gamecli_ui.project_ui WHERE project_id=$1',['p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484'])).rows[0];
 await save(path.join(out,'project-snapshot.json'),project);
 const design=project.payload.design;
 const roots=design.nodes.filter(n=>!n.parent||n.parent===design.rootId);
 const templates=(await db.pool.query('SELECT id,version,payload,scripts,configurations FROM gamecli_ui.templates WHERE deprecated_at IS NULL')).rows;
 const configs=[]; const assets=new Set(); const manifest=[];
 for(const node of design.nodes){if(node.image)assets.add(node.image);if(node.fontAsset)assets.add(node.fontAsset);}
 async function scripts(folder,pkg){for(const file of pkg?.files??[]){const p=path.resolve(folder,file.path);if(!p.startsWith(path.resolve(folder)+path.sep))throw Error('Unsafe script path'); const bytes=Buffer.from(file.content,'utf8');if(file.sha256&&hash(bytes)!==file.sha256)throw Error('Source hash mismatch '+file.path);await save(p,bytes);}}
 for(const root of roots){
  const ids=new Set([root.id]);let change=true;while(change){change=false;for(const n of design.nodes)if(ids.has(n.parent)&&!ids.has(n.id)){ids.add(n.id);change=true;}}
  const nodes=design.nodes.filter(n=>ids.has(n.id));const animations=(design.animations??[]).filter(a=>ids.has(a.hostNodeId));
  const input={root:root.id,design:{...design,nodes,animations}};
  await save(path.join(out,'inputs',root.name,'input.json'),input);
  await scripts(path.join(out,'sources',root.name),root.scripts);
  if(root.configurations)configs.push({name:root.name,configurations:root.configurations});
  manifest.push({name:root.name,root:root.id,template:root.templateSourceId,version:root.templateSourceVersion,nodeCount:nodes.length,animations:animations.length});
 }
 for(const t of templates){await save(path.join(out,'standards',t.id+'.json'),t);await scripts(path.join(out,'standard-sources',t.id),t.scripts);if(t.configurations)configs.push({name:t.id,configurations:t.configurations});}
 for(const id of assets){const a=(await db.pool.query('SELECT mime,body FROM gamecli_ui.assets WHERE id=$1',[id])).rows[0];if(!a)throw Error('Missing asset '+id);if(hash(a.body)!==id)throw Error('Asset hash mismatch '+id);await save(path.join(out,'assets',id+(a.mime==='image/png'?'.png':a.mime.includes('font')?'.ttf':'.bin')),a.body);}
 for(const c of configs){await save(path.join(out,'configurations',c.name,'configurations.json'),c.configurations);for(const t of c.configurations.tables??[]){if(!t.source?.assetId)continue;const a=(await db.pool.query('SELECT body FROM gamecli_ui.configuration_assets WHERE id=$1',[t.source.assetId])).rows[0];if(!a||hash(a.body)!==t.source.sha256)throw Error('Configuration hash mismatch '+c.name);await save(path.join(out,'configurations',c.name,path.basename(t.source.fileName)),a.body);}}
 await save(path.join(out,'source-manifest.json'),{projectVersion:project.version,exportedAt:new Date().toISOString(),roots:manifest,assets:assets.size,standards:templates.map(t=>({id:t.id,version:t.version}))});
 console.log(JSON.stringify({projectVersion:project.version,roots:manifest,assets:assets.size,templates:templates.length}));
}finally{await db.close();}
