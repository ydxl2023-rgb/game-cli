// Contracts use unique sibling names so template instance ID remapping is safe.
export function inspectRenderOrder(design) {
 const errors=[],warnings=[],nodes=design.nodes??[];
 for(const n of nodes){
  const siblings=nodes.filter(x=>x.parent===n.parent);
  if(n.role!=='backdrop'&&!(n.role==='background'&&n.mustRenderBehind!==undefined)){
   if(n.mustRenderBehind!==undefined)errors.push({code:'render-contract-role',node:n.id});
   if(!n.role&&/^(bg|mask|backdrop)$/i.test(n.name??'')&&n.color==='#000000'&&n.opacity>0&&n.opacity<1&&siblings.indexOf(n)>0)
    warnings.push({code:'unannotated-backdrop',node:n.id,message:'疑似半透明遮罩位于内容之后，请标注并审核绘制顺序。'});
   continue;
  }
  const targets=n.mustRenderBehind;
  if(!Array.isArray(targets)||!targets.length||targets.some(t=>typeof t!=='string'||!t)||new Set(targets).size!==targets.length){errors.push({code:'render-contract-targets',node:n.id});continue;}
  for(const name of targets){
   const found=siblings.filter(x=>x.name===name);
   if(found.length!==1||found[0]===n){errors.push({code:'render-target-not-unique-sibling',node:n.id,target:name});continue;}
   if(siblings.indexOf(n)>=siblings.indexOf(found[0]))errors.push({code:'backdrop-over-content',node:n.id,target:name,expected:'backdrop before content',actual:siblings.map(x=>x.name)});
  }
 }
 return {status:errors.length?'failed':warnings.length?'incomplete':'passed',passed:!errors.length&&!warnings.length,errors,warnings};
}

export function assertRenderOrder(design){
 const result=inspectRenderOrder(design);
 if(result.errors.length)throw Error('UI 绘制顺序检查失败：'+JSON.stringify(result.errors));
 return result;
}
