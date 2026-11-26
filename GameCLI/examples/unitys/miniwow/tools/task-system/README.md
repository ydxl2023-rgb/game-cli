# TaskSystem 首版开发记录

日期：2026-11-26。对应 MW-004，以及 MW-008 的执行基础部分。本轮没有提交或推送。

## 已实现与职责

- TaskSystem : RuntimeSystem：具名事件订阅、InstallTasks/UninstallTasks、任务事件编排及引导时钟；不引用其他具体系统。
- TaskCollectionEntity : XEntity：插件启动时创建，管理已派发任务和当前章节；按 TaskChapter.TaskIDs 的原数组顺序查询，待领奖优先于进行中。领取记录跨系统卸载保留，插件关闭时释放。
- TaskEntity : XEntity：单任务实例，保存定义、绝对进度和状态；按任务ID、条件类型和参数匹配，达到 FinishValue 才完成。没有 ITaskCommand。
- ITaskRewardReceiver：完整奖励批次的同步原子提交接口。包含 Rewards 与 QuestValue；失败/异常不得改变经济数据，成功才写领取状态；以任务ID作为幂等键。尚无真实背包接收方。
- ITaskAction：表现生命周期 Start/Tick/Cancel 与 Running/Succeeded/Failed/Cancelled；与任务业务状态分离。
- TaskGuideRunner：条件 Evaluate、动作 Execute，复用 com.oathx.expression 缓存。false条件停留当前节点，显式 Retry 才重新求值；不会自动走失败分支。
- TaskWaitAction：实际执行 Wait 秒数。其他八类命令通过 TaskPresentationRequested 发送类型化操作及完成句柄，消费者在实际完成后调用 Complete。取消后迟到完成无效。

配置 FunctionSystem 新增数字ID 10004，InstallOnStartup=0、Active=1。Master 登录成功后请求 TaskSystem 与 Home；正常返回和失败返回都会卸载 TaskSystem。通过现有装配边界解析配置类型，不从 Home 或其他玩法系统直接调用 TaskSystem。

新增 TaskSetting 表：数字ID=1，InitialChapterID=1。TaskSystem 首次加载才按此设置派发第一章；不是把任务表第一行10001当作新手第一步。手动派发不自动判断 TaskUnlock，下一章通过显式事件请求，尚未加入自动解锁派发策略。

## 对外事件

|输入|意义|
|---|---|
|TaskDispatchRequested|明确派发一个任务，重复派发不会重置|
|TaskChapterRequested|切换到明确章节；当前章节未领完则拒绝|
|TaskProgressReported|可信玩法模块上报某任务的绝对进度、条件类型及参数；不是UI直接点完成|
|TaskClaimRequested|领取已完成任务；结果通过TaskClaimed返回|
|TaskRewardsRegistered|接入或解除奖励接收方|
|TaskGuideRequested|执行某个进行中任务的配置引导|
|TaskGuideCancelled|取消当前引导及其表现|
|TaskGuideRetryRequested|业务变化后重试被条件阻塞的节点|

输出 TaskChanged 通知投影刷新（taskId=0表示整个章节变化），TaskClaimed 返回实际业务结果。
外部表现接收 TaskPresentationRequested 和 TaskPresentationCancelled，负责资源、UI或场景对象的创建与清理。
没有消费者时表现保持等待，不假定成功，不自动完成任务。
表达式注册与变量接口在 TaskGuideRunner，后续新增条件查询需连同业务适配接入，不能为缺失查询提供虚构数值。

## 首版规则与边界

进度是匹配条件的绝对值而不是每次事件增量；重复及旧值不会重复累计或倒退。
Empty/One/Two/ThreeParamCondition按配置精确匹配；ListParamCondition仍需专门业务适配，当前明确抛出不支持，不能宣称78类条件已全部实现。
FinishTalk只是一种条件标识，60001～60003必须由实际救火/救人业务确认，不在播放对话后自动上报。
一次发行的任务不循环重置；重复任务、日常刷新、跨账号隔离另行实现。后续开场实现已接入本地任务存档，见下方更新。
非空奖励没有接收方时返回MissingRewardReceiver，保持待领奖。Claiming状态仅覆盖同步奖励提交期间，阻止接收方重入导致重复发奖；不是系统激活锁或请求代次。
章节可以同时存在多个进行中任务，是否累计由实际玩法事件决定；有待领奖任务时查询优先返回它。派发和顺序是本地首版规则，不冒称原服务器实现。
NeedSave节点检查点、任务存档属于MW-005；任务栏和任务页属于MW-006。导航、手指提示、对话、真实奖励与场景救火尚未接入，本轮不是可玩的完整新手流程。
原始配置5处缺失引用仍保留，进入缺失引导时明确失败，不静默换目标。

## 沿用依据

- development RuntimeSystem 与 miniwow RuntimeSystem：框架负责一次激活；业务只处理自身装卸。
- development MailEntity 的实体规则/收据归属：沿用规则在实体；产品差异是将实际奖励提交抽象为整批接收方，不把领取收据当作已经发奖。
- 当前 HomeEntity：集合持有子XEntity并负责销毁；不将每个TaskEntity注册成全局单例。
- GamePlugin.InstallEntities/InstallSystems、XEntityManager.Startup：沿用Initialize标记与配置装配，不新增加载器。
- 原作 PLAYER-JOURNEY / TaskProxy / UIMainView：参考待领奖优先和章节数组顺序；服务端判定未恢复。
- development 源码漂移检查：559文件current。标准模板工具本次未返回结果；本实现依照用户确认设计、已核对本地参考与框架方法，不声称采用了未读取的数据库Task模板。

## 验证

MiniwowTaskProbe.RunAll 在已加载 App 配置的 Unity Play Mode 执行40项检查：
重复/错误任务进度、条件与英雄参数匹配、进度上限、待领奖优先、章节顺序、重复派发、非法章节切换、
奖励缺失/拒绝/异常/重入/重复领取、缓存复用、条件阻塞、等待节点、取消及迟到完成。
测试使用独立实体及奖励接收器，不写入正式背包或持久存档。

真实插件事件链另验：登录安装10004 → 60001进度 → 领取 → 跟踪60002 → 返回登录卸载 → 不再消费进度事件。
见 rules-evidence.json、install-evidence.json、unload-evidence.json及failed-entry-evidence.json（失败返回清理）。最终规范报告为gamecli-code-final.json、gamecli-architecture-report.json。
Unity编译与Console无错误；正式GameCLI code及architecture均通过。新增唯一人工review为TaskEntity奖励接收方可选判空，具有MissingRewardReceiver业务结果。其余review来自既有代码。
代码检查不等于全部玩法验收；MW-004/008保持in_progress，经济接入和场景表现尚未完成。

复现：进入App场景并激活Game视图（Launch有帧末等待），登录页就绪后调用MiniwowTaskProbe.RunAll；再EnterGame、VerifyInstalled、Return、VerifyUnloaded。测试结束退出Play Mode。


## 开场接入更新（2026-11-26）

首次可玩片段已接入，详见 [开场实现与验收](../opening/README.md)。TaskCollection 现在保存和恢复本地任务进度/领取收据；Home 原任务栏显示真实跟踪任务，60001 的实际灭火结果通过事件上报，显式领取后进入 60002。

这不代表通用 TaskGuide 表现接收器、完整任务页、奖励经济或后续章节已经完成。上文“尚未接入/无存档”描述保留为首版历史边界，当前状态以本节和开场文档为准。
