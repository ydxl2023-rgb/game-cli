"""Read building display references using the recovered CfgBuilding serialization schema."""
from pathlib import Path
import struct,json
ROOT=Path(__file__).resolve().parents[2]
data=(ROOT/'extracted-resources/config-tables/tbbuilding.bytes').read_bytes(); pos=0
def integer():
 global pos
 b=data[pos];pos+=1
 if b<128:return b
 n,mask=(1,63) if b<192 else (2,31) if b<224 else (3,15) if b<240 else (4,0)
 v=b&mask
 for _ in range(n):v=(v<<8)|data[pos];pos+=1
 return v if v<2147483648 else v-4294967296
def string():
 global pos
 n=integer();s=data[pos:pos+n].decode();pos+=n;return s
rows=[]
for _ in range(integer()):
 row=dict(id=integer(),name=string(),type=integer(),feature=integer())
 for key in ['npcName','npcModel','npcUiModel','npcIcon','initialModelPath','effect']:row[key]=string()
 row['effectTransform']=struct.unpack_from('<9f',data,pos);pos+=36
 has=data[pos];pos+=1
 if has:
  kind=integer()
  if kind==-476586193:
   integer()
   for i in range(integer()):integer()
  else:raise ValueError(('Unhandled effect',kind,pos))
 for key in ['guideDelay','guideConversation','jumpType']:row[key]=integer()
 rows.append(row)
assert pos==len(data),(pos,len(data))
(Path(__file__).parent/'building-config.json').write_text(json.dumps(rows,ensure_ascii=False,indent=2),'utf8')
print(json.dumps([dict(id=r['id'],name=r['name'],model=r['initialModelPath']) for r in rows],ensure_ascii=False))
