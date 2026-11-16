import {createHash} from 'node:crypto';
export const configurationSchema=`
CREATE TABLE IF NOT EXISTS gamecli_ui.configuration_assets(id text PRIMARY KEY,body bytea NOT NULL,created_at timestamptz NOT NULL DEFAULT now());
ALTER TABLE gamecli_ui.templates ADD COLUMN IF NOT EXISTS configurations jsonb GENERATED ALWAYS AS (payload->'configurations') STORED;
ALTER TABLE gamecli_ui.template_versions ADD COLUMN IF NOT EXISTS configurations jsonb GENERATED ALWAYS AS (payload->'configurations') STORED;
ALTER TABLE gamecli_ui.template_candidates ADD COLUMN IF NOT EXISTS configurations jsonb GENERATED ALWAYS AS (payload->'configurations') STORED;
`;
export async function checkConfigurationAssets(pool,configurations){
 for(const table of configurations?.tables??[]){const row=(await pool.query('SELECT body FROM gamecli_ui.configuration_assets WHERE id=$1',[table.source.assetId])).rows[0];if(!row||createHash('sha256').update(row.body).digest('hex')!==table.source.sha256)throw Error('配置源表缺失或 SHA256 不匹配：'+table.name);}
}
