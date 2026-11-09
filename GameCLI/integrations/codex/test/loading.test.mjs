import test from 'node:test';
import assert from 'node:assert/strict';
import {validateDesign,designHtml} from '../shared/design.mjs';
test('standard progress and image references survive validation and export',()=>{
 const base={x:0,y:0,width:100,height:20,color:'#ff5e00',text:'',fontSize:24};
 const d={schemaVersion:1,name:'Test',width:1920,height:1080,nodes:[
 {...base,id:'Progress',name:'Progress',parent:null,type:'Progress',progress:.59},
 {...base,id:'Fill',name:'Fill',parent:'Progress',type:'Image'},
 {...base,id:'Percentage',name:'Percentage',parent:'Progress',type:'Text'},
 {...base,id:'Background',name:'Background',parent:null,type:'Image',image:'a'.repeat(64),imageFit:'cover'}]};
 assert.equal(validateDesign(d).nodes[0].progress,.59);
 assert.match(designHtml(d),/width:59px/);assert.match(designHtml(d),/Loading\.\.\. 59%/);
 assert.match(designHtml(d),/\/api\/studio\/assets\/a{64}/);
 d.nodes[0].progress=0;assert.match(designHtml(d),/width:0px/);
 d.nodes[0].progress=2;assert.throws(()=>validateDesign(d),/进度/);
 d.nodes[0].progress=1;d.nodes[3].image='javascript:bad';assert.throws(()=>validateDesign(d),/资源/);
});
