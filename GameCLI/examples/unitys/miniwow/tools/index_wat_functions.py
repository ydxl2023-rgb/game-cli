"""Index WAT function text byte ranges for bounded, reproducible reading."""
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1] / 'research/code-study'


def main():
    summary = []
    for path in sorted((ROOT / 'packages/decoded').rglob('*.wat')):
        functions = []
        offset = 0
        with path.open('rb') as source:
            for number, line in enumerate(source, 1):
                if line.startswith(b'  ('):
                    if functions and 'end_byte' not in functions[-1]:
                        functions[-1]['end_byte'] = offset
                    if line.startswith(b'  (func '):
                        functions.append({'line': number, 'start_byte': offset,
                            'signature': line.decode('utf-8').rstrip()})
                offset += len(line)
        if functions and 'end_byte' not in functions[-1]:
            functions[-1]['end_byte'] = offset
        output = path.with_suffix('.functions.json')
        output.write_text(json.dumps({'wat': path.relative_to(ROOT).as_posix(),
            'warning': 'WAT names and instructions, not original source. End byte is exclusive.',
            'functions': functions}, ensure_ascii=False, indent=2), 'utf-8')
        summary.append({'wat': path.relative_to(ROOT).as_posix(), 'functions': len(functions)})
    print(json.dumps(summary))


if __name__ == '__main__':
    main()
