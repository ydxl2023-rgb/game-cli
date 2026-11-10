export function createCardScripts({getDesign,getTemplate,onOpen}){
 const panel=document.createElement('details');panel.id='card-scripts';panel.open=true;panel.hidden=true;
 const heading=document.createElement('summary');heading.textContent='代码文件';
 const context=document.createElement('p');context.className='scripts-context';
 const list=document.createElement('ul');list.className='scripts-list';
 const empty=document.createElement('p');empty.className='scripts-empty';
 panel.append(heading,context,list,empty);document.querySelector('.palette .unity-state').before(panel);
 let selected=null,enabled=false;
 function render(){
  panel.hidden=!enabled;if(!enabled)return;
  list.replaceChildren();context.textContent='';empty.hidden=false;
  const root=getDesign().nodes.find(n=>n.id===selected&&n.parent===null);
  if(!root){empty.textContent='点击关系卡片查看对应的代码文件。';return;}
  const template=getTemplate(root);
  context.textContent=root.name+(template?' · '+template.name:'');
  if(!template){empty.textContent='此界面未关联可读取的数据库模板。';return;}
  const scripts=template.scripts;
  if(!scripts){empty.textContent='该模板尚未保存原始代码。';return;}
  if(scripts.schemaVersion!==1||!Array.isArray(scripts.files)){empty.textContent='代码文件数据格式无效，请检查模板 scripts 字段。';return;}
  const files=scripts.files;
  context.textContent+=` · ${files.length} 个文件`;
  empty.hidden=files.length>0;empty.textContent='该模板的代码文件列表为空。';
  for(const file of files){
   const row=document.createElement('li'),button=document.createElement('button'),name=document.createElement('strong'),path=document.createElement('span');
   const relativePath=String(file.path??'未命名文件');name.textContent=relativePath.split('/').at(-1);path.textContent=relativePath;button.type='button';button.title='预览 '+relativePath;button.setAttribute('aria-label','预览代码 '+relativePath);button.onclick=()=>onOpen({...file,path:relativePath},template.name);button.append(name,path);row.append(button);list.append(row);
  }
 }
 return {render,select(id){selected=id;panel.open=true;render();},setVisible(value){enabled=value;render();}};
}
