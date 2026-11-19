import assert from 'node:assert/strict';
import {readFile,writeFile,readdir} from 'node:fs/promises';
import {createHash} from 'node:crypto';
const root='G:/Works/GameCLI/GameCLI/examples/unitys/development/client',hash=b=>createHash('sha256').update(b).digest('hex');
const migration=JSON.parse(await readFile('reports/micros-directory/migration.json','utf8'));
const moved=new Set(migration.files.map(f=>f.to));let checked=0;const mismatches=[];
for(const name of await readdir('.data/micros-directory/candidates')){
 const c=JSON.parse(await readFile('.data/micros-directory/candidates/'+name,'utf8'));
 for(const f of c.scripts.files){
  if(!f.path.endsWith('/materials.json'))continue;
  for(const m of JSON.parse(f.content).files??[]){
   if(!moved.has(m.path)&&m.path!=='Assets/Data/Widgets/Mail/MailExample.json')continue;
   const bytes=await readFile(root+'/'+m.path);checked++;
   if(hash(bytes)!==m.sha256)mismatches.push({template:c.id,path:m.path});
  }
 }
}
const report={verified:mismatches.length===0,checked,mismatches};await writeFile('reports/micros-directory/candidate-materials.json',JSON.stringify(report,null,2));console.log(JSON.stringify(report));
