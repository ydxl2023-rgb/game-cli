"""Read-only SV Loading static-layout experiment. Requires PyYAML; not a Unity prefab resolver."""
import argparse, hashlib, json, re
from pathlib import Path
import yaml

parser=argparse.ArgumentParser()
parser.add_argument('--project',required=True)
parser.add_argument('--output',required=True)
args=parser.parse_args()
project=Path(args.project)
relative='Assets/data/UI/Public/@Prefab/Loading.prefab'
path=project/relative
source=path.read_text(encoding='utf-8-sig')
records={}
for match in re.finditer(r'^--- !u!(\d+) &(-?\d+)[^\n]*\n(.*?)(?=^--- !u!|\Z)',source,re.M|re.S):
    doc=yaml.safe_load(match[3]); kind,data=next(iter(doc.items()))
    records[int(match[2])]=(kind,data)
rects={key:data for key,(kind,data) in records.items() if kind=='RectTransform' and 'm_GameObject' in data}
roots=[key for key,data in rects.items() if data.get('m_Father',{}).get('fileID')==0]
if len(roots)!=1: raise ValueError('Expected one resolved RectTransform root')
nodes=[]; issues=[]; mapping=[]; references=[]
def name(value):
    parts=re.findall('[A-Za-z][A-Za-z0-9]*',value)
    return ''.join(p[0].upper()+p[1:] for p in parts)[:64] or 'Node'
def visit(key,parent,pw,ph):
    r=rects[key];go=records[r['m_GameObject']['fileID']][1]
    components=[records[c['component']['fileID']] for c in go['m_Component'] if c['component']['fileID'] in records]
    text=next((d for k,d in components if 'm_Text' in d),None)
    graphic=next((d for k,d in components if 'm_Color' in d),None)
    is_visual=text is not None or graphic is not None
    amin=r['m_AnchorMin'];amax=r['m_AnchorMax'];delta=r['m_SizeDelta'];pivot=r['m_Pivot'];pos=r['m_AnchoredPosition']
    w=pw*(amax['x']-amin['x'])+delta['x'];h=ph*(amax['y']-amin['y'])+delta['y']
    x=pw*(amin['x']+(amax['x']-amin['x'])*pivot['x'])+pos['x']-pivot['x']*w
    bottom=ph*(amin['y']+(amax['y']-amin['y'])*pivot['y'])+pos['y']-pivot['y']*h
    y=ph-bottom-h
    if parent is None:x=y=0;w=1920;h=1080
    if w<1 or h<1: issues.append({'node':go['m_Name'],'reason':'Nonpositive size clamped to one pixel'})
    w=max(1,w);h=max(1,h)
    color=(text or graphic or {}).get('m_Color',{'r':.1,'g':.14,'b':.2,'a':1})
    rgb='#'+''.join(f'{max(0,min(255,round(color[c]*255))):02x}' for c in ['r','g','b'])
    node={'id':'Sv'+str(abs(key)),'name':name(go['m_Name']),'parent':parent,'type':'Text' if text else 'Image' if graphic else 'Container','x':x,'y':y,'width':w,'height':h,'color':rgb,'text':text.get('m_Text','') if text else '', 'fontSize':max(8,min(128,(text or {}).get('m_FontData',{}).get('m_FontSize',32)))}
    if not go.get('m_IsActive',1):node['previewHidden']=True
    if is_visual: node['opacity']=color.get('a',1)
    else: node['surface']='transparent'
    nodes.append(node)
    mapping.append({'id':node['id'],'fileId':str(key),'name':go['m_Name'],'rect':r,'components':components})
    for kind,data in components:
        if kind=='MonoBehaviour' and not ('m_Text' in data or 'm_Color' in data):issues.append({'node':go['m_Name'],'reason':'Unmapped MonoBehaviour','script':data.get('m_Script')})
        for field in ['m_Sprite','m_Texture']:
            if data.get(field,{}).get('guid'):references.append({'node':node['id'],'field':field,'reference':data[field],'status':'placeholder'})
    for child in r.get('m_Children',[]):
        child_id=child['fileID']
        if child_id in rects and 'm_AnchorMin' in rects[child_id]: visit(child_id,node['id'],w,h)
        else: issues.append({'fileId':str(child_id),'reason':'Unresolved nested prefab/transform; requires Unity exporter'})
visit(roots[0],None,1920,1080)
issues.append({'reason':'Runtime progress, localized tips, animation and source textures are not executed in this static test.'})
output=Path(args.output);output.mkdir(parents=True,exist_ok=True)
payload={'root':nodes[0]['id'],'design':{'schemaVersion':1,'rootId':'Canvas','name':'SvLoading','width':1920,'height':1080,'nodes':nodes},'source':{'project':str(project),'path':relative,'sha256':hashlib.sha256(path.read_bytes()).hexdigest(),'mode':'static-layout-test'},'issues':issues,'resources':references,'bindings':{'Progress':'_Slider','Tip':'_TipText','LoadNum':'Rate'}}
(output/'template.json').write_text(json.dumps(payload,ensure_ascii=False,indent=2),encoding='utf-8')
(output/'source-map.json').write_text(json.dumps(mapping,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps({'nodes':len(nodes),'issues':len(issues),'resources':len(references),'output':str(output)},ensure_ascii=False))
