import {test} from 'node:test';
import assert from 'node:assert/strict';
import {mkdtemp,mkdir,writeFile,rm,cp,readFile} from 'node:fs/promises';
import {spawnSync} from 'node:child_process';
import {fileURLToPath} from 'node:url';
import {tmpdir} from 'node:os';
import {join} from 'node:path';
import {analyzeCode,collectCode,collectStyleCode} from '../server/verify-code.mjs';

test('confirmed lifecycle, events, routes, scenes and animation conventions have bounded checks',async()=>{
 const cases=[
  ['event','class A { void Go() { dispatcher.DispatchEvent(new Ready()); } }','OX024',1],
  ['event-ok','class A { void Go() { XPluginUtility.FireEvent(new Ready()); XPluginUtility.PostEvent(new Ready()); } }','OX024',0],
  ['conditional-event','class A { void Go() { dispatcher?.DispatchEvent(new Ready()); } }','OX024',1],
  ['route','class A { bool Go() { return request.route == "MapPreview"; } }','OX025',1],
  ['route-ok','class A { bool Go() { return request.route == HomeEntryRoute.MapPreview; } }','OX025',0],
  ['other-text','class A { bool Go() { return text == "MapPreview"; } }','OX025',0],
  ['lifecycle','class A : RuntimeSystem { bool OnActive() { XCoroutine.Run(Open()); return true; } private bool Install() { return true; } private void Uninstall() {} }','OX026',3],
  ['lifecycle-ok','class A : RuntimeSystem { bool OnActive() { return InstallA(); } private bool InstallA() { XCoroutine.Run(Open()); return true; } private void UninstallA() {} }','OX026',0],
  ['override-ok','class A : RuntimeSystem { public override bool Install() { return base.Install(); } }','OX026',0],
  ['other-coroutine','class A : RuntimeSystem { bool OnActive() { OtherXCoroutine.Run(Open()); return true; } }','OX026',0],
  ['scene','class A { string scene = "Assets/Scenes/Home.unity"; }','OX027',1],
  ['scene-ok','class A { string scene = Excel.propSceneById(Excel.SCENE_HOME).Path; }','OX027',0],
  ['animation','class A { void Go() { state.SetAnimation(0, "idle", true); } }','OX028',1],
  ['animation-ok','class A { void Go() { state.SetAnimation(0, ActorAnimationNames.idle, true); } }','OX028',0],
  ['variables','class A { int x, y; void Go() { int first = 1, second = 2; } }','OX029',2],
  ['variables-ok','class A { int x; int y; void Go() { int first = 1; int second = 2; } }','OX029',0],
  ['generation','class A : RuntimeSystem { int generation; bool requestedActive; }','OX030',2],
  ['generation-other','class Population { int generation; }','OX030',0],
  ['comment','class A { /* dispatcher.DispatchEvent(evt); route == "MapPreview" */ }','OX024',0]
 ];
 const result=await analyzeCode(cases.map(([path,content])=>({path:path+'.cs',content})));
 for(const [path,,rule,count] of cases)
  assert.equal(result.issues.filter(i=>i.file===path+'.cs'&&i.rule===rule).length,count,path);
 for(const rule of ['OX025','OX028','OX030'])
  assert.ok(result.issues.filter(i=>i.rule===rule).every(i=>i.severity==='review'));
});

test('analyzer stdin preserves UTF-8 source containing Chinese literals and escaped quotes',async()=>{
 const content='/// <summary>Unicode transport fixture.</summary>\npublic class UnicodeFixture {\n' +
  '/// <summary>Localized sample.</summary>\nprivate string text = "营地见习生\\\"万\\\"";\n' +
  '// 中文传输\n'.repeat(4000) + '}';
 const result=await analyzeCode([{path:'Assets/Scripts/Systems/Probe/UnicodeFixture.cs',content}]);
 assert.equal(result.statistics.files,1);
 assert.ok(!result.issues.some(issue=>issue.rule==='CS_PARSE'));
 assert.ok(result.issues.some(issue=>issue.rule==='OX016'));
});

test('Roslyn checks declarations without mistaking callback lambdas or strings for code',async()=>{
 const result=await analyzeCode([{path:'Assets/Scripts/Systems/Login/LoginSystem.cs',content:`
/// <summary>Login controller.</summary>
public class LoginSystem
{
    /// <summary>Run a callback.</summary>
    public void Run()
    {
        System.Action callback = () => { };
        string example = "public int m_Fake => 1;";
    }
}
`},{path:'Assets/Scripts/Systems/Login/Bad.cs',content:`
public class Bad
{
    [Binding] private int m_Value;
    public int GetValue() => m_Value;
    public void Save()
    {
        PlayerPrefs.SetInt("login.key", 1);
        return;
    }
}
`},{path:'Assets/Scripts/Systems/Probe/ProbeSystem.cs',content:`
public class ProbeSystem : UIWidget<VMProbe>
{
    private void Hidden() { }
    [field: SerializeField] public int value { get; set; }
    public void Run(string message = "中文")
    {
        UISystem.GetSingleton().OpenWidget<UIProbe>(null);
        UISystem.GetSingleton().CloseWidget<UIProbe>();
    }
#if NEVER_DEFINED
    invalid inactive code
#endif
}
`},{path:'Assets/Scripts/Systems/Probe/Broken.cs',content:'public class Broken {'}]);
 assert.equal(result.issues.filter(i=>i.file.endsWith('LoginSystem.cs')).length,0);
 for(const rule of ['CS_PARSE','OX001','OX002','OX003','OX004','OX005','OX006','OX007','OX009','OX010','OX011','OX012','OX013','OX014','OX016','OX017'])assert.ok(result.issues.some(i=>i.rule===rule),rule);
 assert.equal(result.statistics.files,4);
 assert.ok(result.statistics.commentLineRate>0);
});

test('source scope excludes generated sources and rejects traversal and empty scopes',async()=>{
 const project=await mkdtemp(join(tmpdir(),'gamecli-code-'));
 try{
  const root=join(project,'Assets','Scripts','Systems','Login');await mkdir(root,{recursive:true});
  await writeFile(join(root,'Login.cs'),'public class Login {}');
  await writeFile(join(root,'Generated.g.cs'),'public class Generated {}');
  const result=await collectCode(project,'Login');
  assert.equal(result.files.length,1);assert.equal(result.excluded.length,1);
  await assert.rejects(collectCode(project,'../Login'),/single/);
  await rm(join(root,'Login.cs'));
  await assert.rejects(collectCode(project,'Login'),/No handwritten/);
 }finally{await rm(project,{recursive:true,force:true});}
});

test('multiline initialized locals require separation on both sides, including callback bodies',async()=>{
 const wrap=body=>`class Fixture { void Run() {\n${body}\n} }`;
 const initializer='var configure = new ResourceConfigure(sender) {\n loader = new GameUILoader(),\n cacher = new UIDefaultCacher()\n};';
 const cases=[
  ['before',wrap(`Register();\n${initializer}\n\nStartup(configure);`),1],
  ['after',wrap(`${initializer}\nStartup(configure);`),1],
  ['both',wrap(`Register();\n${initializer}\nStartup(configure);`),2],
  ['correct',wrap(`Register();\n\n${initializer}\n\nStartup(configure);`),0],
  ['comment-not-blank',wrap(`Register();\n// Configuration\n${initializer}`),1],
  ['callback',wrap(`Run(() => {\nRegister();\n${initializer}\n});`),1],
  ['array',wrap('Register();\nSystem.Type[] widgets = new System.Type[] {\n typeof(UILogin)\n};\nClose(widgets);'),2],
  ['compact-declarations',wrap('int first = 1;\nint second = 2;\nif (first == second)\n return;'),0],
  ['single-statement',wrap(initializer),0],
  ['nested-lambda',wrap(`Register();\nSystem.Action callback = () => {\n${initializer}\n};\nRegister();`),0],
 ];
 const report=await analyzeCode(cases.map(([name,content])=>({path:name+'.cs',content})));
 assert.equal(report.policy,'oathx-code/9');
 assert.match(report.policySha256,/^[a-f0-9]{64}$/);
 assert.equal(report.rules.length,31);
 for(const [name,,expected] of cases)assert.equal(report.issues.filter(i=>i.file===name+'.cs'&&i.rule==='OX018').length,expected,name);
});

test('missing, incomplete and unknown policies fail instead of silently skipping checks',async()=>{
 await analyzeCode([{path:'Probe.cs',content:'class Probe {}'}]);
 const folder=await mkdtemp(join(tmpdir(),'gamecli-policy-'));
 try{
  const binaries=fileURLToPath(new URL('../../../clis/code-verify/bin/Release/net8.0/',import.meta.url));
  await cp(binaries,folder,{recursive:true});
  const path=join(folder,'rules.json');
  const policy=JSON.parse(await readFile(path,'utf8'));
  const run=()=>spawnSync('dotnet',[join(folder,'CodeVerify.dll')],{input:JSON.stringify({files:[{path:'Probe.cs',content:'class Probe {}'}]}),encoding:'utf8',windowsHide:true});
  await rm(path);assert.notEqual(run().status,0,'missing policy');
  await writeFile(path,'{');assert.notEqual(run().status,0,'invalid JSON');
  await writeFile(path,JSON.stringify({...policy,rules:policy.rules.slice(1)}));assert.notEqual(run().status,0,'missing rule');
  await writeFile(path,JSON.stringify({...policy,rules:[...policy.rules,{id:'OX999',severity:'error',description:'Unknown'}]}));assert.notEqual(run().status,0,'unknown rule');
  await writeFile(path,JSON.stringify({...policy,initializerSpacing:{before:false,after:true}}));assert.notEqual(run().status,0,'disabled spacing');
  await writeFile(path,JSON.stringify({...policy,maxCodeLineLength:0}));assert.notEqual(run().status,0,'invalid line limit');
  await writeFile(path,JSON.stringify(policy));assert.equal(run().status,0,'valid policy');
 }finally{await rm(folder,{recursive:true,force:true});}
});

test('code width checks boundary, tabs, literals and original compound conditions without counting comments',async()=>{
 const line=width=>' '.repeat(width-4)+'Run;';
 const files=[
  {path:'boundary.cs',content:`class Probe { void Run() {\n${line(120)}\n${line(121)}\n} }`},
  {path:'comments.cs',content:`// ${'x'.repeat(200)}\nclass Probe {\n/// <summary>${'x'.repeat(200)}</summary>\nvoid Run() { Call(); // ${'x'.repeat(200)}\n} }`},
  {path:'literal.cs',content:`class Probe { string text = "${'x'.repeat(140)}"; }`},
  {path:'multiline.cs',content:`class Probe { string text = @"\n${'x'.repeat(200)}\n"; }`},
  {path:'tabs.cs',content:`class Probe { void Run() {\n${'\t'.repeat(30)}Call();\n} }`},
  {path:'condition.cs',content:'class Probe { void Run() {\nif (systems == null || systems.Count == 0 || !systems.Values.Any(row => row.InstallOnStartup != 0) || systems.Values.Any(row => !ValidSystemConfiguration(row)))\nreturn;\n} }'},
  {path:'wrapped.cs',content:'class Probe { void Run() {\nif (systems == null\n    || systems.Count == 0\n    || !systems.Values.Any(row => row.InstallOnStartup != 0)\n    || systems.Values.Any(row => !ValidSystemConfiguration(row)))\nreturn;\n} }'},
 ];
 const report=await analyzeCode(files);
 const issues=report.issues.filter(i=>i.rule==='OX019');
 assert.deepEqual(issues.map(i=>[i.file,i.line]),[['boundary.cs',3],['literal.cs',1],['tabs.cs',2],['condition.cs',2]]);
 assert.ok(issues.every(i=>i.severity==='error'));
});

test('default style scope includes GamePlugin and generated Excel exclusions; system scope retains common files',async()=>{
 const project=await mkdtemp(join(tmpdir(),'gamecli-style-'));
 try{
  for(const relative of ['GamePlugin.cs','Systems/RuntimeSystem.cs','Systems/Login/LoginSystem.cs','Config/SceneConfig.cs']){
   const path=join(project,'Assets','Scripts',relative);
   await mkdir(join(path,'..'),{recursive:true});await writeFile(path,'class Fixture {}');
  }
  const all=await collectStyleCode(project);
  assert.equal(all.files.length,3);
  assert.deepEqual(all.excluded,['Assets/Scripts/Config/SceneConfig.cs']);
  const system=await collectStyleCode(project,'Login');
  assert.equal(system.files.length,3);
  assert.ok(system.files.some(f=>f.path==='Assets/Scripts/GamePlugin.cs'));
 }finally{await rm(project,{recursive:true,force:true});}
});

test('OX020 distinguishes parameter-only throws from other null handling',async()=>{
 const content = `class Fixture {
 void Run(object value) {
  if (value == null) throw new System.ArgumentNullException(nameof(value));
  if (value is null) { throw new ArgumentNullException(); }
  if (value == null) return;
  if (value == null || value.Equals(this)) return;
  object local = value;
  if (local == null) throw new ArgumentNullException();
 }
}`;
 const report=await analyzeCode([{path:'Fixture.cs',content}]);
 assert.deepEqual(report.issues.filter(i=>i.rule==='OX020').map(i=>[i.line,i.severity]),[[3,'review'],[4,'review'],[5,'review'],[6,'review']]);
});

test('OX021 rejects activation mirrors and permits business and callback conditions',async()=>{
 const content=`class Example : RuntimeSystem {
 bool entered;
 bool OnActive() {
  if (!IsActive()) return false;
  if (entered) return true;
  if (!ValidConfiguration()) return false;
  entered = true;
  Open(() => { if (IsActive() && version == expected) Ready(); });
  return true;
 }
 void OnDetive() { entered = false; }
 }`;
 const report=await analyzeCode([{path:'Example.cs',content}]);
 assert.deepEqual(report.issues.filter(i=>i.rule==='OX021').map(i=>i.line),[4,5]);
});

test('OX022 requires named On handlers for event subscriptions',async()=>{
 const content=`class Example { void Run() {
 Subscribe<EnterLocalGame>(OnEnterGame);
 this.Subscribe(this.OnReturnToLogin);
 Subscribe<EnterLocalGame>(message => EnterGame());
 bus.Subscribe((Event message) => { EnterGame(); });
 bus?.Subscribe<Event>(HandleEntry);
 Subscribe<Event>(delegate(Event message) { EnterGame(); });
 Subscribe<Event>(OnenterGame);
 OpenWidget(widget => Show(widget));
 } }`;
 const report=await analyzeCode([{path:'Example.cs',content}]);
 assert.deepEqual(report.issues.filter(i=>i.rule==='OX022').map(i=>i.line),[4,5,6,7,8]);
});

test('OX023 separates entity lookup from use',async()=>{
 const content=`class Example { void Run() {
 XEntityManager.GetSingleton().QueryEntity<MasterEntity>().OpenLogin();
 (manager.QueryEntity<Entity>())?.Stop();
 widget.Invalidate(manager.QueryEntity<Entity>());
 var master = manager.QueryEntity<MasterEntity>();
 master.OpenLogin();
 } }`;
 const report=await analyzeCode([{path:'Example.cs',content}]);
 assert.deepEqual(report.issues.filter(i=>i.rule==='OX023').map(i=>i.line),[2,3,4]);
});
