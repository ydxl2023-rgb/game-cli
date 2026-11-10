export function openCodeTemplate(template) {
 const dialog=document.createElement('dialog');dialog.style.cssText='width:min(1000px,90vw);max-height:85vh;overflow:auto';
 const title=document.createElement('h2');title.textContent=template.name+' · 纯代码系统';
 const note=document.createElement('p');note.textContent=template.description;
 const close=document.createElement('button');close.textContent='关闭';close.onclick=()=>dialog.close();
 const download=document.createElement('button');download.textContent='下载代码包';download.onclick=()=>{
  const url=URL.createObjectURL(new Blob([JSON.stringify(template.scripts,null,2)],{type:'application/json'}));
  const a=document.createElement('a');a.href=url;a.download=template.id+'-scripts.json';a.click();setTimeout(()=>URL.revokeObjectURL(url),1000);
 };
 dialog.append(title,note,close,download);
 for(const file of template.scripts?.files??[]){
  const row=document.createElement('details'),name=document.createElement('summary'),code=document.createElement('pre');
  name.textContent=file.path;code.textContent=file.content;code.style.whiteSpace='pre-wrap';row.append(name,code);dialog.append(row);
 }
 dialog.onclose=()=>dialog.remove();dialog.onkeydown=e=>e.stopPropagation();document.body.append(dialog);dialog.showModal();
}
