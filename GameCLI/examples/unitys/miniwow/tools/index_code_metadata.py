"""Index retained REA metadata without pretending symbols are source code."""
import hashlib
import json
from collections import defaultdict
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OUT = ROOT / 'research/code-study'


def main():
    bundle = json.loads((OUT / 'evidence/rea-metadata.json').read_text('utf-8'))
    evidence = next(r for r in bundle['records'] if r['operation'] == 'inspect_il2cpp_metadata')
    data = evidence['normalized_result']
    target = OUT / 'metadata-symbols'
    target.mkdir(exist_ok=True)
    members = {}
    for collection in ['methods', 'fields', 'properties', 'events']:
        grouped = defaultdict(list)
        for index, row in enumerate(data[collection]):
            grouped[row.get('declaring_type')].append({'index': index, **row})
        members[collection] = grouped
    index_rows = []
    for assembly in data['images']:
        rows = []
        for index in range(assembly['type_start'], assembly['type_start'] + assembly['type_count']):
            rows.append({'index': index, **data['types'][index],
                         **{key: value.get(index, []) for key, value in members.items()}})
        filename = assembly['name'] + '.symbols.json'
        (target / filename).write_text(json.dumps({'evidence_id': evidence['evidence_id'],
            'warning': 'Metadata symbols only. No method bodies or native type signatures recovered.',
            'assembly': assembly, 'types': rows}, ensure_ascii=False, indent=2), 'utf-8')
        index_rows.append({**assembly, 'file': filename,
                           'method_count': sum(len(t['methods']) for t in rows)})
    (target / 'index.json').write_text(json.dumps(index_rows, indent=2), 'utf-8')
    (target / 'string-literals.json').write_text(json.dumps(data['string_literals'], ensure_ascii=False, indent=2), 'utf-8')
    inventory = []
    source = ROOT / 'wxecf0e106809a477d'
    for path in sorted(source.rglob('*')):
        if path.is_file():
            payload = path.read_bytes()
            inventory.append({'path': path.relative_to(source).as_posix(), 'bytes': len(payload),
                              'sha256': hashlib.sha256(payload).hexdigest(), 'magic_hex': payload[:16].hex()})
    (OUT / 'evidence/source-inventory.json').write_text(json.dumps(inventory, ensure_ascii=False, indent=2), 'utf-8')
    print(json.dumps({'images': len(index_rows), 'sources': len(inventory), 'evidence_id': evidence['evidence_id']}))


if __name__ == '__main__':
    main()
