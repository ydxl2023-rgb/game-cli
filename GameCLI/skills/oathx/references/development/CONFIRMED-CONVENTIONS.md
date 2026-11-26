# 已确认约定与检查覆盖台账

整理日期：2026-11-25。范围为本次对话中用户对 miniwow/GameCLI 的工程、资源、代码、系统和交付要求。此表用于追踪落点，不替代具体 Skill、源码或本轮验证报告。

## 阅读及维护

- 写代码前读 [共享开发规范](../../development-rules/SKILL.md)、[源码学习入口](README.md)，按模块读对应 Skill；执行 index_sources.py，再核对实际方法。
- 产品当前结构以 [miniwow 项目 Skill](../../../../examples/unitys/miniwow/skills/miniwow-project/SKILL.md) 为准。用户最新决定优先于参考工程和历史文档。
- 新约束同步“要求 → 适用范围 → Skill → 检测实现/测试或人工检查 → 证据”。不能只往 JSON 加描述就宣称自动检测。
- error 是可确定语法违规；review 是需说明语义的候选项。静态通过不等于编译、行为、视觉或架构全部正确。
- 以下编号是文档追踪号。OX/OXA 是实际检测编号；“人工”表示尚无可靠自动证明，不能以检查通过替代。

## 工程、资料和资源

|编号|用户确认的要求|规范落点与验证|
|---|---|---|
|P01|miniwow 位于指定目录，按 development 建立干净独立 Unity 工程|项目 Skill；工程路径/Packages/ProjectSettings 人工核对；不能把参考工程作为写入目标|
|P02|清理旧 DEMO 代码及已授权旧产物，保留原包和研究资料|项目 Skill；历史清理授权不等于以后可删除任意资料|
|P03|建立版本管理，提交不含 Unity 临时文件|项目 .gitignore、AGENTS；检查暂存清单，Library/Temp/Logs/Obj/构建输出不提交；提交/推送须当次授权|
|P04|不能将普通目录拷贝说成 SVN 分支|项目 Skill；根据真实仓库元数据描述；当前 LocalPackages 为独立快照|
|P05|原作是《小骑士冲冲冲》，学习机制并形成可玩 DEMO|项目 Skill、research；原作证据与产品设计分开，不虚称已获得完整代码或服务端|
|P06|提取资源按原始目录存放，完整记录用途、原始路径、哈希、依赖和证据|资源台账/数据库助手；人工抽验可追溯性；未知用途明确标注，不猜测|
|P07|研究/反编译不得修改干净 Unity 工程|项目范围规则；后续明确开发授权只覆盖指定开发任务|
|P08|GameCLI 项目 miniwow 与本地工程绑定一致|AGENTS、绑定工具；Studio 发起任务先查 verified 绑定及 GameCLIProject.json|
|P09|统一 1080×1920 竖屏，Launch/Login/Home 还原原作布局|项目 Skill；Unity 画面及不同纵横比检查，不把横屏模板等比缩小当适配|
|P10|MainPage 更名 UIHome.prefab，理清预制和资源引用|Home Skill、IMPLEMENTATION；原生 AssetDatabase 保留 GUID，Metadata 和配置路径一致|
|P11|按正式资源目录、CLI 制作资产，不手写 Unity YAML|共享开发规范、unity-cli；导入 dry-run、依赖检查、运行视觉验收|
|P12|C# 工程文件随 Unity 导入更新|框架/工具文档；检查 Unity 编译和 IDE 工程同步，不把 .csproj 当业务源文件手工维护|
|P13|HomeSafeArea、HomeSceneCamera 放 Home/Common；场景组件放 Home/Scene|项目 Skill；这是明确产品目录例外，不推广为所有系统必须设 Scene 子目录|

## 编码、格式和工具

|编号|要求|自动覆盖 / 人工边界|
|---|---|---|
|C01|命名、字段私有、序列化引用、英文 XML 注释、参数及返回说明|OX001/003/005/006；OX003 仅覆盖 SerializeField，不全面检查普通公开字段/UI属性；其他封装与注释含义人工|
|C02|VM/UI 分目录；字段→构造→生命周期→公共→私有方法|OX007/009；VM 的 Binding 独占一行 OX004|
|C03|成员前空行、处理与 return 分组、多行初始化前后空行|OX008/010/018；不同职责的语义空行人工|
|C04|ResourceConfigure 使用局部变量，多项初始化多行排列|OX018 检查多行边界、OX019 检查宽度；是否合理拆分仍人工|
|C05|长条件/参数/链式调用换行，默认最多120列|OX019，rules.json.maxCodeLineLength；续行运算符和求值顺序人工|
|C06|一条声明只定义一个字段/局部变量|新增 OX029；包含 for 声明，拆分时保留作用域|
|C07|禁止表达式体；订阅事件禁用 lambda/匿名委托|OX002、OX022；不能将普通 LINQ/非订阅 lambda 全部判错|
|C08|Subscribe 回调必须 On + PascalCase 方法组|OX022；包括非泛型/限定调用的单参数订阅，其他签名人工|
|C09|QueryEntity 结果先存局部，再操作|OX023 覆盖直接成员/条件访问/内联参数；间接包装仍人工|
|C10|参数为 null 时不要无意义返回、兜底或重复抛 ArgumentNullException|OX020 review；可选值、资源失败、正常生命周期缺失可保留，有具体语义即可|
|C11|必需系统/实体安装失败不能装作成功；加载失败明确异常|配置文档、人工；不能把所有 Try 接口普通未找到也改成异常|
|C12|不重复维护 requestedActive、generation、ready 等系统生命周期状态|OX021 检查重复激活守卫；新增 OX030 提示部分字段；ready 是否有独立业务意义、取消机制是否充分仍人工|
|C13|所有应用事件统一 XPluginUtility|新增 OX024 阻止直接 DispatchEvent；FireEvent 同步、PostEvent 排队，时序及间接绕过人工|
|C14|启动与清理成对封装，避免隐藏父类 Install/Uninstall|新增 OX026 检查直接 RuntimeSystem 子类的 OnActive.Run 和非 override 同名方法；完整成对关系、bool 返回及清理范围人工|
|C15|固定业务目的地使用枚举，不用字符串分派|新增 OX025 review 检查 route 与字符串的 ==/!=；switch、别名、字符串常量及契约传播人工|
|C16|固定反馈进入语言表，用生成ID，不使用 home.locked 等业务字符串|OX016 仅提示中文文本；键是否归属语言表、导表链与英文文案人工|
|C17|动画名集中定义在类之前的常量类型中|新增 OX028 review 检查 SetAnimation/FindAnimation 字面量；字段默认值、其它播放 API、常量位置人工|
|C18|场景路径读取 Scene 配置表，不使用路径常量替代查表|新增 OX027 检查 Assets/...unity 字面量；拼接路径、外部变量和实际查表人工|
|C19|每次规则变更必须更新配置、实现、正反例和覆盖说明|RulePolicy 严格要求 OX001–OX030；缺失、未知、损坏配置失败；测试文件见 RULES.md|
|C20|系统间禁止直接引用/缓存具体系统|verify architecture/OXA001 语义检查；反射、接口中转及未解析项人工，不只跑风格检查|

## 插件、系统和运行时

|编号|要求|规范/依据及检查|
|---|---|---|
|A01|GamePlugin 负责基础设施和装配，不直接继承 UISender|Core、CONFIGURATION；GameUISender 独立类放 GamePlugin.cs，和 Loader 同文件|
|A02|GameUISender 不再注入 dispatcher|最新事件入口规则；Send→FireEvent、Post→PostEvent；保留实际 IEvent 契约|
|A03|配置读取参考 development，不自建复杂反射快照/整表防御链|CONFIGURATION；核对 cfgBundle、IConfigData、Excel 字段映射和实际重载|
|A04|InstallSystems 按 FunctionSystem 启动项装配|参考 GamePlugin.InstallSystems；不擅加排序、Distinct 或整表条件链；源表验收承担完整性检查|
|A05|业务系统继承 RuntimeSystem；Load 后自管初始化及资源，随时可插拔|共享规范、Core；不让 Master 操作业务内部 UI/VM 或 Actor|
|A06|Master 启动常驻，只监听并调度登录成功、返回、激活、关闭事件|Master Skill、项目运行时参考；不能因普通业务退出销毁 Master|
|A07|删除 MasterPhase/TryEnter/CompleteEntry 等平行切换状态及系统组合快照|项目参考；不为无独立状态职责创建 MasterEntity；未来玩家数据另按实际需求设计|
|A08|删除 LocalGamePrepared/SessionGameplayChanged 等无需求全局门禁|项目参考；HomeReady 是用户后续明确要求的加载完成事件，不属于被删除的旧事件|
|A09|Launch→Plugin 读表→Load 启动项 Master/Login；登录成功启动 Home|项目 Skill；InstallOnStartup 不等于常驻，不写死系统实例字段|
|A10|Home 激活启动协程，在其中创建 HomeRuntime，Load 状态加载场景和资源|Home Skill、项目参考；异步安装 true 不表示 Ready|
|A11|Home 场景与 UI 真正就绪后发 HomeReady，Master 再卸载 Login|项目运行时参考；失败重置保留的 Login，返回取消加载；运行回归|
|A12|取消 TownSystem，场景运行不是单独玩法系统|项目 Skill；FunctionSystem 不再注册 Town/HomeRuntime；不是要求所有产品禁用 Town 名称|
|A13|HomeRuntime 为状态机；RuntimeManager 单例按类型管理，未来容纳 FightRuntime|GameRuntime : XActionStateMachine，HomeRuntime : GameRuntime；参考 FightRuntime 继承模式，不保留 IGameRuntime 接口|
|A14|退出删除活动注册、释放输入/Actor/UI，异步场景结束后销毁组件|项目运行时参考；DontDestroyOnLoad 不代表跨登录永久存活，运行检查泄漏|
|A15|TownEntity 合并 HomeEntity；TownScene/Navigation 更名 Home 并归 Scene|项目 Skill；保留 GUID；角色数据仍 ActorEntity；场景释放不重置 Home 配置快照|
|A16|BuildingEntity 作为建筑集合，内部实际建筑数据与表现分离|项目规划；尚未实现，不能标成已完成解锁/升级系统|

## Actor、输入及验证

|编号|要求|规范与验证|
|---|---|---|
|R01|每个 Actor 有 ActorEntity 与 ActorShape，数据/显示分离|Actor Skill；集合实体拥有实例，Entity 不持有 Transform/Spine 等表现对象|
|R02|ActorShape 继承 XActionStateMachine，每角色独立状态|Actor Skill；参考 development/Fight 和用户指定 rts3x；核对真实状态比较语义|
|R03|NPC 本轮恢复原作 Spine，不能仅放占位|项目资源证据；不推广为所有 Actor 必须用 Spine|
|R04|可控角色、固定位置摇杆，图像和导航坚持原作数据|项目 Skill/玩家验收；恢复原导航瓦片，不因卡点重烘焙替代原数据|
|R05|手势按下判断 UI 阻挡，捕获后经过其他 UI 不取消，松手归中|玩家运行验收；失焦/暂停/页面关闭仍释放输入|
|R06|学习和引用文档必须先读，不能只列索引就宣称读完源码|development 阅读流程、SOURCE-INDEX/COVERAGE；源码漂移检查只是辅助|
|R07|运行、编译、静态、视觉和独立审查结果分别陈述|各模块验收；不拿旧报告当本次结果，不以 pending 事件当功能完成|
|R08|未授权不提交/推送/发布模板/改原作服务|项目 AGENTS；本次整理不含发布或游戏新功能开发|

## 有效版本与被取代决定

1. `StartHome` → `Install/Uninstall` → **InstallHome/UninstallHome**；最新名称避免父类冲突。不要恢复中间版本。
2. “HomeSystem.OnActive 启动协程”现在通过 `return InstallHome()` 封装；协程仍由系统拥有，停用由 UninstallHome 取消。
3. TownSystem/TownEntity、IGameRuntime、全局 sessionEnabled、MasterPhase 均为历史方案；保留历史研究不等于继续使用。
4. 字符串快捷入口 → HomeShortcut；配置入口字符串 → HomeEntryRoute，配置边界允许保存可读枚举名称，业务层必须类型化。
5. HomeReady 使用 FireEvent 保持当前同步交接；不能因为统一工具入口就把所有事件机械改为 PostEvent。
6. 无依据的代次字段应删除；真正无法取消的旧异步回调仍须解决，但优先使用已有 Coroutine/Widget 加载取消能力，不能把 generation 当模板必备字段。
7. 配置、原图、导航和程序集事实要以当前文件核对；历史教程中 “Town” 或 “先关闭 Login” 不再是 miniwow 当前标准。

## 自动检查的真实边界

风格分析器仅处理默认激活的 C# 分支和语法，不加载 Unity 程序集。OX026/OX030 仅识别直接 RuntimeSystem 子类；OX025/OX028 为保守候选。OX024 依据 DispatchEvent 方法名，若业务另有同名非应用事件 API，需人工确认并改善识别，不能静默降级整个规则。架构检查可分析源码符号，但不能保证动态/反射完整覆盖。历史文件不会因新规则自动被修复；本次应单独列出工具测试结果和 miniwow 复查结果。
