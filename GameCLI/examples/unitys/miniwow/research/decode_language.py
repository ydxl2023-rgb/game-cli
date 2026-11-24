from pathlib import Path
import json

root = Path(__file__).resolve().parent
raw = (root / 'tables/tblanguage.bytes').read_bytes()
pos = 0

def read_size():
    global pos
    first = raw[pos]
    pos += 1
    if first < 0x80:
        return first
    if first < 0xc0:
        result = ((first & 0x3f) << 8) | raw[pos]
        pos += 1
        return result
    if first < 0xe0:
        result = ((first & 0x1f) << 16) | (raw[pos] << 8) | raw[pos+1]
        pos += 2
        return result
    raise ValueError(f'Unsupported size at {pos-1}')

def read_string():
    global pos
    size = read_size()
    value = raw[pos:pos+size].decode('utf-8')
    pos += size
    return value

count = read_size()
rows = [{'id': read_string(), 'zh': read_string(), 'en': read_string()} for _ in range(count)]
assert pos == len(raw), (pos, len(raw))
(root / 'language.json').write_text(json.dumps(rows, ensure_ascii=False, indent=2), encoding='utf-8')
for row in rows:
    if any(word in row['zh'] for word in ['战斗', '探索', '上阵', '挂机', '技能', '阵容']):
        print(row['id'], row['zh'])
print('Fully consumed', count, 'records;', pos, 'bytes')
