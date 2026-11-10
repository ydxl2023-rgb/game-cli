import hljs from 'highlight.js/lib/core';
import csharp from 'highlight.js/lib/languages/csharp';
import yaml from 'highlight.js/lib/languages/yaml';
import json from 'highlight.js/lib/languages/json';
import javascript from 'highlight.js/lib/languages/javascript';
for(const [name,language]of Object.entries({csharp,yaml,json,javascript}))hljs.registerLanguage(name,language);
const languages={cs:'csharp',meta:'yaml',yaml:'yaml',yml:'yaml',json:'json',asmdef:'json',js:'javascript',mjs:'javascript'};
const labels={csharp:'C#',yaml:'YAML',json:'JSON',javascript:'JavaScript',plaintext:'纯文本'};
export function highlightSource(path,content){
 const language=languages[path.split('.').at(-1).toLowerCase()]??'plaintext';
 const html=language==='plaintext'?content.replaceAll('&','&amp;').replaceAll('<','&lt;').replaceAll('>','&gt;'):hljs.highlight(content,{language,ignoreIllegals:true}).value;
 return {language,label:labels[language],html,lines:content.split(/\r\n|\r|\n/).length};
}
export function createCodePage(onShow){
 const tab=document.createElement('button');tab.id='tab-code';tab.type='button';tab.setAttribute('role','tab');tab.setAttribute('aria-controls','code-view');tab.setAttribute('aria-selected','false');tab.textContent='代码';tab.onclick=onShow;
 document.getElementById('tab-relations').after(tab);
 const panel=document.createElement('section');panel.id='code-view';panel.hidden=true;panel.setAttribute('role','tabpanel');panel.setAttribute('aria-labelledby','tab-code');
 const empty=document.createElement('p');empty.className='code-page-empty';empty.textContent='在关系页选择卡片，再点击右侧代码文件进行预览。';panel.append(empty);
 document.getElementById('viewport').after(panel);
 return {open(file,templateName){previewCode(file,templateName,panel);onShow();}};
}
function previewCode(file,templateName,panel){
 const source=String(file.content??''),highlighted=highlightSource(file.path,source);
 const dialog=document.createElement('div');dialog.className='code-preview';dialog.setAttribute('aria-labelledby','code-preview-title');
 const header=document.createElement('header'),title=document.createElement('h2');title.id='code-preview-title';title.textContent=file.path.split('/').at(-1);
 header.append(title);
 const info=document.createElement('p');info.className='code-preview-info';info.textContent=`${templateName} · ${highlighted.label} · 只读 · ${highlighted.lines} 行`;
 const path=document.createElement('p');path.className='code-preview-path';path.textContent=file.path;
 const view=document.createElement('div');view.className='code-preview-scroll';view.tabIndex=0;view.setAttribute('aria-label','源码内容');
 const gutter=document.createElement('pre');gutter.className='code-line-numbers';gutter.setAttribute('aria-hidden','true');gutter.textContent=Array.from({length:highlighted.lines},(_,i)=>i+1).join('\n');
 const pre=document.createElement('pre'),code=document.createElement('code');code.className='hljs language-'+highlighted.language;code.innerHTML=highlighted.html;pre.append(code);view.append(gutter,pre);
 dialog.append(header,info,path,view);panel.replaceChildren(dialog);
 dialog.addEventListener('keydown',e=>e.stopPropagation());
}
