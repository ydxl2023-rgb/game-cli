import fs from 'node:fs';
import path from 'node:path';
import crypto from 'node:crypto';
import {fileURLToPath} from 'node:url';
const out=path.dirname(fileURLToPath(import.meta.url));
const base=path.dirname(out);
const read=p=>JSON.parse(fs.readFileSync(path.join(base,p),'utf8'));
const hash=b=>crypto.createHash('sha256').update(b).digest('hex');
const project=read('project-snapshot.json');
const design=project.payload.design;
const roots=design.nodes.filter(n=>!n.parent||n.parent===design.rootId);
function normalized(input){
 const map=new Map(input.nodes.map(n=>[n.id,n]));
 const name=n=>n.name??n.id;
 const route=n=>map.has(n.parent)?route(map.get(n.parent))+'/'+name(n):name(n);
 return input.nodes.map(n=>({path:route(n),node:n}));
}
function verifyPackage(pkg){
 return (pkg?.files??[]).map(f=>{
  if(f.content===undefined){
   const name=fs.readdirSync(path.join(base,'assets')).find(n=>n.startsWith(f.assetId+'.'));
   const actual=name?hash(fs.readFileSync(path.join(base,'assets',name))):null;
   return {path:f.path,declared:f.sha256,actual,match:actual===f.sha256,status:name?'external-material-verified':'external-material-missing',assetId:f.assetId};
  }
  const b=Buffer.from(f.content,f.encoding==='base64'?'base64':'utf8');
  return {path:f.path,encoding:f.encoding??'utf8',bytes:b.length,declared:f.sha256,actual:hash(b),match:hash(b)===f.sha256};
 });
}
const report={createdAt:new Date().toISOString(),projectVersion:project.version,projectHash:hash(fs.readFileSync(path.join(base,'project-snapshot.json'))),scope:'Frozen source only. No Unity delivery verdict, no Editor operation.',roots:[],homeShop:[]};
for(const r of roots){
 const standard=read('standards/'+r.templateSourceId+'.json');
 report.roots.push({name:r.name,projectTemplateVersion:r.templateSourceVersion,standardVersion:standard.version,projectCode:verifyPackage(r.scripts),standardCode:verifyPackage(standard.scripts)});
 if(!['Home','Shop'].includes(r.name))continue;
 const instance=read('inputs/'+r.name+'/input.json');
 const a=normalized(instance.design),b=normalized(standard.payload.design);
 const bmap=new Map(b.map(x=>[x.path,x.node]));
 const amap=new Map(a.map(x=>[x.path,x.node]));
 const differences=[];
 const keys=['type','x','y','width','height','rectTransform','localScale','color','opacity','active','text','fontSize','image','fontAsset','raycastTarget'];
 for(const {path:p,node} of a){
  const other=bmap.get(p);if(!other){differences.push({path:p,kind:'project-only'});continue;}
  for(const key of keys)if(JSON.stringify(node[key])!==JSON.stringify(other[key]))differences.push({path:p,key,project:node[key],standard:other[key]});
 }
 for(const {path:p} of b)if(!amap.has(p))differences.push({path:p,kind:'standard-only'});
 const materialFile=r.scripts?.files?.find(f=>f.path.endsWith('/materials.json'));
 const materials=materialFile?JSON.parse(materialFile.content):null;
 const checks=verifyPackage(materials);
 report.homeShop.push({name:r.name,projectNodes:a.length,standardNodes:b.length,differences,materialFiles:checks,download:materials?.download,limits:'Path/name normalized comparison is supplementary; repeated names and root prefixes require interpretation. Native materials are references, not proof of installed GUID binding or runtime visual parity.'});
}
fs.writeFileSync(path.join(out,'source-audit.json'),JSON.stringify(report,null,2));
console.log(JSON.stringify({projectVersion:report.projectVersion,roots:report.roots.map(r=>({name:r.name,projectVersion:r.projectTemplateVersion,standardVersion:r.standardVersion,badProject:r.projectCode.filter(f=>!f.match).length,badStandard:r.standardCode.filter(f=>!f.match).length})),homeShop:report.homeShop.map(r=>({name:r.name,differences:r.differences.length,materialFiles:r.materialFiles.length,badMaterials:r.materialFiles.filter(f=>!f.match).length}))},null,2));
