export async function projectSnapshot(store,database){
  const local=await store.snapshot();
  if(!database.project)return local;
  try{
    const project=await database.project();
    return {...local,...(local.revision===0&&project.design?{design:project.design}:{}),project:{id:project.id,name:project.name,version:project.version}};
  }catch(e){return {...local,projectError:e.message};}
}
export async function saveProject(store,database,{design,revision,projectVersion}){
  const snapshot=await store.save(design,revision);
  try{return {...snapshot,projectSave:{ok:true,...await database.saveProject(snapshot,projectVersion)}};}
  catch(e){return {...snapshot,projectSave:{ok:false,error:e.message}};}
}
