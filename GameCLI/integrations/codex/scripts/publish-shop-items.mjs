// Publish only the current Shop composition; preserve unrelated designs and project edits.
import assert from 'node:assert/strict';
import {readFile,writeFile,readdir,mkdir} from 'node:fs/promises';
import {createHash} from 'node:crypto';
import {createWorkspaces} from '../server/workspaces.mjs';
import {openDatabase} from '../server/database.mjs';
import {validateScripts} from '../server/template-scripts.mjs';
const w=createWorkspaces(),db=await openDatabase(),projectId='p-2d314388-18c4-42f5-80eb-f6729dd23f0e';
const hash=b=>createHash('sha256').update(b).digest('hex');
try {
 const {path:project}=await w.unityBindings.requireBinding(projectId);
 const row=(await db.pool.query("SELECT * FROM gamecli_ui.templates WHERE id='SvShop'")).rows[0];assert.equal(row.version,4);
 const slot=(await db.pool.query("SELECT version FROM gamecli_ui.templates WHERE id='ItemSlot'")).rows[0];assert.equal(slot.version,3);
 const payload=structuredClone(row.payload),files=row.scripts.files.map(f=>({path:f.path,content:f.content}));
 for(const file of files)if(file.path.startsWith('Assets/Scripts/Systems/Shop/')) file.content=await readFile(project+'/'+file.path,'utf8');
 const compositions=JSON.parse(await readFile('reports/shop-item-slot/bindings.json','utf8'));
 const bindings=JSON.parse(files.find(f=>f.path.endsWith('/bindings.json')).content);
 for(const c of compositions){
  const spec=bindings.specs.find(s=>s.name==='Shop'+c.kind+'Item');
  spec.prefab=c.card;spec.standaloneRoot=c.root;
  spec.fields=spec.fields.filter(f=>f.name!=='count').map(f=>f.name==='icon'?{name:'item',type:'VMItemSlot',sync:'SyncMetadata',component:'Oathx.Widget.UIItemSlot',targets:[c.slot.replace(c.root,c.cell)]}:f);
 }
 files.find(f=>f.path.endsWith('/bindings.json')).content=JSON.stringify(bindings,null,2);
 const materialFile=files.find(f=>f.path.endsWith('/materials.json')),materials=JSON.parse(materialFile.content);
 for(const f of materials.files){
  if(!f.path.startsWith('Assets/')||f.assetId)continue;
  const body=await readFile(project+'/'+f.path);f.content=body.toString('utf8');f.sha256=hash(body);
 }
 async function walk(dir){const out=[];for(const e of await readdir(project+'/'+dir,{withFileTypes:true})){const p=dir+'/'+e.name;out.push(...e.isDirectory()?await walk(p):[p]);}return out;}
 for(const name of ['ShopRechargeItem','ShopGiftItem']){
  const folder='Assets/Data/Widgets/'+name;
  for(const path of [folder+'.meta',...await walk(folder)]){
   const body=await readFile(project+'/'+path);materials.files.push({path,encoding:'utf8',content:body.toString('utf8'),sha256:hash(body)});
  }
 }
 materials.dependencies=[{templateId:'ItemSlot',version:3}];materialFile.content=JSON.stringify(materials);
 const integration=[
  'Install ItemSlot v3 first. Both product cards compose the shared UIItemSlot/VMItemSlot through SyncMetadata. Icon is a string asset path resolved by SyncSprite/UILoader; Gift quantity is Item.Text.',
  'ShopRechargeItem/ShopRechargeItem.prefab and ShopGiftItem/ShopGiftItem.prefab are independent native product cards. The Shop pages reference nested instances through Fancy.cellPrefab and ProductTemplate. Root source cells remain inactive list templates; a standalone host activates its instance after binding.',
  'Card code stays in Shop/UI and Shop/VM. Names, prices, first-pay display, purchase limits and sold-out markers belong to card VMs; purchases belong to ShopEntity. The purchase confirmation page is unchanged.',
  'Retain source card artwork and entrance/click animations. Shop-specific nested overrides suppress ItemSlot default backgrounds/rarity borders; recharge also suppresses the quantity and white base. Gift quantity uses dark text on the original white base. Each card keeps one outer Button and inner item Graphics do not intercept clicks.',
  'Unref temporarily clears Item to release its child subscription, unsubscribes the card, and restores Item for list reuse. Shared SyncSprite cancellation/out-of-order behavior is not claimed as fixed by this composition.'
 ];
 payload.configurations.dependencies.push({kind:'template',name:'ItemSlot',templateId:'ItemSlot',version:3,required:true,reason:'Shared item slot code, prefab and resource closure; restore before Shop.'});
 payload.configurations.integration.push(...integration);
 files.find(f=>f.path.endsWith('/README.md')).content+='\n\n## Independent product cards\n\n'+integration.join('\n\n');
 files.push({path:'GameCLI/Templates/Shop/product-cards.json',content:JSON.stringify({schemaVersion:1,dependencies:materials.dependencies,cards:compositions,integration},null,2)});
 // Companion visual designs remain the reference layout. Native composition is explicit.
 for(const kind of ['recharge','gift']){
  const f=files.find(f=>f.path.endsWith('/Designs/'+kind+'.json'));const design=JSON.parse(f.content);
  design.nativeComposition=compositions.find(c=>c.kind.toLowerCase()===kind);f.content=JSON.stringify(design);
 }
 const contract=files.find(f=>f.path.endsWith('/contract.json'));const c=JSON.parse(contract.content);c.productCards=compositions;contract.content=JSON.stringify(c,null,2);
 const scripts=validateScripts({schemaVersion:1,files});
 await mkdir('.data/shop-item-slot',{recursive:true});await writeFile('.data/shop-item-slot/candidate.json',JSON.stringify({payload,scripts}));
 await writeFile('reports/shop-item-slot/updated-bindings.json',JSON.stringify(bindings,null,2));
 if(!process.argv.includes('--apply')){console.log(JSON.stringify({dryRun:true,template:'SvShop',baseVersion:4,files:files.length,materials:materials.files.length}));}
 else{
  const published=await w.templateEditor.published('SvShop');assert.equal(published.revision,4);
  const root=payload.design.nodes.find(n=>n.parent===null);root.templateSourceId='SvShop';root.scripts=scripts;root.configurations=payload.configurations;
  const projectSnapshot=await w.snapshot(projectId);await writeFile('.data/shop-item-slot/project-before.json',JSON.stringify(projectSnapshot));
  const instances=projectSnapshot.design.nodes.filter(n=>n.parent===null&&n.templateSourceId==='SvShop');
  for(const instance of instances){assert.equal(instance.templateSourceVersion,4);assert.deepEqual(instance.scripts,row.scripts);}
  const opened=await w.templateEditor.open({templateId:'SvShop'}),draft=await w.templateEditor.read(opened.id);assert.deepEqual(draft.design,published.design);
  const saved=await w.templateEditor.save(opened.id,{draftId:opened.id,revision:draft.revision,design:payload.design});
  const candidate=await w.templateEditor.submit(opened.id,{draftId:opened.id,revision:saved.revision});const result=await w.publish({candidateId:candidate.id,confirm:true});
  for(const instance of instances){instance.scripts=scripts;instance.configurations=payload.configurations;instance.templateSourceVersion=result.version;}
  const updated=await w.save(projectId,{projectId,revision:projectSnapshot.revision,projectVersion:projectSnapshot.project.version,design:projectSnapshot.design});
  const actual=(await db.pool.query("SELECT * FROM gamecli_ui.templates WHERE id='SvShop'")).rows[0];assert.deepEqual(actual.scripts,scripts);
  await writeFile('.data/shop-item-slot/final.json',JSON.stringify(actual));
  const evidence={verified:true,template:'SvShop',version:result.version,itemSlotVersion:3,projectRevision:updated.revision,instances:instances.map(n=>n.id)};
  await writeFile('reports/shop-item-slot/publication.json',JSON.stringify(evidence,null,2));console.log(JSON.stringify(evidence));
 }
}finally{await w.close();await db.close();}
