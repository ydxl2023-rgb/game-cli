import assert from 'node:assert/strict';
import {readFile,writeFile,mkdir} from 'node:fs/promises';
import {createHash} from 'node:crypto';
import {createWorkspaces} from '../server/workspaces.mjs';
import {openDatabase} from '../server/database.mjs';
import {validateScripts} from '../server/template-scripts.mjs';
const db=await openDatabase(),w=createWorkspaces(),projectId='p-2d314388-18c4-42f5-80eb-f6729dd23f0e';
const hash=s=>createHash('sha256').update(s).digest('hex');
try{
 const migration=JSON.parse(await readFile('reports/common-micros/migration.json','utf8'));
 const {path:project}=await w.unityBindings.requireBinding(projectId);
 const movedFiles=new Set(migration.files.map(f=>f.to));
 const rows=JSON.parse(await readFile('.data/common-micros/templates-before.json','utf8'));
 const moved=Object.entries(migration.directories);
 const hasPath=s=>moved.some(([a])=>s.includes(a+'/')||s.includes(a+'.meta')||s.includes(a+'`')||s.includes(a+'"'));
 const selected=rows.filter(r=>hasPath(JSON.stringify([r.payload,r.scripts])));
 const versions=Object.fromEntries(selected.map(r=>[r.id,r.version+1]));
 const names=Object.fromEntries(selected.map(r=>[r.id,[r.version,r.version+1]]));
 function text(s){s=s.replaceAll('Widgets/Micros','Widgets/Common/Micros');for(const [a,b]of moved)s=s.replaceAll(a+'/',b+'/').replaceAll(a+'.meta',b+'.meta').replaceAll(a+'`',b+'`').replaceAll(a+'"',b+'"');for(const [id,[old,next]]of Object.entries(names))s=s.replaceAll(id+' v'+old,id+' v'+next);return s;}
 function transform(value){
  if(typeof value==='string')return text(value);
  if(Array.isArray(value))return value.map(transform);
  if(!value||typeof value!=='object')return value;
  const obj=Object.fromEntries(Object.entries(value).map(([k,v])=>[k,transform(v)]));
  if(obj.templateId&&versions[obj.templateId]&&typeof obj.version==='number')obj.version=versions[obj.templateId];
  if(obj.templateSourceId&&versions[obj.templateSourceId]&&typeof obj.templateSourceVersion==='number')obj.templateSourceVersion=versions[obj.templateSourceId];
  if(typeof obj.content==='string'&&obj.sha256)obj.sha256=hash(obj.content);
  return obj;
 }
 const candidates=[];
 for(const row of selected){
  const payload=transform(row.payload),files=row.scripts.files.map(f=>{
   // Historical migrations retain their original source paths. A separate current directory map follows them.
   if(/(?:migration|resource-moves)\.json$/.test(f.path))return {path:f.path,content:f.content};
   let content=f.content;
   if(f.path.endsWith('.json')){content=JSON.stringify(transform(JSON.parse(content)));}
   else content=text(content);
   return {path:text(f.path),content};
  });
  files.push({path:'GameCLI/Templates/'+row.id+'/common-micros-migration.json',content:JSON.stringify({schemaVersion:1,directories:migration.directories,policy:'Move existing assets with Unity AssetDatabase, preserving GUIDs; clean installs restore the current materials paths. Do not overwrite the legacy Micros/ItemSlot.prefab.'},null,2)});
  // Unity's Metadata Inspector can refresh its non-semantic hash cache on import.
  // Verify that this is the only content difference before snapshotting current assets.
  for(const file of files.filter(f=>f.path.endsWith('/materials.json'))){
   const material=JSON.parse(file.content);
   for(const entry of material.files??[])if(movedFiles.has(entry.path)&&typeof entry.content==='string'){
    const current=await readFile(project+'/'+entry.path,'utf8');
    const normalize=s=>s.replace(/(\bhash: )-?\d+/g,'$1<CACHE>');
    assert.equal(normalize(current),normalize(entry.content),'Unexpected asset change: '+entry.path);
    entry.content=current;entry.sha256=hash(current);
   }
   file.content=JSON.stringify(material);
  }
  const scripts=validateScripts({schemaVersion:1,files});
  const root=payload.design.nodes.find(n=>n.parent===null);root.templateSourceId=row.id;root.scripts=scripts;if(payload.configurations)root.configurations=payload.configurations;
  candidates.push({id:row.id,baseVersion:row.version,version:versions[row.id],payload,scripts});
 }
 await mkdir('.data/common-micros/candidates',{recursive:true});
 for(const c of candidates)await writeFile('.data/common-micros/candidates/'+c.id+'.json',JSON.stringify(c));
 const live=await w.snapshot(projectId),instances=live.design.nodes.filter(n=>n.parent===null&&versions[n.templateSourceId]);
 for(const n of instances){const old=selected.find(r=>r.id===n.templateSourceId);assert.equal(n.templateSourceVersion,old.version);assert.deepEqual(n.scripts,old.scripts,'Preserve customized instance '+n.id);}
 const summary={templates:candidates.map(c=>({id:c.id,from:c.baseVersion,to:c.version})),instances:instances.map(n=>n.id)};
 await writeFile('reports/common-micros/candidates.json',JSON.stringify(summary,null,2));
 if(!process.argv.includes('--apply'))console.log(JSON.stringify({dryRun:true,...summary}));
 else{
  // Publish prerequisites before consumers. No designs or script contents unrelated to paths are replaced.
  const publishedIds=new Set(),pending=[...candidates],results=[];
  await mkdir('.data/common-micros/final',{recursive:true});
  while(pending.length){
   const index=pending.findIndex(c=>(c.payload.configurations?.dependencies??[]).every(d=>{const id=typeof d==='string'?d:d.templateId;return !versions[id]||id===c.id||publishedIds.has(id);}));assert(index>=0,'Dependency cycle');const c=pending.splice(index,1)[0];
   const published=await w.templateEditor.published(c.id);assert.equal(published.revision,c.baseVersion);
   const opened=await w.templateEditor.open({templateId:c.id}),draft=await w.templateEditor.read(opened.id);assert.deepEqual(draft.design,published.design,'Preserve edited draft '+c.id);
   const saved=await w.templateEditor.save(opened.id,{draftId:opened.id,revision:draft.revision,design:c.payload.design});
   const candidate=await w.templateEditor.submit(opened.id,{draftId:opened.id,revision:saved.revision});const result=await w.publish({candidateId:candidate.id,confirm:true});assert.equal(result.version,c.version);
   const actual=(await db.pool.query('SELECT * FROM gamecli_ui.templates WHERE id=$1',[c.id])).rows[0];assert.deepEqual(actual.scripts,c.scripts);
   await writeFile('.data/common-micros/final/'+c.id+'.json',JSON.stringify(actual));publishedIds.add(c.id);results.push({id:c.id,version:result.version});console.log(JSON.stringify(results.at(-1)));
   await writeFile('reports/common-micros/published-progress.json',JSON.stringify(results,null,2));
  }
  const snapshot=await w.snapshot(projectId);await writeFile('.data/common-micros/project-prepublish.json',JSON.stringify(snapshot));
  for(const n of snapshot.design.nodes.filter(n=>n.parent===null&&versions[n.templateSourceId])){
   const old=selected.find(r=>r.id===n.templateSourceId),c=candidates.find(r=>r.id===n.templateSourceId);assert.deepEqual(n.scripts,old.scripts);assert.equal(n.templateSourceVersion,old.version);
   n.scripts=c.scripts;n.configurations=c.payload.configurations;n.templateSourceVersion=c.version;
  }
  const saved=await w.save(projectId,{projectId,revision:snapshot.revision,projectVersion:snapshot.project.version,design:snapshot.design});
  await writeFile('reports/common-micros/publication.json',JSON.stringify({verified:true,templates:results,projectRevision:saved.revision},null,2));console.log(JSON.stringify({projectRevision:saved.revision,verified:true}));
 }
}finally{await w.close();await db.close();}

