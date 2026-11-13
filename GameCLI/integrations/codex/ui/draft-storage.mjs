// Keep only small, tab-scoped references in sessionStorage. Large source archives live in IndexedDB.
export function indexedDraftBackend(indexedDB){
  let database;
  function open(){return database??=new Promise((resolve,reject)=>{
    const request=indexedDB.open('gamecli-studio-drafts',1);
    request.onupgradeneeded=()=>request.result.createObjectStore('drafts');
    request.onsuccess=()=>resolve(request.result);request.onerror=()=>{database=null;reject(request.error);};
    request.onblocked=()=>{database=null;reject(Error('草稿存储暂时不可用。'));};
  });}
  async function run(mode,operation){
    const db=await open();return new Promise((resolve,reject)=>{
      const transaction=db.transaction('drafts',mode),request=operation(transaction.objectStore('drafts'));
      transaction.oncomplete=()=>resolve(request.result);transaction.onerror=transaction.onabort=()=>reject(transaction.error??request.error??Error('草稿存储失败。'));
    });
  }
  return {get:id=>run('readonly',s=>s.get(id)),put:(id,value)=>run('readwrite',s=>s.put(value,id)),remove:id=>run('readwrite',s=>s.delete(id))};
}
export function createDraftStorage(storage,backend,newId=()=>crypto.randomUUID()){
  let pending=Promise.resolve();
  const enqueue=fn=>{const result=pending.then(fn);pending=result.catch(()=>{});return result;};
  const parse=key=>JSON.parse(storage.getItem(key)??'null');
  async function write(key,value){
    const old=parse(key),id=old?.indexedDraftId??newId();
    await backend.put(id,value);
    // Replace the legacy full payload only after the durable write succeeds.
    storage.setItem(key,JSON.stringify({indexedDraftId:id}));
  }
  return {
    read:key=>enqueue(async()=>{
      const value=parse(key);if(!value)return null;
      if(value.indexedDraftId)return await backend.get(value.indexedDraftId)??null;
      // Failure to migrate must never hide the old recoverable draft.
      try{await write(key,value);}catch{}
      return value;
    }),
    write:(key,value)=>{const copy=structuredClone(value);return enqueue(()=>write(key,copy));},
    remove:key=>enqueue(async()=>{const old=parse(key);if(old?.indexedDraftId)await backend.remove(old.indexedDraftId);storage.removeItem(key);}),
    archive:(key,version)=>enqueue(async()=>{
      const old=parse(key);if(!old)return;
      if(!old.indexedDraftId)await write(key,old);
      let destination=key+'.conflict.'+version;
      if(storage.getItem(destination)!==null)destination+='.'+newId();
      storage.setItem(destination,storage.getItem(key));storage.removeItem(key);
    }),
    conflictKeys:key=>{try{return Array.from({length:storage.length},(_,i)=>storage.key(i)).filter(k=>k?.startsWith(key+'.conflict.'));}catch{return [];}}
  };
}
