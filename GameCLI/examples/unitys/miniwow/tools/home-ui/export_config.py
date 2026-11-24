"""Export the product Excel source; never supply fallback demonstration values."""
from pathlib import Path
import json,hashlib
import openpyxl
ROOT=Path(__file__).resolve().parents[2]
def rows(book,sheet):
 data=list(book[sheet].values);return [dict(zip(data[0],row)) for row in data[1:] if row[0] is not None]
def export():
 source=ROOT/'config/Home.xlsx';book=openpyxl.load_workbook(source,data_only=True)
 entries=rows(book,'Entries');texts={r['id']:r['text'] for r in rows(book,'propCommonText')}
 assert len({r['id'] for r in entries})==len(entries)
 for e in entries:
  assert e['labelId'] in texts and e['route']
 result=dict(schemaVersion=1,sourceSha256=hashlib.sha256(source.read_bytes()).hexdigest(),entries=entries,profile=rows(book,'Profile')[0],texts=texts)
 out=ROOT/'client/Assets/Data/Config/Home.json';out.parent.mkdir(parents=True,exist_ok=True);out.write_text(json.dumps(result,ensure_ascii=False,indent=2),'utf8')
 return result
if __name__=='__main__':print('Exported',len(export()['entries']),'Home entries')
