import test from 'node:test';
import assert from 'node:assert/strict';
import {verifyWidgetBindings} from '../shared/widget-binding-verification.mjs';
function fixture(){return {prefab:{nodes:[{id:'root'},{id:'button'}]},widgetBindings:{schemaVersion:'oathx-widget-bindings/1',mode:'static-compiled-contract',issues:[],widgets:[{
 nodeId:'root',uiType:'UILogin',vmType:'VMLogin',metadataVmType:'VMLogin',metadataTypeMatches:true,scriptMatches:true,
 declaredFields:[{name:'enterClick',syncType:'SyncClick',syncValid:true,componentType:'UnityEngine.UI.Button',propertyPresent:true}],
 fields:[{name:'enterClick',syncType:'SyncClick',targetCount:1,componentCount:1,targets:[{nodeId:'button',componentNodeId:'button',componentType:'UnityEngine.UI.Button',componentIndex:2,assignable:true}]}]
}]}};}
test('matching compiled VM and serialized references pass static integrity',()=>{const r=verifyWidgetBindings(fixture());assert.equal(r.passed,true);assert.equal(r.fields,1);});
test('old exporters cannot silently pass',()=>assert.equal(verifyWidgetBindings({}).status,'incomplete'));
for(const [name,mutate,code] of [
 ['VM mismatch',w=>w.metadataTypeMatches=false,'widget-vm-mismatch'],
 ['stale GUID',w=>w.scriptMatches=false,'vm-script-reference-invalid'],
 ['missing field',w=>w.fields=[],'binding-field-count'],
 ['wrong field order',w=>w.fields[0].name='closeClick','binding-field-order'],
 ['wrong Sync',w=>w.fields[0].syncType='SyncText','binding-sync-mismatch'],
 ['missing Sync type',w=>w.declaredFields[0].syncValid=false,'binding-sync-unresolved'],
 ['missing generated property',w=>w.declaredFields[0].propertyPresent=false,'binding-generated-property-missing'],
 ['null reference',w=>w.fields[0].targets[0].nodeId=null,'binding-reference-invalid'],
 ['component on different object',w=>w.fields[0].targets[0].componentNodeId='root','binding-reference-invalid'],
 ['wrong component type',w=>w.fields[0].targets[0].assignable=false,'binding-component-type'],
 ['empty targets',w=>w.fields[0].targets=[],'binding-target-count'],
 ['duplicate reference',w=>{w.fields[0].targets.push({...w.fields[0].targets[0]});w.fields[0].targetCount=2;w.fields[0].componentCount=2;},'binding-reference-duplicate'],
 ['duplicate field',w=>{w.fields.push(structuredClone(w.fields[0]));w.declaredFields.push({...w.declaredFields[0],name:'closeClick'});},'binding-field-duplicate']
])test(name,()=>{const d=fixture();mutate(d.widgetBindings.widgets[0]);const r=verifyWidgetBindings(d);assert.equal(r.passed,false);assert.ok(r.differences.some(x=>x.code===code));});
test('inherited VM evidence remains incomplete',()=>{const d=fixture();d.widgetBindings.issues.push({severity:'incomplete',code:'inherited-vm-order-unverified'});assert.equal(verifyWidgetBindings(d).status,'incomplete');});
