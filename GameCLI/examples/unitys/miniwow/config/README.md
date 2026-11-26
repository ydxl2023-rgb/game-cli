# 配置表目录

Miniwow 独立配置源表。沿用 development 的 e2cs 格式，不复制其业务数据。

## 原作任务配置

`propTask.xlsx` 包含 Task、TaskChapter、TaskUnlock、TaskGuide 四张表。TaskGuide使用相邻的ConditionExpression（布尔执行条件）与ActionExpression（函数调用，例如 `Wait(0.6)`），不再使用独立参数表。无条件动作明确填true，空条件不合法；该条件不等于任务完成条件。全量来源、字段映射、已知引用缺口与验证见 [任务配置导表说明](../tools/task-config/README.md)。统一执行 `pwsh.exe -File tools/town/export-scene-config.ps1`，会导出已有基础表和任务表，并使用expression库验证两列；仅修改配置不代表任务玩法已实现。


## Home 反馈语言表

维护源为 propCommonText.xlsx/CommonText，稳定别名 TEXT_HOME_LOCKED（10001）、TEXT_HOME_INVALID（10002）、TEXT_HOME_PENDING（10003）。修改 Context 后运行 ../tools/town/export-scene-config.ps1；沿用 development/e2cs 提取语言的方式，生成 propLanguage.xlsx 和客户端 CommonTextConfig、LanguageConfig。propLanguage.xlsx 是该导表链生成的语言清单，当前发布内容为中文，不作为手工修改后可长期保留的翻译源。

旧 Home.xlsx/propCommonText 工作表不再保存这三条反馈；不要恢复 home.locked、home.invalid、home.pending 字符串映射。其余静态页面标签仍留在原表，未在本次批量迁移。

## ID 约定

配置表默认使用数字 ID，禁止自行添加 TASK_10104、TASK_GUIDE_1000001 等字符串 ID/别名常量。语言表可使用字符串 ID；其他配置表仅在用户明确要求时使用。数字 ID 是稳定配置键，不是数组下标；引用与顺序按原数据保留。

propTask.xlsx 的四张表均已采用数字 ID，重建和导表必须保持此约定。
