export const templateGroups=[
 {category:'prefab',id:'item',name:'道具',sort:10},
 {category:'prefab',id:'scroll',name:'滚动面板',sort:20},
 {category:'prefab',id:'dialog',name:'对话框',sort:30},
 {category:'prefab',id:'general',name:'未分类',sort:999},
 {category:'system',id:'loading',name:'Loading',sort:10},
 {category:'system',id:'inventory',name:'背包',sort:20},
 {category:'system',id:'general',name:'未分类',sort:999},
];
export const initialTemplateGroups={ItemSlot:'item',ScrollPanel:'scroll',ConfirmDialog:'dialog',InventorySystem:'inventory',SvLoading:'loading'};
export function groupedTemplates(category,groups,templates){
 const rows=templates.filter(t=>t.category===category),defs=groups.filter(g=>g.category===category).map(g=>({...g,templates:[]}));
 for(const t of rows){let g=defs.find(g=>g.id===(t.group??'general'));if(!g){g=defs.find(g=>g.id==='general');if(!g){g={category,id:'general',name:'未分类',sort:999,templates:[]};defs.push(g);}}g.templates.push(t);}
 return defs.filter(g=>g.templates.length).sort((a,b)=>a.sort-b.sort||a.id.localeCompare(b.id));
}
