# PostgreSQL 资源台账

复用 `integrations/codex/server/database.mjs` 的数据库与 SSH 配置，不将凭证复制到 miniwow。

```powershell
node tools/resource-db.mjs --check
node tools/resource-db.mjs --sync research/resource-catalog/catalog.json
```

`--check` 在只读事务中检查连接、schema 版本和数量，不创建表。`--sync` 在单个事务内创建专属 `miniwow_resources` schema 并 upsert 清单，使用 advisory lock 防止两个同步进程竞争。已有 schema 若没有本工具的版本标记则停止。不会修改 `gamecli_ui`，不会删除历史资产，也不会覆盖人工 annotations。

表：`sources` 原始文件；`assets` Unity对象及用途推断；`exports` 每个导出文件；`annotations` 人工确认；`runs` 同步记录；`catalog_meta` schema版本。

主场景还原额外使用 `scene_imports`：保存导入 ID、原 `assets.id` 外键、目标场景和 payload（Unity 路径/哈希、原包来源、使用节点）。同步命令 `node tools/main-scene/sync-imports.mjs`，校验既有 schema 所有权，不修改 annotations，不上传二进制。本地 SQLite 同名表由 `python tools/main-scene/index_imports.py` 维护，生成可检索的 [资源用途索引](main-scene/RESOURCE-INDEX.md)。

```sql
SELECT payload->>'name' AS name, payload->>'unityPath' AS unity_path,
       payload->'uses' AS scene_nodes
FROM miniwow_resources.scene_imports
WHERE payload->>'name' ILIKE '%hotel%';
```

`resource_search` 视图优先显示人工用途，保留自动置信度、原始完整JSON和导出清单。自动分类不代表人工确认。中文关键词可用 ILIKE；英文 token 可用 simple 全文索引。

```sql
SELECT id,type,name,system_name,purpose,extraction_status,review_status,exported_files
FROM miniwow_resources.resource_search
WHERE name ILIKE '%hero%' OR purpose ILIKE '%英雄%';

SELECT type,count(*) FROM miniwow_resources.resource_search GROUP BY type ORDER BY count(*) DESC;

SELECT id,name,source,asset FROM miniwow_resources.resource_search
WHERE extraction_status NOT IN ('exported','copied');
```

人工标注应带依据，在 `annotations` 中使用 asset_id 关联资源，记录 system_name、purpose、status、evidence 与 updated_at；自动同步不会动此表。`sources/assets/exports` 是截至最近同步所见对象的累计集合；单次导出覆盖范围以本地 catalog 和对应 runs.summary 为准。

导出文件保存在本地：数据库记录路径与内容哈希，不上传资源二进制。其他机器查询台账后仍需获取相应本地导出目录。
