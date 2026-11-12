// Shared primary navigation: same location in the library and both editors.
export function workspaceNavigation({template=false,embedded=false}={}){
 const nav=document.createElement('nav');nav.className='workspace-navigation';nav.setAttribute('aria-label','工作台分页');
 for(const [id,name]of [['projects','项目'],['templates','模板']]){
  const link=document.createElement('a');link.id='workspace-tab-'+id;link.href=(embedded?'http://127.0.0.1:18091':'')+'/studio/'+id;link.textContent=name;
  const active=(id==='templates')===template;if(active)link.setAttribute('aria-current','page');
  if(embedded){link.target='_blank';link.rel='noopener noreferrer';}
  link.onkeydown=e=>{if(!['ArrowLeft','ArrowRight','Home','End'].includes(e.key))return;e.preventDefault();const links=[...nav.children],index=links.indexOf(link);links[e.key==='Home'?0:e.key==='End'?links.length-1:(index+1)%links.length].focus();};
  nav.append(link);
 }
 return nav;
}
