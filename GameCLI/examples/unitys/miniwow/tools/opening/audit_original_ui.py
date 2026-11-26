"""Read authored FGUI object rectangles; do not infer final runtime layout from base coordinates."""
from pathlib import Path
import sys,json,hashlib
root=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(root/'tools'))
from fairy_catalog import Reader

def package(file, names):
    data=file.read_bytes();r=Reader(data)
    assert r.read('I')==0x46475549
    version=r.read('I');assert r.read('B')==0
    package_id=r.string();package_name=r.string();r.pos+=20;start=r.pos
    assert r.seek(start,4)
    r.strings=[r.string() for _ in range(r.read('I'))]
    if r.seek(start,5):
        for _ in range(r.read('I')):
            index=r.read('H');length=r.read('I');r.strings[index]=r.string(length)
    r.seek(start,1);items={};raw={}
    for _ in range(r.read('H')):
        length=r.read('I');end=r.pos+length
        item=dict(type=r.read('B'),id=r.s(),name=r.s(),sourcePath=r.s(),file=r.s(),exported=bool(r.read('B')),width=r.read('i'),height=r.read('i'))
        items[item['id']]=item
        if item['type']==0:
            item['scaleOption']=r.read('B')
            if item['scaleOption']==1:
                item['scale9Grid']=[r.read('i') for _ in range(4)]
        if item['type']==3:
            item['extension']=r.read('B');length=r.read('I');raw[item['id']]=data[r.pos:r.pos+length]
        r.pos=end
    result=[]
    for item in items.values():
        if item['name'] not in names:continue
        c=Reader(raw[item['id']]);c.strings=r.strings
        children=[]
        if c.seek(0,0):
            c.pos+=8
            if c.read('B'): c.pos+=16
            if c.read('B'): item['pivot']=[c.read('f'),c.read('f'),bool(c.read('B'))]
        assert c.seek(0,2)
        for _ in range(c.read('H')):
            length=c.read('H');begin=c.pos;end=begin+length
            assert c.seek(begin,0)
            node=dict(type=c.read('B'),sourceId=c.s(),packageId=c.s(),id=c.s(),name=c.s(),x=c.read('i'),y=c.read('i'))
            if c.read('B'):node['size']=[c.read('i'),c.read('i')]
            elif node['sourceId'] in items:node['size']=[items[node['sourceId']]['width'],items[node['sourceId']]['height']]
            if c.read('B'):node['sizeConstraints']=[c.read('i') for _ in range(4)]
            if c.read('B'):node['scale']=[c.read('f'),c.read('f')]
            if c.read('B'):node['skew']=[c.read('f'),c.read('f')]
            if c.read('B'):node['pivot']=[c.read('f'),c.read('f'),bool(c.read('B'))]
            node.update(alpha=c.read('f'),rotation=c.read('f'),visible=bool(c.read('B')),touchable=bool(c.read('B')))
            if node['sourceId'] in items:node['sourceName']=items[node['sourceId']]['name']
            if node['type']==9 and c.seek(begin,6) and c.read('B')==12:
                node['buttonOverrides']=[c.s() for _ in range(4)]
            children.append(node);c.pos=end
        result.append(dict(**item,children=children))
    return dict(package=package_name,packageId=package_id,version=version,source=str(file.relative_to(root)),sha256=hashlib.sha256(data).hexdigest(),components=result,images=[i for i in items.values() if i['type']==0],limits='Base authored properties only. Relations, controller gears, transitions and viewport adaptation are not applied.')

if __name__ == "__main__":
    result=[package(root/f'extracted-resources/assets/data/ui/atlas/{name}/{name}_fui.bytes',wanted) for name,wanted in [
        ('guide',['ConversationPage','FakeGuideFingerPage']),
        ('building',['BuildingResTipsBubble','BuildingOpBubble','BuyAnimIcon']),
        ('main',['MainPage','TaskCom'])]]
    (root/'tools/opening/original-ui-audit.json').write_text(json.dumps(result,ensure_ascii=False,indent=2),encoding='utf-8')
    for p in result:
        for c in p['components']:
            print(p['package'],c['name'],c['width'],c['height'])
            for n in c['children']:
                if c['name']!='MainPage' or n['name'] in ['TaskCom','JoystickPivot','BubbleLayer']:
                    print(n)
