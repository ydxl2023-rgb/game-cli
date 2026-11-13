export function createDesignChat({bridge,getState,onSnapshot,notice}){
 const el=(tag,text)=>{const e=document.createElement(tag);if(text)e.textContent=text;return e;};
 const panel=el('section');panel.id='library-panel-design';panel.className='design-chat';panel.setAttribute('role','tabpanel');panel.setAttribute('aria-labelledby','library-tab-design');
 const heading=el('div');heading.className='design-chat-heading';const context=el('span','系统设计 · 正在读取项目'),status=el('span');status.setAttribute('role','status');heading.append(context,status);
 const messages=el('div');messages.className='design-messages';messages.setAttribute('role','log');messages.setAttribute('aria-label','系统设计对话');
 const form=el('form'),input=el('textarea');form.className='design-composer';input.placeholder='描述需要的系统和跳转，例如：增加登录系统，登录成功进入大厅';input.setAttribute('aria-label','系统设计需求');input.rows=2;input.maxLength=12000;
 const attachments=el('div'),actions=el('div');actions.className='design-chat-actions';attachments.className='design-attachments';
 const attach=el('button','＋ 参考文件'),file=el('input'),hint=el('span','Enter 发送 · Shift+Enter 换行'),stop=el('button','停止'),send=el('button','发送');attach.type='button';stop.type='button';send.type='submit';file.type='file';file.multiple=true;file.accept='.txt,.md,.json,.csv,.cs';file.hidden=true;attach.onclick=()=>file.click();actions.append(attach,hint,stop,send);form.append(attachments,input,actions,file);panel.append(heading,messages,form);
 let files=[],turns=[],pending=false,reading=false,last='',timer,currentProject='';
 async function api(command,args={}){
  if(bridge.embedded)return bridge.call(command==='list'?'design_chat':'design_message',{projectId:bridge.projectId,...args,...(command==='list'?{}:{command})});
  const r=await fetch('/api/studio/projects/'+encodeURIComponent(bridge.projectId)+'/design-chat',command==='list'?{cache:'no-store'}:{method:'POST',headers:{'content-type':'application/json'},body:JSON.stringify({projectId:bridge.projectId,command,...args})});const d=await r.json();if(!r.ok)throw Error(d.error||'设计服务不可用。');return d;
 }
 function fileList(){attachments.replaceChildren();for(const [i,f]of files.entries()){const b=el('button',f.name+' ×');b.type='button';b.onclick=()=>{files.splice(i,1);fileList();};attachments.append(b);}}
 file.onchange=async()=>{try{const next=[...files];for(const f of file.files){if(f.size>300000||! /\.(txt|md|json|csv|cs)$/i.test(f.name))throw Error('支持小于 300 KB 的文本、Markdown、JSON、CSV 和 C# 参考文件。');next.push({name:f.name,content:await f.text()});}if(next.length>4||next.reduce((s,f)=>s+f.content.length,0)>100000)throw Error('最多 4 个参考文件，内容总计不超过 100,000 字符。');files=next;fileList();}catch(e){notice(e.message,true);}finally{file.value='';}};
 function controls(){const active=turns.some(t=>t.status==='thinking'),s=getState();send.disabled=pending;stop.disabled=pending||!active;input.disabled=pending||s.role==='viewer';context.textContent=`${bridge.projectId||'未选择项目'} · 项目 v${s.version??'—'}${s.dirty?' · 有未保存修改':''}`;status.textContent=active?'正在思考…':'方案讨论';}
 function render(){
  controls();const fingerprint=JSON.stringify([turns,getState().version,getState().dirty]);if(fingerprint===last)return;last=fingerprint;
  const nearBottom=messages.scrollHeight-messages.scrollTop-messages.clientHeight<70;messages.replaceChildren();
  if(!turns.length){messages.append(el('p','告诉我你需要哪些系统，以及它们如何跳转。方案应用前不会修改项目或标准模板。'));return;}
  for(const t of turns){const user=el('article');user.className='design-message user';user.append(el('small','你'),el('div',t.prompt));if(t.attachments?.length)user.append(el('small','参考：'+t.attachments.map(f=>f.name).join('、')));messages.append(user);
   const reply=el('article');reply.className='design-message assistant';reply.append(el('small','设计助手 · 基于 v'+t.project_version));reply.append(el('div',t.reply||({thinking:'正在分析系统和模板…',failed:t.error||'生成失败，请重新发送。',stopped:'已停止生成。'}[t.status]??'')));
   if(t.operations?.length){const details=el('details'),summary=el('summary',`方案变更 · ${t.operations.length} 项`);details.append(summary);const list=el('ul');for(const o of t.operations){list.append(el('li',o.type==='add'?`新增 ${o.templateId} v${o.version}`:o.type==='connect'?`连接 ${o.sourceId} → ${o.targetId} (${o.action})`:o.type==='remove'?`移除系统 ${o.id}`:'自动排列系统'));}details.append(list);reply.append(details);
    const apply=el('button',t.status==='applied'?`已应用到 v${t.applied_version}`:t.project_version!==getState().version?'方案已过期，请重新讨论':'应用到关系图');apply.disabled=t.status!=='complete'||t.project_version!==getState().version||getState().dirty||getState().role==='viewer';
    apply.onclick=async()=>{apply.disabled=true;try{if(getState().dirty)throw Error('请先保存项目，再重新讨论方案。');const result=await api('apply',{turnId:t.id,projectVersion:getState().version});if(!getState().dirty)onSnapshot(result);else notice('方案已应用到数据库；当前新修改的草稿已保留，请核对后重新加载。',true);await refresh();}catch(e){notice(e.message,true);last='';render();}};reply.append(apply);
   }messages.append(reply);
  }if(nearBottom)messages.scrollTop=messages.scrollHeight;
 }
 async function refresh(){if(reading||!bridge.projectId||!getState().ready)return;reading=true;try{if(currentProject!==bridge.projectId){currentProject=bridge.projectId;turns=[];last='';input.value=sessionStorage.getItem('gamecli.design.input.'+currentProject)||'';files=[];fileList();}turns=(await api('list')).turns;render();}catch(e){status.textContent='连接失败';status.title=e.message;send.disabled=pending;}finally{reading=false;}}
 input.oninput=()=>{try{sessionStorage.setItem('gamecli.design.input.'+bridge.projectId,input.value);}catch{}};
 input.onkeydown=e=>{if(e.key==='Enter'&&!e.shiftKey&&!e.isComposing){e.preventDefault();if(!send.disabled)form.requestSubmit();}};
 function sendError(message){
  const dialog=el('dialog');dialog.className='sync-confirm';dialog.setAttribute('aria-labelledby','design-send-error-title');
  const title=el('h2','暂时无法发送');title.id='design-send-error-title';const detail=el('p',message),close=el('button','知道了');close.type='button';close.onclick=()=>dialog.close();
  dialog.append(title,detail,close);dialog.onkeydown=e=>e.stopPropagation();dialog.onclose=()=>{dialog.remove();if(!input.disabled)input.focus();};document.body.append(dialog);dialog.showModal();close.focus();
 }
 form.onsubmit=async e=>{e.preventDefault();if(pending)return;
  const s=getState();
  const reason=!bridge.projectId?'请先选择项目，再发送需求。':!s.ready?'项目尚未加载完成，请稍后再试。':s.role==='viewer'?'当前项目为只读权限，请联系项目管理员获取编辑权限。':s.dirty?'请先保存项目，再发送需求，确保方案基于最新版本。':turns.some(t=>t.status==='thinking')?'当前项目正在生成方案，请等待完成或点击停止后再发送。':!input.value.trim()?'请输入需要讨论的系统设计需求。':input.value.length>12000?'需求不能超过 12,000 个字符，请缩短后重试。':null;
  if(reason){sendError(reason);return;}
  pending=true;controls();try{await api('send',{text:input.value,attachments:files,projectVersion:getState().version});input.value='';input.oninput();files=[];fileList();await refresh();}catch(e){sendError(e.message||'设计服务连接失败，请稍后重试。');}finally{pending=false;controls();}};
 stop.onclick=async()=>{const t=turns.find(t=>t.status==='thinking');if(!t)return;pending=true;controls();try{await api('stop',{turnId:t.id});await refresh();}catch(e){notice(e.message,true);}finally{pending=false;controls();}};
 // Keep keyboard shortcuts from deleting/moving UI nodes while writing a message.
 panel.addEventListener('keydown',e=>e.stopPropagation());
 timer=setInterval(()=>{if(!document.hidden)refresh();},2000);window.addEventListener('pagehide',()=>clearInterval(timer),{once:true});refresh();return panel;
}
