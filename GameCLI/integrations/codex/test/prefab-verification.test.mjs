import test from 'node:test';
import assert from 'node:assert/strict';
import {selectPrefab,comparePrefab} from '../shared/prefab-verification.mjs';
const node={id:'Dialog',name:'Dialog',parent:null,type:'Container',surface:'transparent',x:0,y:0,width:100,height:100,color:'#ffffff',text:'',fontSize:20};
function fixtures(){
 const design=selectPrefab({width:100,height:100,nodes:[structuredClone(node)]},'Dialog');
 const n=design.nodes[0];
 const document={schemaVersion:'ui-template/1',prefab:{nodes:[{id:'root',name:'Dialog',parentId:null,siblingIndex:0,kind:'Node',active:true,rect:Object.fromEntries(Object.entries(n.rectTransform).map(([k,v])=>[k,[v.x,v.y]])),localScale:[1,1,1],localRotation:[0,0,0,1],components:[]}]}};
 document.widgetBindings={schemaVersion:'oathx-widget-bindings/1',mode:'static-compiled-contract',widgets:[],issues:[]};
 return {design,document};
}
test('exact design contract passes independent of document IDs',()=>{const {design,document}=fixtures();assert.equal(comparePrefab(design,document).passed,true);});
test('layout drift fails with expected and actual evidence',()=>{const {design,document}=fixtures();document.prefab.nodes[0].rect.sizeDelta[0]+=1;const r=comparePrefab(design,document);assert.equal(r.status,'failed');assert.equal(r.differences[0].property,'rect.sizeDelta[0]');});
test('float noise within fixed tolerance is accepted',()=>{const {design,document}=fixtures();document.prefab.nodes[0].rect.sizeDelta[0]+=0.00001;assert.equal(comparePrefab(design,document).passed,true);});
test('extra objects are not ignored',()=>{const {design,document}=fixtures();document.prefab.nodes.push({...structuredClone(document.prefab.nodes[0]),id:'extra',parentId:'root',name:'Extra'});assert.equal(comparePrefab(design,document).status,'failed');});
test('unreadable scripts and animation never pass',()=>{const {design,document}=fixtures();design.animations=[{id:'open'}];const r=comparePrefab(design,document,[{code:'unsupported-component',type:'MissingScript'}]);assert.equal(r.status,'incomplete');assert.equal(r.passed,false);assert.equal(r.unverified.length,2);});
test('undeclared components block success',()=>{const {design,document}=fixtures();document.prefab.nodes[0].components.push({type:'UnityEngine.CanvasGroup',properties:{}});assert.equal(comparePrefab(design,document).passed,false);});
test('malformed hierarchy is an error rather than success',()=>{const {design,document}=fixtures();document.prefab.nodes[0].parentId='missing';assert.throws(()=>comparePrefab(design,document),/父级/);});
test('selection uses project root ID and excludes unrelated roots',()=>{assert.throws(()=>selectPrefab({nodes:[node]},'SvNotice'),/根节点/);const d=selectPrefab({width:100,height:100,nodes:[node,{...node,id:'Other',name:'Other'}]},'Dialog');assert.equal(d.nodes.length,1);});
test('binding failures participate in the overall prefab verdict',()=>{const {design,document}=fixtures();document.widgetBindings.issues.push({nodeId:'root',code:'widget-metadata-count',metadataCount:0,widgetCount:1});const r=comparePrefab(design,document);assert.equal(r.status,'failed');assert.equal(r.bindings.passed,false);assert.ok(r.differences.some(d=>d.source==='widget-bindings'));});
test('legacy exports remain incomplete even without visible custom components',()=>{const {design,document}=fixtures();delete document.widgetBindings;assert.equal(comparePrefab(design,document).status,'incomplete');});
