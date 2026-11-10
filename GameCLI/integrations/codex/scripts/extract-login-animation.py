"""Read-only extraction of the inspected Login loop's Euler and scale curves."""
import json,runpy,hashlib
from pathlib import Path
helpers=runpy.run_path('scripts/extract-loading-animation.py');docs=helpers['documents'];keys=helpers['keys']
project=Path('G:/Works/SV/Development/Project');base='Assets/data/UI/NewLogin/@NewLoginView/Effect/GameLoginView/LoginView_Animation_Loop'
clip=docs(project/(base+'.anim'))[0]['AnimationClip'];controller=docs(project/(base+'.controller'));state=next(d['AnimatorState'] for d in controller if 'AnimatorState'in d)
assert not state['m_Transitions'] and not state['m_SpeedParameterActive']
assert not any(clip.get(k) for k in ['m_PositionCurves','m_RotationCurves','m_CompressedRotationCurves','m_FloatCurves','m_PPtrCurves','m_Events'])
source=json.loads(Path('.data/imports/sv-login/unity/unity-prefab.json').read_text(encoding='utf-8'));host=next(n for n in source['nodes'] if n['path']=='0/0/2/0');curves=[]
for field,prop in [('m_EulerCurves','localEulerAnglesRaw'),('m_ScaleCurves','m_LocalScale')]:
 for entry in clip[field]:
  target=host
  for name in entry['path'].split('/'):target=next(n for n in source['nodes'] if n['parent']==target['path'] and n['name']==name)
  for axis in 'xyz':
   curve=entry['curve'];curves.append(dict(targetNodeId='Login'+target['path'].replace('/','_'),sourcePath=entry['path'],component='UnityEngine.Transform',property=prop+'.'+axis,preWrapMode=curve['m_PreInfinity'],postWrapMode=curve['m_PostInfinity'],keys=keys(curve,axis)))
settings=clip['m_AnimationClipSettings']
a=dict(id='LoginLoop',hostNodeId='Login0Animator',source=dict(clip=base+'.anim',sha256=hashlib.sha256((project/(base+'.anim')).read_bytes()).hexdigest()),clip=dict(name=clip['m_Name'],frameRate=clip['m_SampleRate'],startTime=settings['m_StartTime'],stopTime=settings['m_StopTime'],loopTime=bool(settings['m_LoopTime']),wrapMode=clip['m_WrapMode'],curves=curves),state=dict(name=state['m_Name'],default=True,speed=state['m_Speed'],cycleOffset=state['m_CycleOffset'],writeDefaultValues=bool(state['m_WriteDefaultValues'])))
Path('.data/imports/sv-login/animation.json').write_text(json.dumps(a,indent=2),encoding='utf-8');print('Extracted',len(curves),'curves')
