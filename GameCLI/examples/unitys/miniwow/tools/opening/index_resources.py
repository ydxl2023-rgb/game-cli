"""Index imported opening assets against the recovered path manifest."""
from pathlib import Path
import hashlib,json
root=Path(__file__).resolve().parents[2]
manifest=json.loads((root/'extracted-resources/path-manifest.json').read_text(encoding='utf-8'))['files']
by_path={item['restored_path']:item for item in manifest}
resources=json.loads((root/'tools/opening/resources.json').read_text(encoding='utf-8'))
for item in json.loads((root/'tools/opening/materials.json').read_text(encoding='utf-8'))['assets']:
    source=(root/'tools/opening'/item['file']).resolve().relative_to(root).as_posix()
    resources.append(dict(source=source,destination=item['path'],use='UI nodes: '+', '.join(item['usedBy'])))
resources.append(dict(source='extracted-resources/assets/data/audios/music/start_game.0.m4a',destination='Assets/Data/Widgets/Home/Opening/start_game.wav',use='Original separate music; ffmpeg PCM conversion, movie has no audio track'))
records={}
for resource in resources:
    source=resource['source'].replace('\\','/')
    if not source.startswith('extracted-resources/'):source='extracted-resources/'+source
    destination=resource['destination'].replace('\\','/').removeprefix('client/')
    evidence=by_path[source]
    records[destination]=dict(id='opening:'+hashlib.sha256(destination.encode()).hexdigest(),sourceAssetId=evidence['asset_id'],name=evidence['name'],unityPath=destination,source=source,sourceSha256=hashlib.sha256((root/source).read_bytes()).hexdigest(),sha256=hashlib.sha256((root/'client'/destination).read_bytes()).hexdigest(),uses=[resource['use']],originalPath=evidence.get('original_path'),pathBasis=evidence['path_basis'])
(root/'tools/opening/unity-imports.json').write_text(json.dumps(list(records.values()),ensure_ascii=False,indent=2),encoding='utf-8')
print('Indexed',len(records),'imports with original asset IDs and source/output hashes.')
