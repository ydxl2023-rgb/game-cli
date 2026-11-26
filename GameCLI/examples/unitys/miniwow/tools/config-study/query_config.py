"""Read the local original-config research index; does not connect to game services."""
import argparse
import json
import sqlite3
from pathlib import Path

parser=argparse.ArgumentParser()
parser.add_argument('--table',required=True)
parser.add_argument('--id')
parser.add_argument('--search')
parser.add_argument('--limit',type=int,default=10)
args=parser.parse_args()
path=Path(__file__).resolve().parents[2]/'research/config-study/configs.sqlite'
db=sqlite3.connect(path.as_uri()+'?mode=ro',uri=True)
sql='SELECT row_key,label,payload FROM config_rows WHERE table_name=?'
params=[args.table.lower()]
if args.id is not None:sql+=' AND row_key=?';params.append(args.id)
if args.search is not None:sql+=' AND (label LIKE ? OR payload LIKE ?)';params += ['%'+args.search+'%']*2
sql+=' ORDER BY row_key LIMIT ?';params.append(args.limit)
rows=[json.loads(row[2]) for row in db.execute(sql,params)]
print(json.dumps(rows,ensure_ascii=False,indent=2))
