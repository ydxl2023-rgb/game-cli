import {resolve} from 'node:path';
import {isDeepStrictEqual} from 'node:util';
import {readSourcePackage} from '../server/source-package.mjs';
import {createWorkspaces} from '../server/workspaces.mjs';
import {createTemplateStore} from '../server/templates.mjs';
const args=process.argv.slice(2),manifest=args.find(a=>!a.startsWith('--'));
if(!manifest||args.some(a=>a.startsWith('--')&&!['--apply','--candidate'].includes(a)))throw Error('用法：npm run scripts:sync -- <manifest.json> [--apply] [--candidate]；默认仅预检。');
const source=await readSourcePackage(resolve(manifest)),workspaces=createWorkspaces(),templates=createTemplateStore();
try{
 if(!source.projectId)throw Error('源码清单必须明确指定 projectId，禁止写入隐式当前项目。');
 const snapshot=await workspaces.snapshot(source.projectId),roots=snapshot.design.nodes.filter(n=>n.parent===null&&n.templateSourceId===source.templateId&&(!source.rootName||n.name===source.rootName));
 if(roots.length!==1)throw Error('请用 rootName 唯一指定项目中的系统。');
 const root=roots[0],changed=!isDeepStrictEqual(root.scripts,source.scripts);root.scripts=source.scripts;
 let saved=snapshot,candidate=null;
 if(args.includes('--apply')&&changed){const result=await workspaces.save(source.projectId,{projectId:source.projectId,design:snapshot.design,revision:snapshot.revision,projectVersion:snapshot.project.version});saved={...result,project:{version:result.projectSave.version}};}
 if(args.includes('--candidate')){if(!args.includes('--apply'))throw Error('提交候选需同时指定 --apply。');const current=(await templates.list()).templates.find(t=>t.id===source.templateId);candidate=await workspaces.candidate(source.projectId,{projectId:source.projectId,design:snapshot.design,revision:saved.revision,projectVersion:saved.project.version,rootId:root.id,templateId:source.templateId,version:current.version});}
 console.log(JSON.stringify({projectId:source.projectId,projectVersion:saved.project.version,changed,applied:args.includes('--apply')&&changed,candidate,standardTemplateUnchanged:true,files:source.scripts.files.map(({path,sha256})=>({path,sha256}))},null,2));
}finally{await workspaces.close();await templates.close();}
