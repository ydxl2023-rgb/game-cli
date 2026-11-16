// Extract the approved basic lobby without changing the SV prefab.
import {readFile, writeFile, mkdir} from 'node:fs/promises';
import {resolve, join} from 'node:path';
import {createHash} from 'node:crypto';
import {normalizeRect, resolveLayout} from '../shared/rect-transform.mjs';
import {validateTemplate} from '../shared/templates.mjs';
import {readChineseLanguage} from '../server/sv-language.mjs';

const project = resolve(process.env.SV_PROJECT ?? 'G:/Works/SV/Development/Project');
const folder = resolve('.data/imports/sv-home');
const bytes = await readFile(join(folder, 'unity/unity-prefab.json'));
const source = JSON.parse(bytes);
if (source.sourcePath !== 'Assets/data/UI/Home/@HomeView/Prefab/Home.prefab') throw Error('Unexpected Home source.');
const hash = value => createHash('sha256').update(value).digest('hex');
const language = await readChineseLanguage(join(project, 'Assets/data/@Cfg/language_cfg.bytes'));
const exclusions = [
  ['0/0', 'SV authoring library'], ['0/2', 'Wallpaper extension'],
  ['0/3', 'Wallpaper extension'], ['0/4', 'Wallpaper extension'],
  ['0/5', 'Character presentation extension'], ['0/7', 'SV tutorial integration'],
  ['0/8', 'Platform sharing integration'], ['0/6/0/8', 'Platform SDK promotion'],
  ['0/6/0/12', 'Platform SDK integration'], ['0/6/2', 'Live activity catalog extension'],
  ['0/6/3/4', 'Live advertising extension'], ['0/6/3/5', 'Live advertising extension'],
  ['0/6/0/3', 'Character-only viewing extension'],
  ...['1','5','7','8','9','10','11'].map(n => ['0/1/1/'+n, 'Season particle/Spine extension']),
  ['0/6/5/5/4', 'SV particle effect extension'],
];
const visible = new Map();
for (const n of source.nodes) visible.set(n.path, n.activeSelf && (!n.parent || visible.get(n.parent)));
const included = source.nodes.filter(n => visible.get(n.path) && !exclusions.some(([p]) => n.path === p || n.path.startsWith(p+'/')));
const assets = new Map(), nodes = [], bindings = [], texts = [], unsupported = [];
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
  const text = lid ? language.get(Number(lid[1])) : g?.m_Text ?? '';
  const node = {id:id(n.path),name,parent:n.parent?id(n.parent):null,
    type:g?.m_FontData?'Text':button?'Button':graphic?'Image':'Container',
    x:0,y:0,width:1,height:1,color:g?'#'+['r','g','b'].map(k=>Math.round(g.m_Color[k]*255).toString(16).padStart(2,'0')).join(''):'#ffffff',
    text,fontSize:g?.m_FontData?.m_FontSize??20,active:true,rectTransform:normalizeRect(n.rectTransform),
    localScale:{x:n.rectTransform.localScale.x,y:n.rectTransform.localScale.y},rotation:n.rectTransform.localEulerAngles.z};
  if(!graphic) node.surface='transparent';
  if(g) node.opacity=g.m_Enabled===false?0:g.m_Color.a;
  if(g?.m_FontData){node.textAlign=['left','center','right'][g.m_FontData.m_Alignment%3];node.verticalAlign=['start','center','end'][Math.floor(g.m_FontData.m_Alignment/3)];}
  if(button){node.interactable=true;node.action={type:'none'};bindings.push({node:node.id,sourcePath:n.path,sourceName:n.name,name:name+'Click',kind:'ButtonClick'});}
  if(values[n.path]) bindings.push({node:node.id,sourcePath:n.path,name:values[n.path],kind:'string'});
  if(text && !values[n.path]){
    const alias='HOME_TEXT_'+n.path.replaceAll('/','_');
    texts.push({node:node.id,alias,text,sourceLanguageId:lid?Number(lid[1]):null});
    bindings.push({node:node.id,sourcePath:n.path,name:'Label'+n.path.replaceAll('/',''),kind:'string',alias});
    node.name='Label'+n.path.replaceAll('/','');
  }
  const ref=graphic?.references.find(r=>['m_Sprite','m_Texture'].includes(r.property)&&r.path);
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
const report={schemaVersion:1,template:'SvHome',status:'draft',scope:'Basic lobby; character and seasonal animation are extension points.',source:{prefab:source.sourcePath,guid:source.sourceGuid,exportSha256:hash(bytes),prefabSha256:hash(await readFile(join(project,source.sourcePath)))},sourceNodes:source.nodes.length,nodes:design.nodes.length,images:assets.size,exclusions,bindings,texts,extensions:['character-presentation','season-presentation','activity-catalog','advertising','external-system-navigation'],limitations:['Static summer artwork retained. Spine, particles and character are excluded by the approved first-version scope.','Original baked image labels remain image assets. Text-component labels require propCommonText integration.','SV shader, text outline, tween, live operations and remote services are not executed.']};
await writeFile(join(folder,'template.json'),JSON.stringify(payload,null,2));
await writeFile(join(folder,'report.json'),JSON.stringify(report,null,2));
await writeFile(join(folder,'assets/asset-names.json'),JSON.stringify(Object.fromEntries([...assets].map(([k,v])=>[k,v.path])),null,2));
console.log(JSON.stringify({nodes:design.nodes.length,images:assets.size,buttons:bindings.filter(b=>b.kind==='ButtonClick').length,texts:texts.length,output:folder}));
