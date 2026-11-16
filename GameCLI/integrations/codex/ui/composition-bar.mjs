export function setupCompositionBar({bridge,getState,onSnapshot,notice}) {
 const bar=document.createElement('div');bar.className='composition-bar';bar.setAttribute('role','status');bar.textContent='任务开发状态：未启动';document.getElementById('resolution').after(bar);
 let busy=false,warned=null;
 async function refresh(){
  if(busy||!bridge.projectId||!getState().ready)return;busy=true;
  try{
   const s=await bridge.call('snapshot');
   if(s.revision>getState().version){if(getState().dirty){if(warned!==s.revision){notice('项目有远端更新，当前未保存草稿已保留。',true);warned=s.revision;}}else onSnapshot(s);}
   bar.textContent=s.integration?.length?'系统集成状态：'+s.integration.map(i=>i.templateId+' · '+i.label).join('；'):'任务开发状态：未启动 · 尚无集成检查记录';
  }catch(e){bar.textContent='任务开发状态：服务连接异常';bar.title=e.message;}finally{busy=false;}
 }
 const timer=setInterval(()=>{if(!document.hidden)refresh();},5000);window.addEventListener('pagehide',()=>clearInterval(timer),{once:true});refresh();
}
