// Export maintained template code; extraction paths must never generate business fields.
import {readFile,writeFile,mkdir,readdir} from 'node:fs/promises';
import {join,dirname} from 'node:path';
import {createWorkspaces} from '../server/workspaces.mjs';
const folder=process.env.HOME_OUTPUT??'.data/imports/sv-home/fidelity-candidate';
const w=createWorkspaces();
try {
 const binding=await w.unityBindings.requireBinding('p-2d314388-18c4-42f5-80eb-f6729dd23f0e');
 let count=0;
 async function copy(relative){
  const absolute=join(binding.path,relative);
  for(const item of await readdir(absolute,{withFileTypes:true})){
   const path=relative+'/'+item.name;
   if(item.isDirectory())await copy(path);
   else {
    const target=join(folder,'source',path);
    await mkdir(dirname(target),{recursive:true});
    await writeFile(target,await readFile(join(binding.path,path)));
    count++;
   }
  }
 }
 for(const name of ['Home','Function'])await copy('Assets/Scripts/Systems/'+name);
 console.log(JSON.stringify({files:count,source:'verified Unity project; maintained Widget code'}));
}finally{await w.close();}
