from pathlib import Path
import struct
root=Path(r'G:\Works\GameCLI\GameCLI\examples\unitys\miniwow')
b=next((root/'wxecf0e106809a477d/game-cache').glob('*bin')).read_bytes()
end=struct.unpack_from('<I',b,16)[0]; pos=20
while pos<end:
 off,size,n=struct.unpack_from('<III',b,pos);pos+=12
 name=b[pos:pos+n].decode();pos+=n
 p=root/'research/extracted'/name;p.parent.mkdir(parents=True,exist_ok=True);p.write_bytes(b[off:off+size]); print(name,size)
