# Development 源码学习与实施依据

本目录记录 `examples/unitys/development` 的源码事实，供 miniwow 和后续 Oathx 工程使用。基线日期：2026-11-25。它不是将整个参考工程批准为产品模板，也不替代用户最新要求。

## 每次开发必须执行

1. 阅读本页及 [共享开发规范](../../development-rules/SKILL.md)。涉及插件、导表、启动时必须读 [配置与启动](CONFIGURATION.md)；涉及业务时读 [系统职责](SYSTEMS.md)；涉及框架调用或工具时读 [框架与工具](FRAMEWORK.md)。
2. 从 [源码索引](SOURCE-INDEX.md) 定位当前实现，阅读将要调用的方法、相应重载和清理路径。文档用于导航和约束，不能代替当前源代码。
3. 在仓库根执行 `python GameCLI/skills/oathx/references/development/index_sources.py`。返回 `stale` 表示源码已变；先审阅列出的变化、更新相关结论，再执行 `--write` 更新基线。禁止只刷新哈希来掩盖文档过期。
4. 在实施说明中记录“参考文件/方法 → 沿用机制 → 产品必要差异”。无明确依据时沿用已有机制；不要自行建立另一套配置发现、系统管理、UI 绑定或资源加载框架。
5. 变更后执行 GameCLI 的代码检查、架构检查及针对变更的编译/运行验收。静态检查通过不等于业务正确，也不能证明已阅读文档。

## 依据优先级

用户最新明确要求 → 项目 AGENTS/共享规范 → 已核对的目标框架契约 → 本文档的源码事实 → 历史示例。development 中存在直接系统引用、公开字段、字符串动画名等历史写法，不构成现行规范例外。

框架与资源机制应沿用；业务数据和产品流程必须由产品配置及用户需求确定。不得引入样例服务器、测试奖励或默认商城价格。miniwow 的 1080×1920 竖屏约定不受参考工程布局影响。

## 阅读导航

用户确认的规则及对应检查能力见 [CONFIRMED-CONVENTIONS](CONFIRMED-CONVENTIONS.md)。它区分共享规范、miniwow 产品约定、历史实现和人工审查项；本页及源码章节不能覆盖用户最新决定。

|任务|先读|需要核对的源码|
|---|---|---|
|GamePlugin、配置表、启动失败|[CONFIGURATION](CONFIGURATION.md)|GamePlugin、XAppConfigure、XRes、生成的 Excel|
|Login、Master、Home 和业务系统|[SYSTEMS](SYSTEMS.md)|对应 Entity/System/UI/VM、RuntimeSystem|
|Entity、状态机、UI、资源、网络|[FRAMEWORK](FRAMEWORK.md)|对应 LocalPackages 方法及全部相关重载|
|导表、预制制作、服务端|[FRAMEWORK](FRAMEWORK.md)|tools/e2cs、config/export.bat、Assets/Editor、server/http|
|判断研究是否覆盖某文件|[COVERAGE](COVERAGE.md)、[SOURCE-INDEX](SOURCE-INDEX.md)|源文件和 source-index.json 中的哈希|

此前 [GamePlugin 比较文档](../../core/references/game-plugin-standard.md) 保留为历史分析；配置读取的当前实施依据以 CONFIGURATION 为准。
