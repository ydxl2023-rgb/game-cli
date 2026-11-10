import test from 'node:test';
import assert from 'node:assert/strict';
import {validateScripts,saveTemplateScripts} from '../server/template-scripts.mjs';

const bundle=()=>({schemaVersion:1,files:[{path:'Runtime/XLauncher.cs',content:'\ufeff// Original source\r\nclass XLauncher {}\r\n'}]});
test('source packages preserve original text and verify supplied hashes',()=>{
  const input=bundle(),result=validateScripts(input);
  assert.equal(result.files[0].content,input.files[0].content);
  assert.match(result.files[0].sha256,/^[a-f0-9]{64}$/);
  assert.deepEqual(validateScripts(JSON.parse(JSON.stringify(result))),result);
  result.files[0].content+=' ';
  assert.throws(()=>validateScripts(result),/SHA256/);
  assert.equal(validateScripts(null),null);
});
test('reject unsafe paths, ambiguous names and invalid packages',()=>{
  for(const path of ['../x.cs','/x.cs','C:/x.cs','Runtime\\x.cs','./x.cs','Runtime//x.cs']) {
    const input=bundle();input.files[0].path=path;assert.throws(()=>validateScripts(input),/路径/);
  }
  const duplicate=bundle();duplicate.files.push({...duplicate.files[0],path:'runtime/xlauncher.cs'});
  assert.throws(()=>validateScripts(duplicate),/重复/);
  assert.throws(()=>validateScripts({schemaVersion:1,files:[]}),/文件/);
});
test('save uses system category and optimistic version check without changing layout',async()=>{
  const pool={async query(sql,args){
    assert.match(sql,/category_id='system'/);assert.match(sql,/version=\$3/);
    assert.doesNotMatch(sql,/SET payload/);assert.equal(args[1],'SvStart');assert.equal(args[2],4);
    return {rows:[{id:'SvStart',version:5,scripts:JSON.parse(args[0])}]};
  }};
  const result=await saveTemplateScripts(pool,{templateId:'SvStart',version:4,scripts:bundle()});
  assert.equal(result.version,5);assert.equal(result.scripts.files[0].content,bundle().files[0].content);
  await assert.rejects(()=>saveTemplateScripts({query:async()=>({rows:[]})},{templateId:'SvStart',version:4,scripts:bundle()}),/版本/);
});
