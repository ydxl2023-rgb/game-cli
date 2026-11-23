exec(open('GameCLI/examples/unitys/practice/runs/audit_v4.py').read().split('rows=[]')[0])
f=p/'Assets/Data/Widgets/Home/Home.prefab';blocks=re.split(r'--- !u!',f.read_text());gos={};rects={};grect={}
for b in blocks:
 m=re.match(r'(\d+) &(-?\d+)',b)
 if not m:continue
 if m[1]=='1' and '  m_Name:' in b:gos[m[2]]=re.search(r'  m_Name: (.*)',b)[1]
 if m[1]=='224' and 'm_GameObject:' in b:
  go=re.search(r'm_GameObject: \{fileID: (-?\d+)\}',b)[1];rects[m[2]]=b;grect[go]=m[2]
for go,name in gos.items():
 if name in ['Recruit','Training']:
  rid=grect[go]
  for depth in range(4):
   b=rects[rid];print(name,depth,[(k,re.search(k+r': (.*)',b)[1]) for k in ['m_AnchoredPosition','m_SizeDelta','m_Pivot']]);rid=re.search(r'm_Father: \{fileID: (-?\d+)\}',b)[1]
   if rid not in rects:break

