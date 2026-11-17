import test from 'node:test';
import assert from 'node:assert/strict';
import {validateDesign,layerStyle} from '../shared/design.mjs';
import {createWorkspaces,referencesMaterialAsset} from '../server/workspaces.mjs';

function design(){return {schemaVersion:1,name:'TypographyTest',width:1920,height:1080,nodes:[{id:'Label',parent:null,type:'Text',name:'Label',x:0,y:0,width:120,height:40,color:'#ffffff',text:'Label',fontSize:28,fontAsset:'a'.repeat(64),textOutline:{width:2,color:'#0c0c1dff'}}]};}
test('source font and outline survive design normalization',()=>{
  const input=design(),output=validateDesign(input);
  assert.equal(output.nodes[0].fontAsset,input.nodes[0].fontAsset);
  assert.deepEqual(output.nodes[0].textOutline,input.nodes[0].textOutline);
  assert.match(layerStyle(output.nodes[0]),/paint-order:stroke fill/);
});
test('font and outline declarations reject CSS injection and invalid widths',()=>{
  for(const mutate of [n=>n.fontAsset='url(bad)',n=>n.textOutline.color='red;display:none',n=>n.textOutline.width=-1,n=>n.textOutline.width=NaN,n=>n.type='Image']){
    const input=design();mutate(input.nodes[0]);assert.throws(()=>validateDesign(input));
  }
});

test('published font references authorize asset delivery alongside images',async()=>{
 const font='a'.repeat(64),calls=[];
 const workspace=createWorkspaces({connect:async()=>({pool:{query:async(sql,args)=>{
  calls.push({sql,args});
  return {rows:sql.includes('template_versions')&&args.some(arg=>arg===JSON.stringify([{fontAsset:font}]))?[{exists:1}]:[]};
 }},close:async()=>{}})});
 try{
  assert.equal(await workspace.assetAllowed(font),true);
  assert.equal(await workspace.assetAllowed('b'.repeat(64)),false);
  assert.match(calls[0].sql,/OR payload/);
 }finally{await workspace.close();}
});

test('native production assets require an exact manifest reference and hash',()=>{
 const hash='c'.repeat(64),scripts={files:[{path:'GameCLI/Templates/Home/materials.json',content:JSON.stringify({files:[{assetId:hash,sha256:hash}]})}]};
 assert.equal(referencesMaterialAsset(scripts,hash),true);
 assert.equal(referencesMaterialAsset(scripts,'d'.repeat(64)),false);
 scripts.files[0].path='readme.md';assert.equal(referencesMaterialAsset(scripts,hash),false);
 scripts.files[0].path='GameCLI/Templates/Home/materials.json';scripts.files[0].content=hash;
 assert.equal(referencesMaterialAsset(scripts,hash),false);
});
