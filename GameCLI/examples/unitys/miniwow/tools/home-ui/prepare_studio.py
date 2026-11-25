"""Prepare a bounded, hashed source and configuration package for the project copy."""
from pathlib import Path
import json,hashlib,sys
ROOT=Path(__file__).resolve().parents[2];sys.path.insert(0,str(ROOT/'research/python-libs'))
from export_config import rows
import openpyxl
OUT=Path(__file__).parent
hash=lambda b:hashlib.sha256(b).hexdigest()
paths={'Assets/Scripts/Systems/RuntimeSystem.cs','Assets/Scripts/Systems/RuntimeSystem.cs.meta'}
for directory in ['Assets/Scripts/Systems/Home','Assets/Scripts/Systems/Town','Assets/Scripts/Systems/Actor','Assets/Scripts/Systems/Common','Assets/Data/Widgets/Home/UIHomeAssets']:
 for p in (ROOT/'client'/directory).rglob('*'):
  if p.is_file() and p.suffix in ['.cs','.meta','.anim','.controller']:paths.add(p.relative_to(ROOT/'client').as_posix())
for path in ['Assets/Scripts/GamePlugin.cs','Assets/Scripts/GamePlugin.cs.meta','Assets/Data/Widgets/Home/UIHome.prefab','Assets/Data/Widgets/Home/UIHome.prefab.meta','Assets/Data/Config/Home.json','Assets/Data/Config/Home.json.meta','Assets/Scenes/Home.unity','Assets/Scenes/Home.unity.meta','Assets/Editor/MiniwowHomeProbe.cs','Assets/Editor/MiniwowTownSetup.cs','Assets/Editor/MiniwowTownProbe.cs','LocalPackages/com.oathx.unitycli/Editor/Commands/WidgetMetadataUnityCLICommand.cs','LocalPackages/com.oathx.unitycli/UnityCLI~/UnityCLI/command/WidgetMetadataCommand.cs','LocalPackages/com.oathx.widget/Editor/WidgetMetadataPrefabGenerator.cs']:
 paths.add(path)
for path in ['Assets/Scripts/Config/client/ActorConfig.cs','Assets/Data/Jsons/Excels/ActorConfig.bytes','Assets/Data/Widgets/Actor/UIActorJoystick.prefab','Assets/Editor/MiniwowPlayerSetup.cs','Assets/Editor/MiniwowPlayerProbe.cs','Assets/Editor/MiniwowOriginalNavigation.cs','Assets/Editor/MiniwowOriginalPlayerProbe.cs','Assets/Scripts/Config/client/SceneConfig.cs','Assets/Scripts/Config/common/Excel.cs','Assets/Data/Jsons/Excels/SceneConfig.bytes']:
 paths.add(path);paths.add(path+'.meta')
paths.add('ProjectSettings/NavMeshAreas.asset')
paths.add('Assets/Data/Scenes/Town/OriginalTownNavigation.asset.meta')
for path in (ROOT/'client/Assets/Data/Widgets/Actor/JoystickAssets').glob('*.meta'):paths.add(path.relative_to(ROOT/'client').as_posix())
for item in json.loads((OUT/'production-resources.json').read_text('utf8')):
 paths.add(item['destination']+'.meta')
for font in ['fangzhenglanting','hanyijingkai']:paths.add(f'Assets/Data/Widgets/Common/Fonts/{font}.ttf.meta')
files=[]
for path in sorted(paths):
 content=(ROOT/'client'/path).read_bytes().decode('utf8');files.append(dict(path=path,content=content,sha256=hash(content.encode('utf8'))))
for path in ['tools/home-ui/production-resources.json','tools/home-ui/materials.json','tools/home-ui/IMPLEMENTATION.md','tools/town/README.md','tools/town/spine-imports.json','tools/home-ui/build_uihome.py','tools/home-ui/export_config.py','tools/home-ui/bind-uihome.ps1','tools/town/export-scene-config.ps1']:
 content=(ROOT/path).read_bytes().decode('utf8');files.append(dict(path=path,content=content,sha256=hash(content.encode('utf8'))))
# Town/Actor is a separate bounded module attached to the existing CampVisible host.
town_files=[f for f in files if any(token in f['path'] for token in ['Systems/Actor/','Systems/Town/','/TownNavigation.asset','/ActorConfig.','/Widgets/Actor/','/MiniwowTown','/MiniwowPlayer','/MiniwowOriginal','OriginalTownNavigation','NavMeshAreas','Assets/Scenes/Home.unity','tools/town/'])]
files=[f for f in files if f not in town_files]
for relative in ['tools/player/resources.json','tools/player/README.md','tools/player/joystick.json','tools/player/materials.json']:
 content=(ROOT/relative).read_text('utf8');town_files.append(dict(path=relative,content=content,sha256=hash(content.encode('utf8'))))
for group in [files,town_files]:
 assert len(group)<=128 and sum(len(f['content'].encode()) for f in group)<4*1024*1024
book=openpyxl.load_workbook(ROOT/'config/Home.xlsx',data_only=True);sha=hash((ROOT/'config/Home.xlsx').read_bytes());tables=[]
for sheet in ['Entries','Profile','propCommonText','Evidence']:
 data=rows(book,sheet);primary='topic' if sheet=='Evidence' else 'id';fields=[dict(name=k,type=('bool' if isinstance(v,bool) else 'long' if isinstance(v,int) else 'string')) for k,v in data[0].items()]
 entries=[{k:('true' if v is True else 'false' if v is False else '' if v is None else str(v)) for k,v in r.items()} for r in data]
 tables.append(dict(name=sheet,purpose='required' if sheet in ['Entries','propCommonText'] else 'example',primaryKey=primary,aliasField=primary,fields=fields,entries=entries,source=dict(fileName='Home.xlsx',sheet=sheet,assetId=sha,sha256=sha),layout=dict(headerRow=1,dataStartRow=2),merge=dict(strategy='byPrimaryKey')))
scene_source=ROOT/'config/propScene.xlsx'
scene_book=openpyxl.load_workbook(scene_source,data_only=True)
scene_rows=list(scene_book['Scene'].values)
scene_fields=[dict(name=scene_rows[1][i],type=scene_rows[2][i]) for i in range(6)]
scene_entries=[{scene_rows[1][i]:str(row[i]) if row[i] is not None else '' for i in range(6)} for row in scene_rows[3:] if row[0]]
scene_hash=hash(scene_source.read_bytes())
tables.append(dict(name='Scene',purpose='required',primaryKey='NumID',aliasField='ID',fields=scene_fields,entries=scene_entries,source=dict(fileName='propScene.xlsx',sheet='Scene',assetId=scene_hash,sha256=scene_hash),layout=dict(headerRow=2,dataStartRow=4),merge=dict(strategy='byPrimaryKey')))
package=dict(schemaVersion=1,tables=tables,dependencies=[],integration=['Run tools/home-ui/export_config.py after editing config/Home.xlsx with UnityCLI excel.','HomeSystem loads Assets/Data/Config/Home.json before opening UIHome; no hardcoded fallback.','Fixed propCommonText labels are baked by build_uihome.py; reconstruct and bind after label or layout changes.'])
actor_source=ROOT/'config/propActor.xlsx'
actor_rows=list(openpyxl.load_workbook(actor_source,data_only=True)['Actor'].values)
actor_hash=hash(actor_source.read_bytes())
actor_table=dict(name='Actor',purpose='required',primaryKey='NumID',aliasField='ID',fields=[dict(name=name,type=actor_rows[2][i]) for i,name in enumerate(actor_rows[1])],entries=[{name:str(row[i]) if row[i] is not None else '' for i,name in enumerate(actor_rows[1])} for row in actor_rows[3:] if row[0]],source=dict(fileName='propActor.xlsx',sheet='Actor',assetId=actor_hash,sha256=actor_hash),layout=dict(headerRow=2,dataStartRow=4),merge=dict(strategy='byPrimaryKey'))
town_config=dict(schemaVersion=1,tables=[actor_table],dependencies=[],integration=['Install together with UIHome; GamePlugin loads Scene and Actor tables before Town.','Run tools/town/export-scene-config.ps1; import selected resources listed in tools/player/resources.json.','Use MiniwowPlayerSetup after the formal UGUI import; navigation and model authoring are native Unity assets.'])
package['tables'].append(actor_table)
package['integration'].extend(town_config['integration'])
(OUT/'studio-package.json').write_text(json.dumps(dict(scripts=dict(schemaVersion=1,files=files),configurations=package,townScripts=dict(schemaVersion=1,files=town_files),townHost='CampVisible'),ensure_ascii=False),'utf8')
print('Prepared',len(files),'source files; bytes',sum(len(f['content'].encode()) for f in files))
