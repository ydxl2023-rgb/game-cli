import {readFile} from 'node:fs/promises';
// Schema: ConfigProject/src/generated/config/LanguageCfg.cs: ID tag 8, Chinese tag 26.
export async function readChineseLanguage(path){
 const bytes=await readFile(path);let offset=0;
 function varint(end){let value=0,shift=0;for(;;){if(offset>=end||shift>49)throw Error('Invalid language varint');const b=bytes[offset++];value+=(b&127)*2**shift;if(!(b&128))return value;shift+=7;}}
 function fields(end){const result=new Map();while(offset<end){const tag=varint(end),field=Math.floor(tag/8),wire=tag%8;let value;if(wire===0)value=varint(end);else if(wire===2){const size=varint(end),stop=offset+size;if(stop>end)throw Error('Invalid language length');value=bytes.subarray(offset,stop);offset=stop;}else throw Error('Unexpected language field');if(!result.has(field))result.set(field,[]);result.get(field).push(value);}return result;}
 const table=fields(bytes.length),map=new Map();for(const row of table.get(1)??[]){const saved=offset;// Use subarray offsets to preserve each row boundary.
 offset=row.byteOffset-bytes.byteOffset;const values=fields(offset+row.length);offset=saved;map.set(values.get(1)?.[0],values.get(3)?.[0]?.toString('utf8')??'');}return map;
}
