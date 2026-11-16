// Config packages travel with the immutable template payload, separately from scripts.
export function validateConfigurations(value){
 if(value==null)return null;
 if(value.schemaVersion!==1||!Array.isArray(value.tables)||value.tables.length>64||!Array.isArray(value.dependencies)||!Array.isArray(value.integration))throw Error('配置包结构无效。');
 const names=new Set();
 for(const t of value.tables){
  if(!t||typeof t.name!=='string'||!/^\w+$/.test(t.name)||names.has(t.name))throw Error('配置表名无效或重复。');names.add(t.name);
  if(!Array.isArray(t.fields)||!t.fields.length||!Array.isArray(t.entries)||!['required','example'].includes(t.purpose))throw Error('配置表字段、条目或用途无效。');
  const fields=new Set();for(const f of t.fields){if(!f.name||!f.type||fields.has(f.name))throw Error('配置字段无效或重复。');fields.add(f.name);}
  if(!fields.has(t.primaryKey)||!fields.has(t.aliasField))throw Error('配置主键或别名字段不存在。');
  const keys=new Set(),aliases=new Set();for(const row of t.entries){const k=row[t.primaryKey],a=row[t.aliasField];if(k==null||!a||keys.has(String(k))||aliases.has(a)||Object.keys(row).some(k=>!fields.has(k)))throw Error('配置条目缺少主键、别名或存在冲突。');keys.add(String(k));aliases.add(a);}
  if(!t.source||!/^\w[\w.-]*\.xlsx$/.test(t.source.fileName)||! /^[a-f0-9]{64}$/.test(t.source.sha256)||t.source.assetId!==t.source.sha256||!t.source.sheet)throw Error('配置源表引用无效。');
 }
 if(JSON.stringify(value).length>2*1024*1024)throw Error('结构化配置包过大。');
 return structuredClone(value);
}
