// Export maintained template code; extraction paths must never generate business fields.
import {readFile,writeFile,mkdir,readdir} from 'node:fs/promises';
import {join,dirname} from 'node:path';
import {createWorkspaces} from '../server/workspaces.mjs';
const folder=process.env.HOME_OUTPUT??'.data/imports/sv-home/fidelity-candidate';
const w=createWorkspaces();
try {
 const binding=await w.unityBindings.requireBinding('p-2d314388-18c4-42f5-80eb-f6729dd23f0e');
 let count=0;
 const entries=JSON.parse(await readFile('config/home-entry-badges.json','utf8')).entries;
 const report=JSON.parse(await readFile(folder+'/report.json','utf8'));
 report.bindings=report.bindings.filter(binding=>! /currency/i.test(binding.name));
 report.bindings.push({name:'CurrencyBar',kind:'VMCurrencyBar',syncType:'SyncMetadata',entryPath:'Home/Animator/RootView/TopRight/ItemToolTips'});
 report.bindings=report.bindings.filter(binding=>!['MailEntry','SettingsEntry','NoticeEntry','FriendsEntry','AffinityEntry','ProfileShortcuts'].includes(binding.name));
 report.bindings.push({name:'ProfileShortcuts',kind:'VMProfileShortcuts',syncType:'SyncMetadata',entryPath:'Home/Animator/RootView/TopLeft/ProfileShortcuts'});
 report.bindings=report.bindings.filter(binding=>!['ProfileEntry','AvatarEntry','PlayerName','PlayerLevel','Profile'].includes(binding.name));
 report.bindings.push({name:'Profile',kind:'VMProfile',syncType:'SyncMetadata',entryPath:'Home/Animator/RootView/TopLeft/ProfileView'});
 report.bindings=report.bindings.filter(binding=>!['DropSprite','DropSize','DropVisible'].includes(binding.name));
 for(const [name,kind,syncType] of [['DropSprite','Sprite','SyncSprite'],['DropSize','Vector2','SyncSizeDelta'],['DropVisible','bool','SyncVisible']])
  report.bindings.push({name,kind,syncType,entryPath:'Home/Animator/RootView/Right/Matches/ImgDoubleReward'});
 for(const binding of report.bindings){
  if(binding.kind==='ButtonClick'){
   binding.name=binding.name.replace(/Click$/,'Entry');
   binding.kind='VMActivityEntry';
  }
  if(binding.kind==='VMActivityEntry'){
   const entry=entries.find(item=>item.name===binding.name[0].toLowerCase()+binding.name.slice(1));
   if(!entry)throw new Error('Missing navigation entry: '+binding.name);
   binding.entryPath=entry.path;
   binding.syncType='SyncMetadata';
   binding.clickTargets=entry.buttons.map(button=>button[0]);
  }
 }
 await writeFile(folder+'/report.json',JSON.stringify(report,null,2));
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
 for(const relative of ["UIActivityEntry.cs", "UIActivityBadge.cs", "VM/VMActivityEntry.cs", "VM/VMActivityBadge.cs", "Components/CustomTextOutline.cs", "UIProfileShortcuts.cs", "VM/VMProfileShortcuts.cs", "UIProfile.cs", "VM/VMProfile.cs"])for(const suffix of ['', '.meta']){const path='Assets/Scripts/Systems/Common/UI/'+relative+suffix;const target=join(folder,'source',path);await mkdir(dirname(target),{recursive:true});await writeFile(target,await readFile(join(binding.path,path)));count++;}
 console.log(JSON.stringify({files:count,source:'verified Unity project; maintained Widget code'}));
}finally{await w.close();}
