exec(open('GameCLI/examples/unitys/practice/runs/audit_v4.py').read().split('rows=[]')[0])
for page in ['Home/Home','Settings/Settings','Shop/Shop']:
 f=p/'Assets/Data/Widgets'/f'{page}.prefab'
 if not f.exists():continue
 blocks=re.split(r'--- !u!',f.read_text());names={}
 for b in blocks:
  if b.startswith('1 &') and re.search(r'  m_Name: (.*)',b):
   names[re.match(r'1 &(\d+)',b)[1]]=re.search(r'  m_Name: (.*)',b)[1]
 for b in blocks:
  m=re.search(r'm_Sprite:.*guid: (\w+)',b)
  if m:
   i=byguid.get(m[1]);go=re.search(r'm_GameObject: \{fileID: (\d+)\}',b)
   name=names.get(go[1],'?') if go else '?'
   if page.startswith('Settings') or i in [54,63]:print(page,name,i)
for i in [54,63,198,199,237,238]:
 e=items[i];print(i,e['paths']);im=Image.open(old/'originals'/e['paths'][0]);print(im.size,im.getchannel('A').getbbox())

