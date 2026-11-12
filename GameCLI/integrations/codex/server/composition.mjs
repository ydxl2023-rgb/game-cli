
import {compose,checkComposition} from '../shared/composition.mjs';
export function createComposition({connection,access,snapshot,save}){
 async function pool(){return (await connection()).pool;}
 async function apply(id,args){if(args.projectId!==id)throw Error('项目不一致。');await access(id,true);const s=await snapshot(id);if(s.revision!==args.projectVersion)throw Error('项目版本已变化，请重新读取。');const p=await pool();if(!Array.isArray(args.operations))throw Error('需要 operations 数组。');const ids=[...new Set(args.operations.filter(o=>o.type==='add').map(o=>o.templateId))];const templates=ids.length?(await p.query('SELECT template_id AS id,version,category_id AS category,payload,scripts FROM gamecli_ui.template_versions WHERE template_id=ANY($1::text[])',[ids])).rows:[];const result=compose(s.design,args.operations,templates);const saved=await save(id,{projectId:id,revision:s.revision,projectVersion:s.revision,design:result.design});return {...saved,added:result.added,check:checkComposition(result.design)};}
 return {apply};
}
