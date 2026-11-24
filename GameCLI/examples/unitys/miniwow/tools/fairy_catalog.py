"""Read FairyGUI package tables; preserve authored coordinates, never guess bounds.
Format reference: https://github.com/fairygui/FairyGUI-unity/blob/master/Assets/Scripts/UI/UIPackage.cs
"""
import struct
from pathlib import Path

class Reader:
    def __init__(self, data): self.data=data; self.pos=0; self.strings=[]
    def read(self, fmt):
        n=struct.calcsize('>'+fmt)
        value=struct.unpack_from('>'+fmt,self.data,self.pos)[0]; self.pos+=n
        return value
    def string(self, n=None):
        if n is None: n=self.read('H')
        value=self.data[self.pos:self.pos+n].decode('utf-8'); self.pos+=n
        return value
    def s(self):
        i=self.read('H')
        return None if i==65534 else '' if i==65533 else self.strings[i]
    def seek(self, start, block):
        old=self.pos; self.pos=start; count=self.read('B'); short=self.read('B')
        if block>=count: self.pos=old; return False
        self.pos+=block*(2 if short else 4); off=self.read('H' if short else 'I')
        self.pos=start+off if off else old
        return bool(off)

def parse(path):
    r=Reader(Path(path).read_bytes())
    if r.read('I')!=0x46475549: raise ValueError('Not an FGUI package')
    version=r.read('I'); compressed=r.read('B')
    if compressed: raise ValueError('Compressed FGUI unsupported')
    package_id=r.string(); name=r.string(); r.pos+=20; start=r.pos
    if not r.seek(start,4): raise ValueError('Missing string table')
    r.strings=[r.string() for _ in range(r.read('I'))]
    if r.seek(start,5):
        for _ in range(r.read('I')):
            index=r.read('H'); size=r.read('I'); r.strings[index]=r.string(size)
    r.seek(start,1); items=[]
    for _ in range(r.read('H')):
        length=r.read('I'); end=r.pos+length
        item={'type':r.read('B'),'id':r.s(),'name':r.s(),'source_path':r.s(),'file':r.s(),'exported':bool(r.read('B')),'width':r.read('i'),'height':r.read('i')}
        if end>len(r.data): raise ValueError('Item extent exceeds source')
        items.append(item); r.pos=end
    r.seek(start,2); sprites=[]
    for _ in range(r.read('H')):
        length=r.read('H'); end=r.pos+length
        sprite={'item_id':r.s(),'atlas_id':r.s(),'rect':[r.read('i') for _ in range(4)],'rotated':bool(r.read('B'))}
        if version>=2 and r.read('B'):
            sprite['offset']=[r.read('i'),r.read('i')];sprite['original_size']=[r.read('i'),r.read('i')]
        else:
            sprite['offset']=[0,0];sprite['original_size']=sprite['rect'][2:][::-1] if sprite['rotated'] else sprite['rect'][2:]
        if end>len(r.data): raise ValueError('Sprite extent exceeds source')
        sprites.append(sprite);r.pos=end
    return {'package_id':package_id,'name':name,'version':version,'items':items,'sprites':sprites,'limitations':['Component hierarchy, controllers and transitions are not decoded.','Item branches and resolution variants are not decoded.']}

if __name__=='__main__':
    import sys,json
    result=parse(sys.argv[1]); print(json.dumps(result,ensure_ascii=False,indent=2))
