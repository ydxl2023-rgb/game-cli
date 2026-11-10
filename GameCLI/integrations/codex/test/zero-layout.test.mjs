import test from 'node:test';
import assert from 'node:assert/strict';
import {validateDesign} from '../shared/design.mjs';
import {validateTemplate,instantiateTemplate} from '../shared/templates.mjs';
test('zero-sized Unity layout placeholders survive template validation and cloning',()=>{
 const rect=(x,y)=>({anchorMin:{x:0,y:1},anchorMax:{x:0,y:1},pivot:{x:0,y:1},anchoredPosition:{x:0,y:0},sizeDelta:{x,y}});
 const node=(id,parent,width,height)=>({id,parent,name:id,type:'Container',x:0,y:0,width,height,color:'#ffffff',text:'',fontSize:20,rectTransform:rect(width,height)});
 const d={schemaVersion:1,name:'Login',width:1920,height:1080,nodes:[node('Root',null,1920,1080),node('Account','Root',0,59),node('Content','Root',1200,0)]};
 const payload=validateTemplate({root:'Root',design:d},'system'),copy={schemaVersion:1,name:'Project',width:1920,height:1080,nodes:[]};
 instantiateTemplate(copy,{id:'Login',category:'system',payload},null,{x:0,y:0});
 const saved=validateDesign(JSON.parse(JSON.stringify(copy)));
 assert.equal(saved.nodes.find(n=>n.name==='Account').width,0);assert.equal(saved.nodes.find(n=>n.name==='Content').height,0);
 d.nodes[1].rectTransform.sizeDelta.x=-1;assert.throws(()=>validateDesign(d),/RectTransform/);
});
