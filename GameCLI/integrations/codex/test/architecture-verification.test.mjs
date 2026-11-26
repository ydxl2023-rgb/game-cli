import {test} from 'node:test';
import assert from 'node:assert/strict';
import {analyzeCode} from '../server/verify-code.mjs';
const file=(module,name,content)=>({path:`Assets/Scripts/Systems/${module}/${name}.cs`,content});
const context=[{path:'Assets/Scripts/Systems/RuntimeSystem.cs',content:'public abstract class RuntimeSystem {}'},file('Login','LoginSystem','namespace Product { public class LoginSystem : RuntimeSystem { public void Reset() {} public static void Start() {} } }')];
test('architecture finds fields, generic queries, casts, aliases, static calls and inferred helper returns',async()=>{
 const result=await analyzeCode([...context,{path:'Assets/Scripts/Registry.cs',content:'public static class Registry { public static T Query<T>() => default(T); public static Product.LoginSystem GetLogin() => null; }'},file('Master','MasterSystem',`using L = Product.LoginSystem;
public class MasterSystem : RuntimeSystem {
 private L entry;
 public void Run(object value) {
  Registry.Query<L>();
  ((L)value).Reset();
  Product.LoginSystem.Start();
  var inferred = Registry.GetLogin();
  inferred.Reset();
 }
}`)],'architecture');
 const lines=new Set(result.issues.filter(i=>i.rule==='OXA001').map(i=>i.line));
 for(const line of [3,5,6,7,8,9])assert.ok(lines.has(line),`missing violation line ${line}`);
});
test('architecture allows own module, event contracts, strings and composition',async()=>{
 const result=await analyzeCode([...context,file('Master','MasterSystem',`public class MasterSystem : RuntimeSystem { string sample = "LoginSystem"; void Run(Entered message) {} } public class Entered {}`),file('Login','Entry','public class Entry { Product.LoginSystem owner; }'),{path:'Assets/Scripts/GamePlugin.cs',content:'public class GamePlugin { Product.LoginSystem login; }'}],'architecture');
 assert.deepEqual(result.issues,[]);
});
test('architecture resolves indirect inheritance and flags unchecked build branches',async()=>{
 const result=await analyzeCode([...context,file('Town','TownSystem','public abstract class BaseTown : RuntimeSystem {} public class TownSystem : BaseTown {}'),file('Master','MasterSystem','public class MasterSystem : RuntimeSystem { TownSystem other; }\n#if UNKNOWN\ninvalid source\n#endif')],'architecture');
 assert.ok(result.issues.some(i=>i.rule==='OXA001'));
 assert.ok(result.issues.some(i=>i.rule==='OXA_CONDITIONAL'));
});
