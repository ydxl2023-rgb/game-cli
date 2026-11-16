export const runtimeChecks=['Unity 编译与 Console','组件及 Metadata 引用','全部入口与独立子页面','关闭与返回流程','动画与布局','数据回填与持久化'];
export function integrationGate(report,acceptance){
 const failures=report.items.filter(i=>i.status==='missing').length+report.placeholders.length+(report.renderOrder??[]).reduce((sum,r)=>sum+(r.errors?.length??0),0);
 const reviews=[...report.items.filter(i=>i.status==='adapted-review-required'),...report.references].map(i=>i.path);
 const valid=acceptance?.fingerprint===report.fingerprint&&typeof acceptance.reviewer==='string'&&acceptance.reviewer.trim();
 const reviewed=valid&&reviews.every(path=>acceptance.reviews?.some(r=>r.path===path&&r.approved===true&&r.reason?.trim()));
 if(failures||!report.items.length||reviews.length&&!reviewed)return {stage:'repair',label:'返回修复',passed:false};
 if(valid&&acceptance.checks?.some(c=>c.status==='failed'))return {stage:'repair',label:'返回修复',passed:false};
 const complete=valid&&runtimeChecks.every(name=>acceptance.checks?.some(c=>c.name===name&&c.status==='passed'&&c.evidence?.trim()));
 return complete?{stage:'complete',label:'已完成',passed:true}:{stage:'awaiting-acceptance',label:'待验收',passed:false};
}
