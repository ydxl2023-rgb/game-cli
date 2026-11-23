import {readFile, writeFile} from 'node:fs/promises';
import {join} from 'node:path';
import {readChineseLanguage} from '../server/sv-language.mjs';

const cfg = join(process.env.SV_PROJECT ?? 'G:/Works/SV/Development/Project','Assets/data/@Cfg');
const language = await readChineseLanguage(join(cfg,'language_cfg.bytes'));
export function fields(bytes) {
  let offset = 0;
  function integer() {
    let value = 0n;
    for (let shift = 0n; offset < bytes.length && shift < 70n; shift += 7n) {
      const byte = bytes[offset++];
      value |= BigInt(byte & 127) << shift;
      if (byte < 128) return Number(BigInt.asIntN(32,value));
    }
    throw Error('Invalid protobuf integer');
  }
  const result = new Map();
  while (offset < bytes.length) {
    const tag = integer();
    let value;
    if (tag % 8 === 0) value = integer();
    else if (tag % 8 === 2) {
      const size = integer();
      if (size < 0 || offset + size > bytes.length) throw Error('Invalid protobuf length');
      value = bytes.subarray(offset,offset + size);
      offset += size;
    } else throw Error('Unsupported protobuf field '+tag);
    const key = tag >> 3;
    if (!result.has(key)) result.set(key,[]);
    result.get(key).push(value);
  }
  return result;
}
export async function rows(name) {
  return fields(await readFile(join(cfg,name+'_cfg.bytes'))).get(1).map(fields);
}
export function label(value) {
  const text = String(value ?? '');
  return text.replace(/\[LID:(\d+)\]/g,(raw,id)=>language.get(Number(id)) || raw);
}

