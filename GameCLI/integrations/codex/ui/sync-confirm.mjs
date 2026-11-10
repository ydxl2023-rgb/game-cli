export function confirmTemplateSync({name,version,rootName}){
 return new Promise(resolve=>{
  const dialog=document.createElement('dialog');dialog.className='sync-confirm';dialog.setAttribute('aria-labelledby','sync-confirm-title');dialog.setAttribute('aria-describedby','sync-confirm-description');
  const title=document.createElement('h2');title.id='sync-confirm-title';title.textContent='确认更新标准模板？';
  const description=document.createElement('p');description.id='sync-confirm-description';description.textContent=`将使用当前工程中的「${rootName}」覆盖标准模板「${name}」（当前版本 v${version}）。`;
  const detail=document.createElement('p');detail.textContent='此操作会更新模板库，影响今后拖入的模板。若只想保存当前工程，请取消后点击“保存”。';
  const form=document.createElement('form');form.method='dialog';
  const cancel=document.createElement('button');cancel.value='cancel';cancel.textContent='取消';cancel.autofocus=true;
  const confirm=document.createElement('button');confirm.value='confirm';confirm.className='primary';confirm.textContent='确认更新模板';
  form.append(cancel,confirm);dialog.append(title,description,detail,form);document.body.append(dialog);
  dialog.addEventListener('keydown',e=>e.stopPropagation());
  dialog.addEventListener('close',()=>{const accepted=dialog.returnValue==='confirm';dialog.remove();resolve(accepted);},{once:true});
  dialog.showModal();
 });
}
