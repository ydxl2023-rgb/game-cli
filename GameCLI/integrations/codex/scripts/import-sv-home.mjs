// Extract the approved basic lobby without changing the SV prefab.
import {readFile, writeFile, mkdir} from 'node:fs/promises';
import {resolve, join} from 'node:path';
import {createHash} from 'node:crypto';
import {normalizeRect, resolveLayout} from '../shared/rect-transform.mjs';
import {validateTemplate} from '../shared/templates.mjs';
import {readChineseLanguage} from '../server/sv-language.mjs';

const project = resolve(process.env.SV_PROJECT ?? 'G:/Works/SV/Development/Project');
const folder = resolve(process.env.HOME_OUTPUT ?? '.data/imports/sv-home');
const bytes = await readFile(resolve('.data/imports/sv-home/unity/unity-prefab.json'));
const source = JSON.parse(bytes);
if (source.sourcePath !== 'Assets/data/UI/Home/@HomeView/Prefab/Home.prefab') throw Error('Unexpected Home source.');
const hash = value => createHash('sha256').update(value).digest('hex');
const language = await readChineseLanguage(join(project, 'Assets/data/@Cfg/language_cfg.bytes'));
const example = JSON.parse(await readFile('config/home-presentation.json','utf8'));
const exclusions = [
  ['0/0', 'SV authoring library'], ['0/2', 'Wallpaper extension'],
  ['0/3', 'Wallpaper extension'], ['0/4', 'Wallpaper extension'],
  ['0/5', 'Character presentation extension'], ['0/7', 'SV tutorial integration'],
  ['0/8', 'Platform sharing integration'], ['0/6/0/8', 'Platform SDK promotion'],
  ['0/6/0/12', 'Platform SDK integration'],
  ['0/6/3/4', 'Live advertising extension'], ['0/6/3/5', 'Live advertising extension'],
  ['0/6/0/3', 'Character-only viewing extension'],
  ...['1','5','7','8','9','10','11'].map(n => ['0/1/1/'+n, 'Season particle/Spine extension']),
  ['0/6/5/5/4', 'SV particle effect extension'],
];
const visible = new Map();
for (const n of source.nodes) visible.set(n.path, n.activeSelf && (!n.parent || visible.get(n.parent)));
// Inactive badges are runtime states, not unused artwork. Keep their full UI
// subtree and separately record unsupported native effects instead of losing
// the parent badge together with its effect.
const stateRoots = source.nodes.filter(n => /NewTag|Tips$|^AdsNode$|^ImgDoubleReward$/.test(n.name) || (n.path.startsWith('0/6/2/')&&/RedDot/.test(n.name)) || n.parent === '0/6/2/0').map(n => n.path);
const under = (path, root) => path === root || path.startsWith(root + '/');
const effectRoots = source.nodes.filter(n => n.components.some(c => /Spine|ParticleSystem|MeshRenderer/.test(c.type))).map(n => n.path);
const included = source.nodes.filter(n => (visible.get(n.path) || under(n.path,'0/6/2') || stateRoots.some(p=>under(n.path,p)))
  && !exclusions.some(([p])=>under(n.path,p)) && !effectRoots.some(p=>under(n.path,p)));
const assets = new Map(), nodes = [], bindings = [], texts = [], unsupported = [];
const textStyles = [], states = [], layouts = [], effects = [];
for (const path of effectRoots) if (!exclusions.some(([p])=>under(path,p))) effects.push({path,types:source.nodes.find(n=>n.path===path).components.filter(c=>/Spine|ParticleSystem|MeshRenderer/.test(c.type)).map(c=>c.type),sourceArchiveSha256:hash(bytes)});
const id = path => 'Home'+path.replaceAll('/', '_');
const aliases = {
  PlayerHead:'Profile', PlayerHeadNode:'Avatar', _MailButton:'Mail', _SetButton:'Settings',
  AnnouncementButton:'Notice', FriendButton:'Friends', FavorityButton:'Affinity',
  ShangChengButton:'Shop', FuliButton:'Benefits', PaiHangButton:'Rankings', ChatFrame:'Chat',
  Chat:'ChatPanel', ChatIcon:'ChatChannel', BeiBaoButton:'Backpack', TuJianButton:'Collection', HonorButton:'Honor',
  JuqingButton:'Story', SaishiButton:'Matches', HesujidiButton:'Training', ZhaomuButton:'Recruit',
  DuiyuanButton:'Players', DuiwuButton:'Team', SheTuanButton:'Club', HuodongButton:'Activities',
  icon_1:'CurrencyOne', icon_2:'CurrencyTwo', icon_3:'CurrencyThree',
  add_1:'AddCurrencyOne', add_2:'AddCurrencyTwo', add_3:'AddCurrencyThree',
};
const unimplementedTextPlaceholders = [];
const values = {'0/6/0/0/4':'PlayerName','0/6/0/0/6/0/0':'PlayerLevel',
  '0/6/1/0/0/2':'CurrencyOneAmount','0/6/1/0/1/2':'CurrencyTwoAmount',
  '0/6/1/0/2/2':'CurrencyThreeAmount','0/6/3/3/1':'ChatMessage'};
for (const n of included) {
  const components = n.components.filter(c=>c.json).map(c=>({...c,data:Object.values(JSON.parse(c.json))[0]}));
  const graphic = components.find(c=>c.data?.m_Color && ('m_FontData' in c.data || 'm_Sprite' in c.data || 'm_Texture' in c.data));
  const button = components.find(c=>c.type.startsWith('UnityEngine.UI.Button,'));
  const g = graphic?.data;
  const name = values[n.path] ?? aliases[n.name] ?? n.name.replace(/[^a-zA-Z0-9]+(.)?/g,(_,c)=>c?.toUpperCase()??'');
  const lid = g?.m_Text?.match(/^\[LID:(\d+)\]$/);
  if(lid && !language.has(Number(lid[1]))) throw Error('Missing language ID '+lid[1]);
  const sourceText = lid ? language.get(Number(lid[1])) : g?.m_Text ?? '';
  const dynamicText = !values[n.path] && /^(\[FID:\d+\]|\d+:\d\d:\d\d|\d+\s*\/\s*\d+|\d+)$/.test(sourceText);
  const text = dynamicText ? '' : sourceText;
  const node = {id:id(n.path),name,parent:n.parent?id(n.parent):null,
    type:g?.m_FontData?'Text':button?'Button':graphic?'Image':'Container',
    x:0,y:0,width:1,height:1,color:g?'#'+['r','g','b'].map(k=>Math.round(g.m_Color[k]*255).toString(16).padStart(2,'0')).join(''):'#ffffff',
    text,fontSize:g?.m_FontData?.m_FontSize??20,active:n.activeSelf,rectTransform:normalizeRect(n.rectTransform),
    localScale:{x:n.rectTransform.localScale.x,y:n.rectTransform.localScale.y},rotation:n.rectTransform.localEulerAngles.z};
  if(!graphic) node.surface='transparent';
  if(g) node.opacity=g.m_Enabled===false?0:g.m_Color.a;
  if(g?.m_FontData){node.textAlign=['left','center','right'][g.m_FontData.m_Alignment%3];node.verticalAlign=['start','center','end'][Math.floor(g.m_FontData.m_Alignment/3)];}
  if(g?.m_FontData) textStyles.push({node:node.id,sourcePath:n.path,font:graphic.references.find(r=>r.property==='m_FontData.m_Font')??null,fontData:g.m_FontData,outline:components.find(c=>c.type.includes('OutlineEx'))??null,material:graphic.references.find(r=>r.property==='m_Material'&&r.path)??null});
  if(stateRoots.includes(n.path)) {
    if(n.parent==='0/6/2/0') node.active=example.activities.includes(n.path);
    else if(example.visibleIndicators.includes(n.path)) node.active=true;
    states.push({node:node.id,sourcePath:n.path,name:n.name,defaultActive:node.active,activity:n.parent==='0/6/2/0'});
  }
  const grid=components.find(c=>c.type.startsWith('UnityEngine.UI.GridLayoutGroup,'));
  if(grid) layouts.push({node:node.id,sourcePath:n.path,type:'GridLayoutGroup',data:grid.data});
  if(button){node.interactable=true;node.action={type:'none'};bindings.push({node:node.id,sourcePath:n.path,sourceName:n.name,name:name+'Click',kind:'ButtonClick'});}
  if(values[n.path]) bindings.push({node:node.id,sourcePath:n.path,name:values[n.path],kind:'string'});
  if(dynamicText){
    node.name='Label'+n.path.replaceAll('/','');
    // Unimplemented activity placeholders stay empty in the prefab, without VM fields.
    unimplementedTextPlaceholders.push({node:node.id,sourcePath:n.path,sourceText});
  }
  if(text && !values[n.path]){
    const alias='HOME_TEXT_'+n.path.replaceAll('/','_');
    texts.push({node:node.id,alias,text,sourceLanguageId:lid?Number(lid[1]):null});
    // Static text belongs to the prefab; retain its alias only as localization source material.
    node.name='Label'+n.path.replaceAll('/','');
  }
  let ref=graphic?.references.find(r=>['m_Sprite','m_Texture'].includes(r.property)&&r.path);
  if(example.images[n.path] && graphic) ref={path:example.images[n.path]};
  if(n.path==='0/6/5/0/4') {ref={path:'Assets/data/Language/Cn/UI/@Activity/Atlas/hlhd_3bei.png'};node.active=true;node.rectTransform.sizeDelta={x:194,y:104};}
  if(ref && ref.path!=='Resources/unity_builtin_extra'){
    if(!ref.path.endsWith('.png')) throw Error('Unsupported graphic: '+ref.path);
    const body=await readFile(join(project,ref.path)),key=hash(body);
    assets.set(key,{body,path:ref.path});node.image=key;node.imageFit=g.m_PreserveAspect?'contain':'100% 100%';
    if(g.m_Type===1){const meta=await readFile(join(project,ref.path+'.meta'),'utf8');const border=meta.match(/spriteBorder: \{x: ([\d.]+), y: ([\d.]+), z: ([\d.]+), w: ([\d.]+)\}/);if(!border)throw Error('Missing border');node.imageBorder=Object.fromEntries(['x','y','z','w'].map((k,i)=>[k,Number(border[i+1])]));node.imageType='sliced';}
  }
  for(const c of components) if(/Spine|Particle|MeshRenderer/.test(c.type)) unsupported.push({node:node.id,type:c.type});
  if(n.path==='0'){node.localScale={x:1,y:1};node.rectTransform={anchorMin:{x:0,y:0},anchorMax:{x:1,y:1},pivot:{x:.5,y:.5},anchoredPosition:{x:0,y:0},sizeDelta:{x:0,y:0}};}
  if(included.some(child=>child.parent===n.path) && node.opacity<1 && !node.image){
    const alpha=node.opacity;
    node.opacity=1;node.surface='transparent';nodes.push(node);
    if(alpha>0)nodes.push({id:node.id+'Surface',name:'Background',parent:node.id,type:'Image',x:0,y:0,width:1,height:1,color:node.color,text:'',fontSize:20,opacity:alpha,rectTransform:{anchorMin:{x:0,y:0},anchorMax:{x:1,y:1},pivot:{x:.5,y:.5},anchoredPosition:{x:0,y:0},sizeDelta:{x:0,y:0}}});
  }else nodes.push(node);
}
if(unsupported.length) throw Error('Unmapped dynamic components: '+JSON.stringify(unsupported));
// Resolve the source's column-major activity grid for the web reference. Native
// generation also restores GridLayoutGroup, so visibility changes reflow it.
for(const layout of layouts){
  const data=layout.data,children=nodes.filter(n=>n.parent===layout.node && n.active!==false);
  if(data.m_StartAxis!==1 || data.m_Constraint!==2) throw Error('Unreviewed activity grid layout');
  children.forEach((node,index)=>{
    const column=Math.floor(index/data.m_ConstraintCount),row=index%data.m_ConstraintCount;
    node.rectTransform={anchorMin:{x:0,y:1},anchorMax:{x:0,y:1},pivot:{x:.5,y:.5},sizeDelta:data.m_CellSize,
      anchoredPosition:{x:data.m_Padding.m_Left+column*(data.m_CellSize.x+data.m_Spacing.x)+data.m_CellSize.x/2,
      y:-data.m_Padding.m_Top-row*(data.m_CellSize.y+data.m_Spacing.y)-data.m_CellSize.y/2}};
  });
}
const design=resolveLayout({schemaVersion:1,name:'SvHome',width:1920,height:1080,nodes});
for(const n of design.nodes){n.width=Math.max(1,n.width);n.height=Math.max(1,n.height);}
const payload=validateTemplate({root:id('0'),design},'system');
// Retain the separately reviewed entrance contract on future extractions.
try { payload.design.animations=JSON.parse(await readFile('config/home-entrance.json','utf8'));validateTemplate(payload,'system'); }
catch(error){if(error.code!=='ENOENT')throw error;}
// Zero-size layout anchors are valid Unity containers. The transport requires
// positive fallback bounds; their native RectTransform still remains unchanged.
for(const n of payload.design.nodes){n.width=Math.max(1,n.width);n.height=Math.max(1,n.height);}
await mkdir(join(folder,'assets'),{recursive:true});
for(const [key,a] of assets) await writeFile(join(folder,'assets',key+'.png'),a.body);
const activityTemplates=JSON.parse(await readFile('config/home-functions.json','utf8'));
const pageBindings=bindings.filter(b=>!b.sourcePath.startsWith('0/6/2/0/')).map(b=>b.kind==='ButtonClick'?{...b,name:b.name.replace(/Click$/,'Entry'),kind:'VMActivityEntry',syncType:'SyncMetadata'}:b);
pageBindings.unshift({node:id('0/6/2/0'),sourcePath:'0/6/2/0',name:'AcitiveList',kind:'VList<VMActivityEntry>'});
const report={schemaVersion:1,template:'SvHome',status:'draft',scope:'Basic lobby with stateful badges and activity presentation; character and seasonal animation are extension points.',source:{prefab:source.sourcePath,guid:source.sourceGuid,exportSha256:hash(bytes),prefabSha256:hash(await readFile(join(project,source.sourcePath)))},sourceNodes:source.nodes.length,nodes:design.nodes.length,images:assets.size,exclusions,bindings:pageBindings,activityTemplates,texts,unimplementedTextPlaceholders,extensions:['character-presentation','season-presentation','external-system-navigation','remote-service-integration'],limitations:['Static summer artwork retained; character and seasonal effects are deferred.','Original baked image labels remain image assets. Static Text values are serialized in the prefab; propCommonText is retained as localization source material, not a Home VM dependency.','Native typography, layout, state and effects require binding from presentation.json before publication.','Runtime activity artwork must be supplied from verified source configuration; default prefab images do not prove the historical screenshot state.']};
await writeFile(join(folder,'template.json'),JSON.stringify(payload,null,2));
await writeFile(join(folder,'report.json'),JSON.stringify(report,null,2));
await writeFile(join(folder,'assets/asset-names.json'),JSON.stringify(Object.fromEntries([...assets].map(([k,v])=>[k,v.path])),null,2));
// Native presentation dependencies are required even when they cannot be
// represented by the web layout schema. A publisher must resolve this contract.
await writeFile(join(folder,'presentation.json'),JSON.stringify({schemaVersion:1,textStyles,states,layouts,effects,dropVariants:{double:'Assets/data/Language/Cn/UI/@Activity/Atlas/UI_System_DoubleDrop_02.png',triple:'Assets/data/Language/Cn/UI/@Activity/Atlas/hlhd_3bei.png'},status:'requires-native-binding'},null,2));
console.log(JSON.stringify({nodes:design.nodes.length,images:assets.size,buttons:bindings.filter(b=>b.kind==='ButtonClick').length,texts:texts.length,output:folder}));
