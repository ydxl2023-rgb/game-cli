"""Search the local resource catalog without loading its full JSON file."""
from pathlib import Path
import argparse, json, sqlite3

p=argparse.ArgumentParser()
p.add_argument('query',nargs='?',default='')
p.add_argument('--type',default='')
p.add_argument('--limit',type=int,default=20)
args=p.parse_args()
root=Path(__file__).resolve().parents[1]
db=sqlite3.connect(f'file:{(root/"research/resource-catalog/catalog.sqlite").as_posix()}?mode=ro',uri=True)
db.row_factory=sqlite3.Row
query="""SELECT a.id, json_extract(a.payload,'$.name') name,
json_extract(a.payload,'$.type') type,
coalesce(n.system_name,json_extract(a.payload,'$.system_guess')) system_name,
coalesce(n.purpose,json_extract(a.payload,'$.purpose_guess')) purpose,
json_extract(a.payload,'$.confidence') confidence,
json_extract(a.payload,'$.status') extraction_status
FROM assets a LEFT JOIN annotations n ON n.asset_id=a.id
WHERE (?='' OR a.payload LIKE ? OR n.purpose LIKE ? OR n.system_name LIKE ?)
AND (?='' OR json_extract(a.payload,'$.type')=?) LIMIT ?"""
needle='%'+args.query+'%'
rows=db.execute(query,(args.query,needle,needle,needle,args.type,args.type,max(1,min(args.limit,500)))).fetchall()
for row in rows:
    record=dict(row)
    files=[json.loads(r[0]) for r in db.execute("SELECT payload FROM exports WHERE json_extract(payload,'$.asset_id')=?",(row['id'],))]
    record['exports']=[f.get('original_layout_path',f['path']) for f in files]
    print(json.dumps(record,ensure_ascii=True))
db.close()
