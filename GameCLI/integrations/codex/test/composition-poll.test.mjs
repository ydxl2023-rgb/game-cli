import test from 'node:test';
import assert from 'node:assert/strict';
import {setupCompositionBar} from '../ui/composition-bar.mjs';
test('late polling responses cannot roll the canvas back after a successful save',async()=>{
  const original={document:globalThis.document,window:globalThis.window,setInterval:globalThis.setInterval};
  let resolve,tick,version=14,loaded=[];
  try{
    globalThis.document={createElement:()=>({setAttribute(){}}),getElementById:()=>({after(){}})};
    globalThis.window={addEventListener(){}};globalThis.setInterval=fn=>{tick=fn;return 0;};
    setupCompositionBar({bridge:{projectId:'demo',call:()=>new Promise(r=>resolve=r)},getState:()=>({ready:true,version,dirty:false}),onSnapshot:s=>loaded.push(s.revision),notice(){}});
    version=15;resolve({revision:14});await new Promise(r=>setImmediate(r));assert.deepEqual(loaded,[]);
    tick();resolve({revision:16});await new Promise(r=>setImmediate(r));assert.deepEqual(loaded,[16]);
  }finally{Object.assign(globalThis,original);}
});
