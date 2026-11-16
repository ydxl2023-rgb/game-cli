import test from 'node:test';
import assert from 'node:assert/strict';
import {validateConfigurations} from '../shared/configurations.mjs';
import {instantiateTemplate} from '../shared/templates.mjs';
import {validateDesign} from '../shared/design.mjs';
import {checkConfigurationAssets} from '../server/configurations.mjs';
const config=()=>({schemaVersion:1,tables:[{name:'propCommonText',purpose:'required',primaryKey:'NumID',aliasField:'ID',fields:[{name:'ID',type:'int'},{name:'NumID',type:'int'},{name:'Context',type:'string'}],entries:[{ID:'LOGIN',NumID:1,Context:'Hello'}],source:{assetId:'a'.repeat(64),sha256:'a'.repeat(64),fileName:'propCommonText.xlsx',sheet:'CommonText'}}],dependencies:[],integration:[]});
test('configuration packages validate references and reject ambiguous data',()=>{
 assert.deepEqual(validateConfigurations(config()),config());
 const duplicate=config();duplicate.tables[0].entries.push({...duplicate.tables[0].entries[0]});assert.throws(()=>validateConfigurations(duplicate));
 const unsafe=config();unsafe.tables[0].source.fileName='../a.xlsx';assert.throws(()=>validateConfigurations(unsafe));
 const mismatch=config();mismatch.tables[0].source.sha256='b'.repeat(64);assert.throws(()=>validateConfigurations(mismatch));
});
test('code template instances receive isolated configuration copies and survive project save',()=>{
 const template={id:'MasterSystem',category:'system',version:7,payload:{kind:'code',root:null,design:{schemaVersion:1,name:'Master',width:1920,height:1080,nodes:[]},configurations:config()}};
 const d={schemaVersion:1,name:'Project',width:1920,height:1080,nodes:[]};const id=instantiateTemplate(d,template,null,{x:0,y:0},{allowCode:true});
 const saved=validateDesign(d);assert.deepEqual(saved.nodes.find(n=>n.id===id).configurations,config());saved.nodes[0].configurations.tables[0].entries[0].Context='Product';assert.equal(template.payload.configurations.tables[0].entries[0].Context,'Hello');
});
test('publishing refuses missing or corrupt workbook bytes',async()=>{
 await assert.rejects(checkConfigurationAssets({query:async()=>({rows:[]})},config()),/缺失/);
 await assert.rejects(checkConfigurationAssets({query:async()=>({rows:[{body:Buffer.from('invalid')}]})},config()),/SHA256/);
});
