import {test} from 'node:test';
import assert from 'node:assert/strict';
import {mkdtemp,mkdir,writeFile,rm} from 'node:fs/promises';
import {tmpdir} from 'node:os';
import {join} from 'node:path';
import {analyzeCode,collectCode} from '../server/verify-code.mjs';

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
