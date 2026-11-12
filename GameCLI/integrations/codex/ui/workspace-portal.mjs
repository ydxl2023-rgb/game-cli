import {openCodeTemplate} from './code-template.mjs';
import {workspaceNavigation} from './workspace-navigation.mjs';
import {mountCandidateDiff} from './candidate-diff.mjs';
import {mountCandidatePreview} from './candidate-preview.mjs';

const $=id=>document.getElementById(id),el=(tag,text)=>{const e=document.createElement(tag);if(text!==undefined)e.textContent=text;return e;};
async function api(path,body){const r=await fetch('/api/studio/'+path,body?{method:'POST',headers:{'content-type':'application/json'},body:JSON.stringify(body)}:{cache:'no-store'}),v=await r.json();if(!r.ok)throw Error(v.error);return v;}
function confirmCandidate(title,message,action){
 return new Promise(resolve=>{
  const dialog=el('dialog'),heading=el('h2',title),note=el('p',message),cancel=el('button','取消'),confirm=el('button',action);
  dialog.className='candidate-confirm';confirm.className='publish';cancel.onclick=()=>dialog.close();confirm.onclick=()=>{dialog.returnValue='confirmed';dialog.close();};
  dialog.append(heading,note,cancel,confirm);dialog.onclose=()=>{const accepted=dialog.returnValue==='confirmed';dialog.remove();resolve(accepted);};document.body.append(dialog);dialog.showModal();cancel.focus();
 });
}
async function act(fn){try{await fn();}catch(e){$('status').textContent=e.message;}}
const templatePage=location.pathname.startsWith('/studio/templates');
$('navigation').replaceWith(workspaceNavigation({template:templatePage}));
document.body.classList.add(location.pathname.endsWith('/review')?'review-page':templatePage?'templates-page':'projects-page');
if(location.pathname==='/studio/templates/review'){
 const back=el('button','← 返回模板编辑区');back.type='button';back.id='back-to-template-editor';
 back.onclick=()=>{location.href='/studio/templates';};
 document.querySelector('.portal-context').prepend(back);
}
const info=await api('projects').catch(e=>{$('status').textContent=e.message;return null;});
if(info){$('identity').textContent='本机身份：'+info.actor+' · 项目数据独立保存，已发布模板只读';
if(location.pathname==='/studio/templates/review'){
 $('title').textContent='标准模板管理';
 if(!info.templateAdmin){$('status').textContent='当前身份不是模板管理员。';}else{
 const result=await api('admin/candidates?summary=1');
 const pending=result.candidates.filter(c=>c.status==='pending');
 const heading=el('h2','候选模板 · 待审核 '+pending.length+' 项');$('content').append(heading);
 $('content').append(el('p','直接预演候选界面，核对后发布或拒绝。发布只更新标准库，已有项目副本保持不变。'));
 if(!pending.length)$('content').append(el('p','暂无待审核候选。请在模板页打开模板草稿，保存后提交候选。'));
 for(const c of [...result.candidates].sort((a,b)=>(b.status==='pending')-(a.status==='pending'))){
  const card=el('article');card.className='candidate-card';card.setAttribute('aria-label',c.template_name+' 候选');
  const title=el('h2',c.template_name),badge=el('span',({pending:'待审核',published:'已发布',rejected:'已拒绝'})[c.status]??c.status);badge.className='candidate-badge';title.append(badge);
  card.append(title,el('p','来源：'+(c.project_id?'历史项目候选 '+c.project_id:'独立模板草稿 / 源码候选')+' · 提交人：'+c.actor+' · 基于 v'+c.base_version+' / 当前 v'+c.current_version+' · '+new Date(c.created_at).toLocaleString()));
  const previewHost=el('div');previewHost.className='lazy-preview';card.append(previewHost);let dispose=()=>{};
  const previewObserver=new IntersectionObserver(entries=>{if(entries[0].isIntersecting){if(previewHost.childElementCount)return;try{dispose=mountCandidatePreview(previewHost,c.payload);}catch(e){previewHost.append(el('p','预演加载失败：'+e.message));}}else{dispose();dispose=()=>{};previewHost.replaceChildren();}},{rootMargin:'150px'});previewObserver.observe(previewHost);
  const actions=el('div');actions.className='candidate-actions';
  const publish=el('button','发布为 v'+(c.current_version+1)),reject=el('button','拒绝');publish.className='publish';reject.className='reject';
  const stale=c.base_version!==c.current_version;publish.disabled=stale||c.status!=='pending';reject.disabled=c.status!=='pending';
  if(stale&&c.status==='pending')card.append(el('p','此候选基于旧版本，不能发布。请基于最新标准模板重新提交。'));
  const finish=(text)=>{badge.textContent=text;publish.disabled=reject.disabled=true;pending.splice(pending.indexOf(c),1);heading.textContent='候选模板 · 待审核 '+pending.length+' 项';};
  publish.onclick=()=>act(async()=>{if(!await confirmCandidate('发布 '+c.template_name,'确认将此候选发布为 v'+(c.current_version+1)+'？这会更新标准模板库，现有项目副本不会自动更新。','确认发布'))return;publish.disabled=reject.disabled=true;try{const r=await api('admin/publish',{candidateId:c.id,confirm:true});finish('已发布 v'+r.version);$('status').textContent='已发布 '+r.name+' v'+r.version;}catch(e){publish.disabled=stale;reject.disabled=false;throw e;}});
  reject.onclick=()=>act(async()=>{if(!await confirmCandidate('拒绝 '+c.template_name,'此候选将标记为已拒绝，不会更新标准模板。','确认拒绝'))return;publish.disabled=reject.disabled=true;try{await api('admin/reject',{candidateId:c.id});finish('已拒绝');dispose();$('status').textContent='已拒绝 '+c.template_name+' 候选。';}catch(e){publish.disabled=stale;reject.disabled=false;throw e;}});
  actions.append(publish,reject);card.append(actions);
  const details=el('details'),summary=el('summary','查看完整变更（布局与源码）'),diff=el('div');diff.className='diff';details.append(summary,diff);
  let loaded=false;details.addEventListener('toggle',()=>{if(details.open&&!loaded){loaded=true;diff.textContent='正在加载布局与源码…';act(async()=>{try{const data=await api('admin/candidate-detail?id='+encodeURIComponent(c.id));diff.replaceChildren();mountCandidateDiff(diff,data);const download=el('button','下载完整布局与源码差异');download.onclick=()=>{const url=URL.createObjectURL(new Blob([JSON.stringify(data,null,2)],{type:'application/json'})),link=el('a');link.href=url;link.download='candidate-'+c.id+'.json';link.click();setTimeout(()=>URL.revokeObjectURL(url),1000);};diff.append(download);}catch(e){loaded=false;diff.textContent='加载失败，请收起后重试。';throw e;}});}});card.append(details);$('content').append(card);
 }
 const library=await api('templates'),list=el('details');list.append(el('summary','已发布标准模板与版本历史'));
 for(const t of library.templates){const row=el('p'),button=el('button',t.name+' · v'+t.version+' · 查看历史');button.onclick=()=>act(async()=>{const v=await api('template-versions?id='+encodeURIComponent(t.id));row.append(el('pre',v.versions.map(x=>'v'+x.version+' · '+x.created_at).join('\n')));button.disabled=true;});row.append(button);if(t.payload.kind==='code'){const source=el('button','纯代码 · 查看源码');source.onclick=()=>openCodeTemplate(t);row.append(source);}list.append(row);}$('content').append(list);
 }
}else if(location.pathname==='/studio/templates/library'){
 $('title').textContent='模板';
 $('identity').textContent='标准模板库 · 已发布版本只读 · 草稿审核通过后发布';
 if(info.templateAdmin){const review=el('a','候选审核');review.href='/studio/templates/review';$('content').append(review);}
 const library=await api('templates');
 for(const category of library.categories){const section=el('section');section.className='template-section';section.append(el('h2',category.name));
 for(const t of library.templates.filter(t=>t.category===category.id)){const card=el('article');card.className='template-entry';card.append(el('h3',t.name),el('p',(t.description??'')+' · 已发布 v'+t.version));const view=el('a','查看已发布版本');view.href='/studio/templates/'+encodeURIComponent(t.id);card.append(view);
 if(info.templateAdmin){const edit=el('button','编辑独立草稿');edit.onclick=()=>act(async()=>{edit.disabled=true;try{const d=await api('template-drafts',{templateId:t.id});location.href='/studio/template-drafts/'+d.id;}finally{edit.disabled=false;}});card.append(edit);}
 const versions=el('button','版本历史');versions.onclick=()=>act(async()=>{const v=await api('template-versions?id='+encodeURIComponent(t.id));card.append(el('pre',v.versions.map(x=>'v'+x.version+' · '+x.created_at).join('\n')));versions.disabled=true;});card.append(versions);section.append(card);
 }$('content').append(section);}
}else{
 $('title').textContent='项目';
 const form=el('form'),input=el('input'),create=el('button','创建独立项目');input.placeholder='项目名称';input.required=true;input.maxLength=80;form.append(input,create);form.onsubmit=e=>{e.preventDefault();act(async()=>{create.disabled=true;try{const project=await api('projects',{name:input.value});location.href='/studio/projects/'+encodeURIComponent(project.id);}finally{create.disabled=false;}});};$('content').append(form);
 for(const p of info.projects){const card=el('article'),link=el('a','打开项目工作区');card.className='project-entry';link.href='/studio/projects/'+encodeURIComponent(p.id);card.append(el('h2',p.name),el('p','项目 ID：'+p.id+' · 权限：'+p.role+' · 版本：v'+p.version),link);$('content').append(card);}
}}
