# 任务配置转换与导表

2026-11-26。本文维护配置转换契约。TaskSystem首版实现与边界见 [TaskSystem开发记录](../task-system/README.md)。

## 源表与输出

源文件为 `config/propTask.xlsx`，沿用现有三行表头：说明、字段名、类型；第4行起为数据。四张表的ID列均直接填写原作数字ID，不再生成TASK_*、TASK_CHAPTER_*、TASK_GUIDE_*或TASK_UNLOCK_*别名常量；NumID列暂保留。统一原位迁移命令为 `node tools/task-config/build.mjs --numeric-ids`。所有任务定义都导入，运行时是否派发由之后的实体规则决定，不能把第一行10001当作新手第一步。

|工作表|行数|来源|生成输出|
|---|---:|---|---|
|Task|1397|tbtask|TaskConfig.cs / TaskConfig.bytes|
|TaskChapter|114|tbchaptertask|TaskChapterConfig.cs / TaskChapterConfig.bytes|
|TaskUnlock|6|tbtaskunlock|TaskUnlockConfig.cs / TaskUnlockConfig.bytes|
|TaskGuide|1594|tbguidetask|TaskGuideConfig.cs / TaskGuideConfig.bytes|
|TaskSetting|1|本地产品初始章节设置|TaskSettingConfig.cs / TaskSettingConfig.bytes|

客户端生成类在 `client/Assets/Scripts/Config/client`；数据在 `client/Assets/Data/Jsons/Excels`；语言沿用现有 LanguageConfig。表由生成的 Excel 静态字段和 GamePlugin 原有反射配置读取机制识别，不新增配置加载器。

## 字段映射

- TaskID → 数字ID/NumID；原位迁移命令为 `node tools/task-config/build.mjs --numeric-task-ids`；TaskName → Name；TaskDescription → Description；JumpType → GuideID；Reward → Rewards。其余原有任务字段保留，包括 ShowTips、QuestValue、BuildingType、对话ID、优先级及两个延迟字段。
- FinishConditionType → ETaskConditionType；原数字另存 SourceConditionType。原作的参数Bean转换成 TaskConditionShape、Param1/2/3 和 ConditionItems 列表。空参数与值为0的参数由 Shape 区分，不用JSON字符串掩盖类型。
- ConditionItems 和 Rewards 使用 `List<int:Id,long:Num>`。FinishValue与原作一致为int。
- 原建筑类型保留为 ETaskBuildingType + SourceBuildingType。原解锁TaskType保留为 ETaskCategory + SourceTaskType。
- TaskChapter.ID直接使用数值，原位迁移命令为 `node tools/task-config/build.mjs --numeric-chapter-ids`；不把ID当作数组下标。TaskChapter.TaskIDs 保留原数组顺序，尤其第一章60112～60114必须排在60107之前。
- TaskUnlock.Conditions 保存原条件Type数值和Value，类型对照记录在 manifest.json；TriggerRewards完整保留。此字段仍引用原作条件域，后续实现不能直接转换成未经映射的本项目功能枚举。
- TaskGuide保留组、类型、保存标记、成功/失败后继。ConditionExpression保存执行前条件，ActionExpression保存方法与参数，不再导出ETaskGuideAction或TaskGuideParam表。迁移时全部条件显式填写true，保持原作动作的可达性；不凭空推断角色或任务前置条件。
- 字符串参数使用双引号与转义保留逗号、空白和路径；Wait参数为Number，ShowDialogue与SelectExploreArea为数值ID；ShowMain无参数。Tips改为公共文本ID，不把中文写进表达式。
- 原唯一Tips文案迁入propCommonText.xlsx，别名TEXT_TASK_GUIDE_ASSIST，ID20001。旧参数表语言键694709285随废表移除；文本内容保持一致，其余既有语言键和值全部保留。
- 自动生成枚举的数值由e2cs决定，不能拿它与原作协议数值直接比较。源数字已独立保存；不以自动枚举整数作为持久化协议。

未将 tbguide/tbguidetrigger 或 tbopenstory 混入当前任务引导。也未虚构救火场景交互配置：其原作部分行为来自 GuideStoryController，需后续专门适配。

## 重建与导出

首次生成Excel需已安装的 bundled `@oai/artifact-tool`。在 `client/Library/TaskConfigAuthor/node_modules` 建立指向官方依赖目录的junction（路径由 load_workspace_dependencies 返回），然后在 miniwow 根目录运行：

```powershell
node tools/task-config/build.mjs
pwsh.exe -File tools/town/export-scene-config.ps1
```

build.mjs 从研究JSON重新生成源表，会覆盖 propTask.xlsx，并在公共语言源中维护上述Tips条目；策划开始手工维护后，不应无条件运行重建脚本。正常修改Excel只运行导表脚本。导表脚本统一导出 Scene、Actor、FunctionSystem、CommonText 和任务配置，防止以后单独导场景时遗漏任务语言和枚举。每次先写Library暂存目录，原字段校验和表达式校验全部通过后安装生成文件。

验证命令：

```powershell
python -X utf8 tools/task-config/verify.py --output client/Library/SceneConfigExport/output
```

此验证器用于原作基线转换：逐字段核对全量任务及相关表、列表顺序、枚举映射、语言还原和已有语言条目保留。后续刻意调整产品配置时，需要记录设计差异并更新对应验收基线，不能删除断言冒充原版一致。

## 动作表达式

两列相邻排列，配置示例：

|ConditionExpression|ActionExpression|
|---|---|
|true|Navigate(60004)|
|true|Wait(0.6)|
|true|ShowDialogue(3001)|

条件采用Evaluate模式，只允许Query并必须得到Boolean；动作采用Execute模式。两列必须非空，无条件执行使用true。条件false表示本次不启动动作，不视为动作失败，不应据此走FailureNextID或自动完成任务；何时重新检查由后续业务事件决定，不加入每帧全表轮询。异步动作完成后才由ITaskAction推进后继。

条件是动作执行条件，不替代Task的完成条件、进度、累计计数或领奖规则。任务完成配置本轮保持不变。

例如未来需要 `ATTR(SELF, HP) > 100` 时，由业务注册ATTR查询并注入SELF、HP；表达式库支持该形式，本轮验证器用测试上下文验证动态血量，不代表工程已实现角色属性适配。正式配置使用新查询前，必须补充真实注册契约和导表验证上下文；不能静默提供虚构数值通过导表。当前1594条正式条件均为true。

现有源表的一次性迁移入口为 `node tools/task-config/build.mjs --add-conditions`，在原工作簿内插入条件列，保留其他工作表与动作字段；重复运行不重置已有条件。不带参数运行仍是从原作研究数据重建。

动作列统一使用 `命令名(参数…)`，导表为完整string，由表达式库解析并缓存。无参数写 `ShowMain()`，字符串用双引号，数值不加引号。参数也支持数值表达式。条件列仍使用普通布尔表达式，支持括号、&&、||和短路；不能用||串联返回void的动作。

迁移命令：`node tools/task-config/build.mjs --function-calls`，将旧大括号格式原位恢复为函数调用，保留已有条件及其他工作表。Navigation改名Navigate，Conversation改名ShowDialogue。69条导航的17个目标改用建筑ID，原路径保存在manifest.json的navigationTargets中。映射依据原作从路径提取建筑ID的方法，校验器逐条核对ID及路径。武器店60002、医院60018不可混用。OpenUI和GuideFinger目前保留原作页面标识；示例Hospital不是当前原作OpenUI配置值，不能凭名称擅自替换。

直接在TaskGuide.ActionExpression编辑命令：

```text
Navigate(60004)
Wait(0.6)
OpenUI("ExploreMapPage")
GuideFinger("Main/MainPage/FuncBtn/Button")
ShowDialogue(3001)
SelectExploreArea(2)
ShowMain()
Tips(20001)
```

|注册契约|参数语义|原代码消费依据|
|---|---|---|
|Navigate(number)|建筑整数ID|GuideManager.ParseGuideNavPointNameToBuildingID|
|CloseUI(string)|页面路径|CloseUI / ToStr(0)|
|OpenUI(string)|原页面标识|OpenUI / ToStr(0)|
|Wait(number)|等待秒数|Wait / ToFloat(0)|
|GuideFinger(string)|原UI目标路径|GuideFinger / ToStr(0)|
|ShowMain()|无参数|ShowMain|
|Tips(number)|产品公共文本ID，执行时使用CommonText解析|原PopupTips使用字符串；迁移保留文案|
|SelectExploreArea(number)|探索区域整数ID|SelectExploreArea / ToInt(0)|
|ShowDialogue(number)|对话组整数ID|StartConversation / ToInt(0)|

manifest.json记录注册契约及原数据哈希。ExpressionValidation直接编译当前com.oathx.expression源码，注册仅用于校验的Command回调，遍历全部表达式并与原始动作和参数比较。数值参数按原消费方式解释；路径没有改成猜测的Unity资源ID。未知方法、错误参数数量/类型、语法错误均阻止安装生成文件。

已验证1594条件、1594动作、1523参数，缓存合计321条（1条条件与320条动作）；4类动作错误及5类条件错误样例均被拒绝。动态条件测试中hp=80不执行、hp=150执行，编译计数不增加。详情见 expression-validation.json。参数校验会通过无副作用测试回调检查所有动作，即使条件false也检查动作配置；真实执行的条件门控单独用测试验证。这里不表示Navigate/OpenUI等业务适配已经实现。

本次通过UnityCLI asset delete删除旧TaskGuideParamConfig.cs与.bytes及其meta；今后导表不再生成或复制这些文件。输入Excel包含Task、TaskChapter、TaskUnlock、TaskGuide和新增产品TaskSetting五张表。

## 本地导表工具的必要修正

`tools/e2cs` 来自 development 的同名工具副本，参考工程未修改。仅修正两个文件：

1. ExcelInfo.TryParseEnums：仅单元格包含多个 `|` 分项时采用位标志；跨行累积判断，None固定在首位。原实现以 `splits.Length > 0` 判定，78种条件被当作位移，产生重名值/溢出循环。普通任务枚举现在为顺序值，互不重复。
2. Utility.TryParseCell：补齐原缺失的顶层bool赋值，否则ShowTips与NeedSave全部为false。

试导时还发现顶层long没有读取分支；本产品FinishValue依据原作声明使用int，不为本次任务扩展未使用的顶层long支持。列表内部long原解析器可正常读取。本地工具构建需要.NET8及原工具NuGet依赖，不更换生成协议。config/bin与工具obj已忽略。

## 已验证与已知边界

- 全部原字段、奖励和条件参数反向对比通过；114章的754个任务位置顺序保留。
- 生成枚举值无重复，ShowTips和NeedSave与源值逐条一致；仅废止参数表的提示语言键迁入公共文本，其余旧语言条目未变化。
- UnityCLI compile --wait结束，错误日志0；Unity自动生成新增资产meta。
- GameCLI代码与架构检查通过。生成配置目录被代码风格检查排除；其正确性依据导表、逐字段核对和Unity编译，不能用风格检查代替。
- 配置原始验收不等于玩法验收。TaskSystem本轮运行检查另见开发记录，战斗与场景任务尚未接入。

原始数据存在5处未解析内部引用，原样保留：任务10003的GuideID=3000001；任务10008、10014、10020的GuideID=4000001；引导1028007的NextID=1028008。它们不在已核实的第一章任务导航链中。详见 validation.json；后续派发涉及这些行时必须明确解决，不能静默换成别的目标。

奖励物品、建筑、英雄、对话与FairyGUI路径仍为原作引用。数据转换不意味着这些业务表、UGUI页面或资源已接入Unity。manifest.json保存源文件哈希；validation.json保存生成数据哈希和异常清单。
