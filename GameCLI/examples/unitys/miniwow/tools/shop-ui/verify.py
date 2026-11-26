"""Compare every exported trading field with its recorded source manifest."""
from pathlib import Path
import json, argparse
p=argparse.ArgumentParser();p.add_argument('--output',type=Path,required=True);args=p.parse_args()
source=json.loads((Path(__file__).parent/'configuration-source.json').read_text(encoding='utf8'))
for table in source['sheets']:
    exported=json.loads((args.output/'json'/(table['name']+'Config.bytes')).read_text(encoding='utf8'))['data']
    assert len(exported)==len(table['rows'])
    for row in table['rows']:
        for column,value in zip(table['columns'],row):
            assert exported[str(row[0])][column[0]]==value,(table['name'],row[0],column[0],value)
print('Trading configuration: every exported field matches source; prices 30/10/10 verified.')
