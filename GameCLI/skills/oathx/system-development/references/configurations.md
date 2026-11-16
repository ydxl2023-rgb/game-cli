# 系统模板配置包

系统模板是产品开发范式。配置与代码、UI、动画共同交付，不能把“源码能编译”视为“配置已接入”。

## 读取与保存契约

- 模板 `configurations` 为独立 JSON 配置包，schemaVersion=1。它存于发布 payload，同名数据库生成列供查询；候选、正式版本及历史版本共用这一数据源，不单独修改生成列。
- 项目副本将配置包复制至系统根节点 `configurations`，与标准版本分离。普通项目保存不反向发布模板，不自动升级其他产品实例。
- `tables` 包含 name、purpose（required/example）、primaryKey、aliasField、fields、entries、source、layout、merge。fields 保留导表原始类型声明；entries 保留源单元格字符串，类型转换由产品导表器处理。ID 列可为符号别名，不将其误解析为普通整数。
- source 包含 fileName、sheet、assetId、sha256。Excel 原始字节保存在 `gamecli_ui.configuration_assets`，按 SHA256 去重；不要再次塞进 scripts/materials.json。
- 下载源表：`GET /api/studio/templates/<templateId>/configurations/<assetId>?version=<模板版本>`。必须使用读取记录的版本、核对 SHA256；禁止猜测文件路径或把整表覆盖到产品。
- dependencies 列出生成表与外部业务依赖；integration 描述导表与运行时加载要求。null 表示尚未声明，不能据此判定无依赖。

## 独立开发 Agent

1. 先读取配置包及源码查表引用，记录版本、源表哈希、所需符号别名。未声明的引用属于交付缺口，不能靠生成空常量掩盖。
2. 使用目标工程 UnityCLI excel schema/query 检查已有表。缺失条目按 aliasField 与 primaryKey 合并；共享表由集成负责人协调写入。写入先 dry-run，保留目标原有条目和翻译。
3. 相同 ID 不同含义、相同别名不同 ID 或字段类型不兼容时报告具体冲突。数字 ID 不能静默重编号；产品明确要求变更时同步调整引用与配置。
4. required 表示标准实现引用的契约，example 表示参考业务数据。默认保留模板设计；产品需求改变时同步适配，不能自动安装无关系统来满足示例外键。
5. 运行目标工程既有 e2cs/导表流程，生成 Config.Excel、表类及 JSON/bytes。固定文案放 propCommonText，由导表器汇总生成多语言表，保留产品已有翻译。
6. 在使用 UI 前按产品启动流程加载所需表。记录实际导表命令、输出文件和运行时检查；不得把下载 Excel 当成完成安装。

## 独立验收 Agent

各系统 acceptance 负责自己的配置引用检查：所需别名存在、主键不冲突、生成表与源码一致、必要表已加载、多语言能查到文本。验证示例依赖是否已按产品契约适配。开发 Agent 的导表结果不是独立验收结论。

## 发布与维护

配置变更与相关源码一起通过模板草稿 → 候选 → 发布。保存和发布会校验引用的源表字节与 SHA256；历史版本保持不变。

2026-11-16 已迁移 SvLogin v9（propCommonText 的 LOGIN_* 七条），MasterSystem v7（propChapter 的 CHAPTER_HOME）。Master 引用此常量不代表已交付 Home/Level 系统；示例 Levels=10001 的产品含义必须另行适配。

维护入口：`integrations/codex/scripts/migrate-configurations.mjs` 默认只读预检，`--apply` 执行迁移、备份、版本发布及 GameCLIApp 同步。已存在配置包时保留，不覆盖产品修改；常规新配置编辑应走模板草稿，不重复使用首次迁移当通用同步。
