import {readFile,writeFile,mkdir} from 'node:fs/promises';
import {createHash} from 'node:crypto';
import {fileURLToPath} from 'node:url';
import {join} from 'node:path';
import {createWorkspaces} from '../../../../../integrations/codex/server/workspaces.mjs';
import {createTemplateStore} from '../../../../../integrations/codex/server/templates.mjs';
import {nodeName} from '../../../../../integrations/codex/shared/design.mjs';
const run=fileURLToPath(new URL('.',import.meta.url));
const w=createWorkspaces(),store=createTemplateStore();
const projectId='p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484';
try{
 const binding=await w.unityBindings.requireBinding(projectId),snapshot=await w.snapshot(projectId);
 await writeFile(join(run,'project-baseline.json'),JSON.stringify(snapshot,null,2));
 for(const templateId of ['SvStart','SvLogin','SvNotice','SvLoading']){
  const template=JSON.parse(await readFile(join(run,'templates',templateId+'.json'),'utf8'));
  const design=template.payload.design,uiName=nodeName(design.nodes.find(n=>n.id===template.payload.root));
  const directory=join(run,'inputs',uiName);await mkdir(join(directory,'images'),{recursive:true});
  const assets=[];
  for(const hash of new Set(design.nodes.map(n=>n.image).filter(Boolean))){
   if(!await w.assetAllowed(hash,projectId))throw Error('Asset access denied');
   const asset=await store.asset(hash);if(!asset||asset.mime!=='image/png')throw Error('Missing PNG '+hash);
   if(createHash('sha256').update(asset.body).digest('hex')!==hash)throw Error('Asset hash mismatch');
   await writeFile(join(directory,'images',hash+'.png'),asset.body);
   assets.push({id:hash,file:'images/'+hash+'.png',path:'Assets/Data/Widgets/'+uiName+'/Images/'+hash+'.png',usedBy:design.nodes.filter(n=>n.image===hash).map(n=>n.id)});
  }
  function node(n,px=0,py=0){
   const x=px+n.x,y=py+n.y;
   return {id:n.id,name:nodeName(n),element:n.type==='Container'?'Container':n.type==='Progress'?'Image':n.type,
    rect:{x,y,width:n.width,height:n.height},style:{color:n.surface==='transparent'&&!n.image?'#ffffff00':n.color,opacity:n.opacity??1,sprite:n.image??'',imageType:n.imageBorder?'sliced':'simple'},
    text:n.type==='Text'?{content:n.text??'',fontSize:n.fontSize??20,color:n.color,alignment:({left:'Left',center:'Center',right:'Right'}[n.textAlign??'left'])}:null,
    children:design.nodes.filter(c=>c.parent===n.id).map(c=>node(c,x,y))};
  }
  await writeFile(join(directory,uiName+'.visual-ui.json'),JSON.stringify({schemaVersion:'1.0',uiName,referenceResolution:{width:design.width,height:design.height},nodes:[node(design.nodes.find(n=>n.id===template.payload.root))]},null,2));
  await writeFile(join(directory,'asset-manifest.json'),JSON.stringify({schemaVersion:'1.0',uiName,referenceResolution:{width:design.width,height:design.height},assets},null,2));
  await writeFile(join(directory,'source-design.json'),JSON.stringify(design,null,2));
  console.log(JSON.stringify({templateId,uiName,nodes:design.nodes.length,assets:assets.length,directory,unityProject:binding.path}));
 }
}finally{await store.close();await w.close();}
