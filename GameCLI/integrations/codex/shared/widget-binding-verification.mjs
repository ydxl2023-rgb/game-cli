// Static integrity is separate from the product's intended target mapping and runtime behavior.
export function verifyWidgetBindings(document){
 const evidence=document.widgetBindings,differences=[],unverified=[];
 const fail=(node,field,code,expected,actual)=>differences.push({node,field,code,expected,actual:actual??null});
 if(evidence?.schemaVersion!=='oathx-widget-bindings/1'||evidence.mode!=='static-compiled-contract'||!Array.isArray(evidence.widgets)||!Array.isArray(evidence.issues))
  return {status:'incomplete',passed:false,widgets:0,fields:0,differences,unverified:[{code:'widget-binding-evidence-unavailable'}]};
 const nodeIds=new Set(document.prefab.nodes.map(n=>n.id)),owners=new Set();let count=0;
 for(const issue of evidence.issues){
  if(issue.severity==='incomplete')unverified.push(issue);
  else fail(issue.nodeId,null,issue.code,'valid binding structure',issue);
 }
 for(const widget of evidence.widgets){
  const node=widget.nodeId;
  if(!nodeIds.has(node)||owners.has(node))fail(node,null,'widget-owner-invalid','unique existing node',node);
  owners.add(node);
  if(!widget.uiType||!widget.vmType||widget.metadataTypeMatches!==true)fail(node,null,'widget-vm-mismatch',widget.vmType,widget.metadataVmType);
  if(widget.scriptMatches!==true)fail(node,null,'vm-script-reference-invalid',widget.vmType,'missing or different MonoScript');
  if(!Array.isArray(widget.declaredFields)||!Array.isArray(widget.fields)){unverified.push({node,code:'widget-fields-unavailable'});continue;}
  const expected=widget.declaredFields,actual=widget.fields;
  if(expected.length!==actual.length)fail(node,null,'binding-field-count',expected.length,actual.length);
  const names=new Set();
  for(let index=0;index<Math.max(expected.length,actual.length);index++){
   const e=expected[index],a=actual[index];count+=!!e;
   if(!e||!a){fail(node,e?.name??a?.name,'binding-field-missing-or-extra',e?.name,a?.name);continue;}
   if(names.has(a.name))fail(node,a.name,'binding-field-duplicate','unique',a.name);
   names.add(a.name);
   if(e.name!==a.name)fail(node,e.name,'binding-field-order',e.name,a.name);
   if(e.syncValid!==true||!e.componentType)fail(node,e.name,'binding-sync-unresolved','known ISyncAction and target type',e.syncType);
   if(e.propertyPresent!==true)fail(node,e.name,'binding-generated-property-missing',true,e.propertyPresent);
   if(e.syncType!==a.syncType)fail(node,e.name,'binding-sync-mismatch',e.syncType,a.syncType);
   if(!Array.isArray(a.targets)||a.targets.length===0||a.targetCount!==a.componentCount||a.targets.length!==a.targetCount){fail(node,e.name,'binding-target-count','equal nonzero targets and components',a);continue;}
   const refs=new Set();
   for(const target of a.targets){
    if(!target?.nodeId||!nodeIds.has(target.nodeId)||target.nodeId!==target.componentNodeId||!target.componentType||!Number.isInteger(target.componentIndex)||target.componentIndex<0)
     fail(node,e.name,'binding-reference-invalid','live component on its target inside exported hierarchy',target);
    if(target?.assignable!==true)fail(node,e.name,'binding-component-type',e.componentType,target?.componentType);
    const key=JSON.stringify([target?.componentNodeId,target?.componentIndex]);
    if(refs.has(key))fail(node,e.name,'binding-reference-duplicate','unique component per field',target);
    refs.add(key);
   }
  }
 }
 return {policy:'oathx-widget-bindings/1',status:unverified.length?'incomplete':differences.length?'failed':'passed',passed:!unverified.length&&!differences.length,widgets:evidence.widgets.length,fields:count,differences,unverified,
  limitations:['Static compiled-contract integrity only; intended product target mapping and runtime updates/callbacks are not verified.','Generated field order follows compiled declaration order; inherited VM contracts are incomplete.']};
}
