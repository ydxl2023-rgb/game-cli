"""Read-only source package recovery; format reference: BlackTrace/pc_wxapkg_decrypt."""
from pathlib import Path, PurePosixPath
import hashlib, json, struct
from cryptography.hazmat.primitives.ciphers import Cipher, algorithms, modes
ROOT=Path(__file__).resolve().parents[1]
OUT=ROOT/'research/code-study/packages'
APPID='wxecf0e106809a477d'
def sha(b): return hashlib.sha256(b).hexdigest()
def save(p,b):
    p.parent.mkdir(parents=True,exist_ok=True)
    if p.exists() and p.read_bytes()!=b: raise ValueError(f'refusing overwrite: {p}')
    p.write_bytes(b)
    assert sha(p.read_bytes())==sha(b)
records=[]
for p in sorted((ROOT/APPID/'packages/6').glob('*.wxapkg')):
    original=p.read_bytes(); data=original
    if data[:6]==b'V1MMWX':
        key=hashlib.pbkdf2_hmac('sha1',APPID.encode(),b'saltiest',1000,32)
        d=Cipher(algorithms.AES(key),modes.CBC(b'the iv: 16 bytes')).decryptor()
        head=d.update(data[6:1030])+d.finalize()
        assert head[-1:]==b'\x01'
        data=head[:-1]+bytes(x^ord(APPID[-2]) for x in data[1030:])
    assert data[0]==0xbe and data[13]==0xed
    _,indexsize,bodysize=struct.unpack_from('>III',data,1)
    assert 14+indexsize+bodysize==len(data)
    count=struct.unpack_from('>I',data,14)[0]; pos=18; members=[]
    save(OUT/'decrypted'/p.name,data)
    for i in range(count):
        n=struct.unpack_from('>I',data,pos)[0]; pos+=4
        name=data[pos:pos+n].decode();pos+=n
        offset,size=struct.unpack_from('>II',data,pos);pos+=8
        rel=PurePosixPath(name.lstrip('/'))
        assert '..' not in rel.parts and ':' not in name and '\\' not in name
        assert offset>=14+indexsize and offset+size<=len(data)
        content=data[offset:offset+size]; dest=OUT/'members'/p.stem/str(rel)
        assert dest.resolve().is_relative_to(OUT.resolve())
        save(dest,content)
        members.append(dict(name=name,offset=offset,bytes=size,sha256=sha(content),path=str(dest.relative_to(ROOT))))
    assert pos==14+indexsize
    records.append(dict(source=str(p.relative_to(ROOT)),source_sha256=sha(original),decoded_sha256=sha(data),members=members))
OUT.mkdir(parents=True,exist_ok=True)
(OUT/'manifest.json').write_text(json.dumps(records,ensure_ascii=False,indent=2),encoding='utf-8')
print(json.dumps([{'package':r['source'],'members':len(r['members']),'files':[m['name'] for m in r['members']]} for r in records],ensure_ascii=False))
