# 配置表目录

Miniwow 独立配置源表。沿用 development 的 e2cs 格式，不复制其业务数据。

## 原作任务配置

`propTask.xlsx` 包含 Task、TaskChapter、TaskUnlock、TaskGuide、TaskSetting 五张表。TaskGuide使用相邻的ConditionExpression（布尔执行条件）与ActionExpression（函数调用，例如 `Wait(0.6)`），不再使用独立参数表。无条件动作明确填true，空条件不合法；该条件不等于任务完成条件。全量来源、字段映射、已知引用缺口与验证见 [任务配置导表说明](../tools/task-config/README.md)。统一执行 `pwsh.exe -File tools/town/export-scene-config.ps1`，会导出已有基础表和任务表，并使用expression库验证两列；仅修改配置不代表任务玩法已实现。


## Home 反馈语言表

维护源为 propCommonText.xlsx/CommonText，稳定别名 TEXT_HOME_LOCKED（10001）、TEXT_HOME_INVALID（10002）、TEXT_HOME_PENDING（10003）。修改 Context 后运行 ../tools/town/export-scene-config.ps1；沿用 development/e2cs 提取语言的方式，生成 propLanguage.xlsx 和客户端 CommonTextConfig、LanguageConfig。propLanguage.xlsx 是该导表链生成的语言清单，当前发布内容为中文，不作为手工修改后可长期保留的翻译源。

旧 Home.xlsx/propCommonText 工作表不再保存这三条反馈；不要恢复 home.locked、home.invalid、home.pending 字符串映射。其余静态页面标签仍留在原表，未在本次批量迁移。

## ID 约定

配置表默认使用数字 ID，禁止自行添加 TASK_10104、TASK_GUIDE_1000001 等字符串 ID/别名常量。语言表可使用字符串 ID；其他配置表仅在用户明确要求时使用。数字 ID 是稳定配置键，不是数组下标；引用与顺序按原数据保留。

propTask.xlsx 的五张表均采用数字 ID，重建和导表必须保持此约定。

TaskSetting.InitialChapterID配置TaskSystem首次派发章节；FunctionSystem数字ID10004由Master在登录后加载。接口与验收见 [TaskSystem记录](../tools/task-system/README.md)。


## 开场首段

propOpening.xlsx 包含 HomeOpening（ID 1）与 OpeningDialogue（3042/3043）。数量、消耗、交互坐标、视频和对话组由源表配置，公共文案在 propCommonText.xlsx。统一导表已包含两张表；原 Task/TaskGuide 的业务数据没有为本片段伪造节点。使用方式见 [开场说明](../tools/opening/README.md)。

## 医院交互配置

propHomeBuilding.xlsx 包含医院 60018 的建筑、200 级属性、费用、条件、有序商品和物品引用，另含医院表现参数，共七张表。数字 ID 和语言引用遵循本工程约定。统一导表会逐字段验证并生成对应配置类和 bytes；首级医院 UI 与业务已接入这些关联表，VillageSetting 重复价格和时间字段已移除。51～200 级首项商品的恢复值存在原始资料冲突，标记为 −1，禁止用于业务执行。详见 [医院原作研究与配置契约](../tools/hospital-ui/README.md)。
