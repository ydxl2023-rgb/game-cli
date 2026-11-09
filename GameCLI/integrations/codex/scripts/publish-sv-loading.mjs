import {readFile,mkdir,writeFile} from 'node:fs/promises';
import {join} from 'node:path';
import {createHash} from 'node:crypto';
import {openDatabase} from '../server/database.mjs';
import {validateTemplate} from '../shared/templates.mjs';

const project=process.argv[2];if(!project)throw new Error('Usage: node scripts/publish-sv-loading.mjs <SV project>');
const files={Background:'Assets/data/UI/Public/@Texture/Loading[ASTC6x6]/bg_loading_cylogin.png',Footer:'Assets/data/UI/Public/Atlas/@common/UI_System_Loading_01.png',Icon:'Assets/data/UI/Public/Atlas/@common/UI_System_Loading_02.png',Logo:'Assets/data/UI/Public/Atlas/@common/UI_System_Loading_03.png'};
const assets=[];
for(const [name,path] of Object.entries(files)){const body=await readFile(join(project,path));assets.push({name,path,body,id:createHash('sha256').update(body).digest('hex')});}
const nodes=[];
const add=(name,parent,type,x,y,width,height,color,extra={})=>nodes.push({id:name,name,parent,type,x,y,width,height,color,text:'',fontSize:28,...extra});
add('Loading',null,'Container',0,0,1920,1080,'#171c24');
add('Background','Loading','Image',0,0,1920,1080,'#ffffff',{image:assets[0].id,imageFit:'cover'});
add('Footer','Loading','Container',0,972,1920,1080-972,'#171c24',{image:assets[1].id,imageFit:'100% 100%'});
add('Icon','Footer','Image',24,28,48,48,'#ffffff',{image:assets[2].id});
add('Logo','Footer','Image',88,30,270,44,'#ffffff',{image:assets[3].id,imageTone:'light'});
add('Progress','Footer','Progress',480,20,960,20,'#323841',{progress:.59});
add('Track','Progress','Image',0,0,960,20,'#323841');
add('Fill','Progress','Image',0,0,960,20,'#ff5e00');
add('Percentage','Progress','Text',0,32,960,36,'#ffffff',{text:'Loading... 59%',textAlign:'center'});
add('Tip','Footer','Text',1472,20,416,72,'#ffffff',{text:'正在加载场景，请稍候…',fontSize:24});
const payload={...validateTemplate({root:'Loading',design:{schemaVersion:1,name:'SvLoading',width:1920,height:1080,nodes}}),source:{project,path:'Assets/data/UI/Public/@Prefab/Loading.prefab',mode:'normalized-standard',backgroundSelection:'SV runtime loads a configured background. This template uses bg_loading_cylogin.png as a sample.',tipSelection:'Editable preview sample, not a localized game configuration.'},resources:assets.map(({body,...asset})=>asset),bindings:{Progress:'LoadSceneStateData.progress',Tip:'LoadingViewModel.Tip',Percentage:'LoadingViewModel.LoadNum'},limitations:['Spine/mesh/transition effects are omitted from this reusable standard template.','Preview progress is simulated, not a game scene-loading connection.']};
const db=await openDatabase();const client=await db.pool.connect();
try{
 const current=(await client.query('SELECT * FROM gamecli_ui.templates WHERE id=$1',['SvLoading'])).rows[0];
 if(!current)throw new Error('Existing SvLoading template not found');
 await mkdir(new URL('../.data/imports/sv-loading/',import.meta.url),{recursive:true});
 await writeFile(new URL(`../.data/imports/sv-loading/version-${current.version}.json`,import.meta.url),JSON.stringify(current,null,2));
 await client.query('BEGIN');
 await client.query('CREATE TABLE IF NOT EXISTS gamecli_ui.assets(id text PRIMARY KEY,mime text NOT NULL,body bytea NOT NULL,source_path text NOT NULL,created_at timestamptz NOT NULL DEFAULT now())');
 for(const a of assets)await client.query('INSERT INTO gamecli_ui.assets(id,mime,body,source_path) VALUES($1,$2,$3,$4) ON CONFLICT(id) DO NOTHING',[a.id,'image/png',a.body,a.path]);
 const updated=await client.query('UPDATE gamecli_ui.templates SET name=$1,description=$2,payload=$3::jsonb,version=version+1,updated_at=now() WHERE id=$4 AND version=$5 RETURNING version',['SV Loading','1920 × 1080 · 实图背景 · 可调进度',JSON.stringify(payload),'SvLoading',current.version]);
 if(!updated.rowCount)throw new Error('Template version changed');
 await client.query('COMMIT');
 await writeFile(new URL('../.data/imports/sv-loading/standard.json',import.meta.url),JSON.stringify(payload,null,2));
 console.log(JSON.stringify({version:updated.rows[0].version,nodes:nodes.length,assets:assets.length}));
}catch(e){await client.query('ROLLBACK');throw e;}finally{client.release();await db.close();}
