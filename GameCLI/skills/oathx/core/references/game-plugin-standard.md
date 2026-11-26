# GamePlugin：参考工程学习与后续开发标准
> 历史比较说明：本文早期 miniwow 的 MasterPhase、Town 与组合状态描述是整改前证据，不是当前规范。当前机制见 [配置与启动](../../references/development/CONFIGURATION.md) 和 [确认台账](../../references/development/CONFIRMED-CONVENTIONS.md)。


后续阅读入口：[development 源码学习](../../references/development/README.md)。本文保留原取样时的比较；配置读取已按 [配置与启动](../../references/development/CONFIGURATION.md) 修复，不能把下文的历史 miniwow 实现当作当前机制。

审阅日期：2026-11-25。对象为本机 development 与 miniwow 当前工作区源码，不代表远端模板或某个历史提交的完整状态。主仓库 HEAD 为 `a96447fc5eaaf9a70b58d0ce565d2fa9163d8fef`，工作区存在未提交修改；具体取样文件的 SHA256 见同目录 `game-plugin-sources.json`。

本文依据静态源码分析。没有为这次文档任务修改 C#、启动 Unity、重新运行测试或发布模板。此前 miniwow 的功能回归通过，不能推导出其架构已经成为最佳标准。

## 1. 结论与采用原则

**后续 GamePlugin 的组织方式以 development 的分层安装方法为主要参考；miniwow 当前实现不应整份复制为标准。** development 更清楚地表达了“插件安装基础设施、启动时加载配置并装配实体和系统”的职责，系统数量增加时不需要把每个产品流程塞进插件。

miniwow 的可取之处是独立 UISender、显式的配置有效性检查、退出时使异步请求失效，以及区分“模块已激活”和“场景/UI 真正就绪”。这些应保留为生命周期契约。它的不足是 GamePlugin 仍认识 Login/Town 阶段、角色配置和营地输入，`UpdateSessionRoute` 是产品专用编排，不能包装成通用插件基础设施。

development 也不是逐字复制的规范。用户已明确要求独立 UISender、禁止业务系统互相引用，以及统一编码格式；这些要求优先于参考工程里的历史实现。本文将**观察到的事实**、**已确认的约束**和**后续重构建议**分别记录。

## 2. 阅读入口与实际启动链

主要来源：

- [development GamePlugin.cs](G:/Works/GameCLI/GameCLI/examples/unitys/development/client/Assets/Scripts/GamePlugin.cs)
- [development RuntimeSystem.cs](G:/Works/GameCLI/GameCLI/examples/unitys/development/client/Assets/Scripts/Systems/RuntimeSystem.cs)
- [development XApp.cs](G:/Works/GameCLI/GameCLI/examples/unitys/development/client/LocalPackages/com.oathx.core/Runtime/XApp.cs)
- [development XPluginManager.cs](G:/Works/GameCLI/GameCLI/examples/unitys/development/client/LocalPackages/com.oathx.core/Runtime/XPluginManager.cs)
- [development XEventDispatcher.cs](G:/Works/GameCLI/GameCLI/examples/unitys/development/client/LocalPackages/com.oathx.core/Runtime/XEventDispatcher.cs)
- [miniwow GamePlugin.cs](G:/Works/GameCLI/GameCLI/examples/unitys/miniwow/client/Assets/Scripts/GamePlugin.cs)

实际主链是：XApp 按配置解析插件类型 → `XPluginManager.LoadPlugin(Type)` 构造插件并调用 `Install()` → XApp 调用 `Startup()` → development 加载配置 → 初始化实体 → 按配置安装系统。

每帧由 XApp 驱动插件管理器；管理器先调用各插件 `Update`，再处理 `XPluginUtility` 的待发事件。应用退出使用 `UnloadAllPlugin()`，对各插件依次调用 `Shutdown()` 与 `Uninstall()`。

这里有两个不能混淆的语义：`Startup()` 返回 true 是同步返回，不等于异步配置加载完成；`Active()` 设置 active 并调用 `OnActive()`，也不等于 UI、场景、资源就绪。`RuntimeSystem.LoadSystem` 的回调发生在帧末，不能作为业务准备完成通知。

## 3. development 的 GamePlugin 做了什么

| 入口/方法 | 实际职责与顺序 | 应学习的部分 |
| --- | --- | --- |
| `Install` | `InstallSdkAPI` → `InstallNetRpc` → `InstallUISystem` | 顶层只描述安装步骤，细节下沉 |
| `InstallSdkAPI` | 注册 Product，随后注册依赖它的 Tracking、Activity、Email API | 安装顺序体现真实依赖 |
| `InstallNetRpc` | 准备协议对象，启动 XNetRpc | 网络基础设施集中装配；离线项目不必安装 |
| `InstallUISystem` | 配置 Loader、Cacher、Sender、UI 生命周期转发，注册所用 SyncAction | UI 基础设施集中注册 |
| `Startup` | 请求加载配置；完成回调中 `InstallEntitys` → `InstallSystems` | 先数据依赖，再实体，再系统 |
| `ReadConfigFiles` | 按资源名解析 Config 类型，反序列化 IConfigData，通过 Excel 字段注册数据 | 复用配置数据契约，不逐表复制加载逻辑 |
| `InstallEntitys` | 扫描启动时初始化标记，交给 XEntityManager.Startup | 实体初始化与系统激活分开 |
| `InstallSystems` | 读取 functionSystemConfig，筛选 InstallOnStartup，解析 ClassType，调用 `Load(Type)` | 装配清单来自配置，不散落具体系统调用 |
| `Load(Type)` | 检查功能开放；查表；创建、设置 dispatcher、Install、注册 RPC；按 Active 激活 | 插件是模块生命周期策略入口 |
| `Unload(Type)` | Detive、Uninstall、注销 RPC，再入待删除队列 | 成对释放，集合删除由管理器处理 |
| `Update` | RPC 更新 → Entity 更新 → base.Update 处理模块删除 | 每帧工作集中，顺序要解释依赖 |
| `Shutdown` | UI 关闭 → 系统卸载 → Entity 关闭 | 有明确的运行期清理入口；具体顺序见下方风险 |
| `Uninstall` | 关闭 RPC，清除 SDK API | 对应安装期资源释放 |

UI 适配也在同一文件：独立 `UIResourceLoader` 转发到 XRes；GamePlugin 自己实现 UISender 和 IUIEventSender，将请求和窗口生命周期转发为应用事件。**学习其适配职责，不沿用插件直接实现 UISender 的形式。** 用户已指定使用独立 GameUISender，并允许与 Loader 一起写在 GamePlugin.cs。

## 4. 不能照搬的参考实现细节

以下为源码观察及其推论，不是本轮已经修复的问题：

1. **泛型入口不等价。** GamePlugin 重写 `Load(Type)`、`Unload(Type)`，但基类 `Load<T>()`、`Unload<T>()` 各有自己的实现，并未转发到 Type 重载。若在 development 使用泛型入口，可能跳过功能开放、Install/Uninstall 和 RPC 注册策略。后续统一入口前必须检查两个重载，不能认为调用任意一种都等价。miniwow 当前使用的基础模块契约更简单，也不能因此直接移植这些泛型调用到 development。
2. **Try 命名与行为不符。** `TryGetFunctionSystem` 缺项时抛异常；`TryGetExcelFields` 始终返回 true；`TryLoadAsyncConfiyFiles` 返回值不表达异步最终结果。名称会误导调用者处理失败。
3. **配置加载缺少完整失败契约。** 本地目录不存在时没有完成回调；非 Editor 的 Local 分支不执行目录加载；跳过未识别的表后 ReadConfigFiles 仍可返回 true。反序列化、空结果与必需表完整性也没有统一结果。不能将 true 当作配置全部有效。
4. **启动回调缺少退出失效检查。** 当前 development 的配置完成回调未检查启动代际；退出后回调是否可能重新装配，需要专项验证。miniwow 的 startupGeneration 在这点上更明确。
5. **失败安装和重复激活需要统一语义。** Load(Type) 在 module.Install 返回 false 后仍走到加入 modules；已存在模块会再次 Active。基类 Active 会先置 active，再调用 OnActive。不能把存在、安装成功、激活成功、业务就绪混作一个状态。
6. **卸载顺序须按依赖验证。** development 先关闭 UI 再卸载系统；系统 OnDetive 若还需 UI 服务，这个顺序存在风险。推荐先停业务，再关闭它依赖的 UI 等基础设施，并验证关闭幂等性，不能机械抄原顺序。
7. **拼写和声明位置不是标准。** `Entitys`、`Confiy` 等历史拼写、空 XML 注释、生命周期方法穿插于私有方法之间、嵌套构造格式，均应服从当前共享规范。无条件 `using UnityEditor` 也应检查是否需要编译条件保护；本轮没有做 Player 构建，不能声称已证明具体构建失败。
8. **插件管理器也有重载差异。** 当前 `LoadPlugin(Type)` 未依据 Install 的返回值决定注册；泛型版本有检查。泛型 `UnloadPlugin<T>` 与 Type 版本的 Shutdown 调用也不同。引用框架 API 前必须核对实际调用路径，不能只凭方法名猜测契约。

## 5. 与 miniwow 当前实现对比

| 维度 | development | miniwow 当前版本 | 判断 |
| --- | --- | --- | --- |
| 顶层可读性 | Install/Startup 展示安装步骤 | Install 内直接初始化 UI；StartConfiguredSystems 同时加载、校验、注册表并启动 Master | development 更适合成为组织范例 |
| 配置扩展 | 根据 IConfigData 与 Excel 字段批量注册 | 对 Scene、Actor 分别写加载与校验 | 通用机制参考 development；保留必需数据校验 |
| 系统装配 | functionSystemConfig 驱动启动安装 | 写死 Master、Home、Town、Login 路线 | development 更利于增加系统；具体配置仍须按产品建立 |
| 产品耦合 | 插件主要处理通用基础设施与模块策略 | 插件识别 MasterPhase，检查 Actor 配置，恢复 Login、设置游戏门禁 | miniwow 仍偏重，不宜定为通用标准 |
| UI 适配 | 插件直接实现 UISender | 独立 GameUISender、GameUILoader，同文件组织 | miniwow 符合用户最新约定 |
| 异步失效 | 启动回调缺少明确代际判断 | 具有启动代际、路由版本、实际 ready 判断 | 保留 miniwow 的生命周期保障，避免散落实现 |
| 实体生命周期 | 明确 Startup/Shutdown | 系统按需建实体，插件 Update，但 Shutdown 未统一关闭实体管理器 | miniwow 必须进一步说明实体跨会话保留与插件终止销毁的区别 |
| 模块策略完整性 | 安装、卸载、RPC、功能门禁较完整，但重载不统一 | 沿用较简单的基类，满足当前离线范围 | 应按使用契约选择，不能声称两套等价 |
| 更新顺序 | Entity 更新后再清移除队列 | 先清移除队列，再执行路由、更新 Entity | 没有统一优胜；miniwow 当前顺序服务于跨帧卸载后重装 |
| 业务系统隔离 | Master 中仍有直接 typeof(HomeSystem) 等历史调用 | 新版 Master/Home 已改事件协作 | 以用户最新隔离要求为准，历史直接调用不作为例外 |

**最终评价：development 的职责分解、安装方法命名体系与配置装配模式更好；miniwow 的若干失败/取消防护更严谨，但整体 GamePlugin 仍需向参考组织靠拢。** “15 处违规清零”只说明检查器覆盖的具体类型依赖被移除，不证明产品逻辑放置已最合理，更不授权把任何业务搬到插件里绕过规则。

## 6. 后续统一采用的方法命名契约

框架 override、接口签名保持原样；已有 `Detive` 等框架拼写不能单独在产品侧改名。以下是新建或后续授权重构时采用的名称，不宣称本轮已经重命名。

| 语义 | 建议名称 | 契约 |
| --- | --- | --- |
| 插件生命周期 | `Install` / `Startup` / `Update(float deltaTime)` / `Shutdown` / `Uninstall` | 沿用基类；注释说明同步返回与异步就绪的区别 |
| UI 安装/卸载 | `InstallUISystem` / `UninstallUISystem` | 集中配置本产品的 Loader、Cacher、Sender、Sync |
| SDK 安装/卸载 | `InstallSdkAPI` / `UninstallSdkAPI` | 保留参考命名；只有实际使用时引入 |
| RPC 安装/卸载 | `InstallNetRpc` / `UninstallNetRpc` | 协议注册与注销成对 |
| 实体安装/卸载 | `InstallEntities` / `UninstallEntities` | 修正 Entitys；明确启动实体与销毁范围 |
| 系统安装/卸载 | `InstallSystems` / `UninstallSystems` | 读取已验证的装配配置；不处理单个系统的业务状态 |
| 配置异步加载 | `LoadConfigFilesAsync` | 完成/失败须能观察，不能用同步 bool 冒充异步结果 |
| 协程实现体 | `DoLoadConfigFilesAsync` | 只有确实需要 IEnumerator 时使用；不要求所有方法改协程 |
| 已取得资源的解析 | `ReadConfigFiles` | 返回是否成功，检查必需配置；不同时启动业务系统 |
| 可失败的同步查询 | `TryGetFunctionSystem(..., out ...)` | 普通缺项返回 false，成功填充 out；异常仅用于非预期错误 |
| 必须取得的对象 | `GetRequiredFunctionSystem(...)` | 若约定缺项抛错，用名称明确，不冒充 Try |
| 纯构造/转换 | `GetConfigTypeName` | 无系统激活或数据写入副作用 |
| 事件处理 | `On<Event>`，例如 `OnLoginSucceed` | 接收/校验请求；管理集合的修改进入约定执行边界 |

`QuerySystem<T>`、`GetTown`、`GetMaster` 不是业务系统跨模块调用入口。是否保留这些诊断/管理 API 要明确调用范围；命名好看不能替代职责隔离。`StartConfiguredSystems`、`UpdateSessionRoute` 并非语法错误，但前者职责混合，后者暴露产品流程，都不作为新插件默认方法名。

## 7. 后续标准与实施边界

### 已确认、应立即遵守

- GamePlugin 是插件入口与模块管理策略的装配位置；顶层方法保持步骤清楚，安装细节采用有意义的成对方法。
- 业务 System 不能引用、查询、转换或调用其他具体业务 System。事件携带意图/结果数据；不携带具体系统实例来绕过隔离。
- Entity 负责业务规则与状态，System 负责事件和业务流程；GamePlugin 不应吸收建筑、角色、账号等业务规则。
- GameUISender 与 GameUILoader 独立成类，可同处 GamePlugin.cs；GamePlugin 不直接实现 UISender。ResourceConfigure 先用多行初始化器构造，再传给 Startup。
- 具体系统类型只在明确的装配边界使用。目录在 Systems 外不等于天然获得例外；不能以通过静态检查作为迁移业务逻辑的理由。
- 沿用框架的注册表、事件和生命周期，不另建一套保存业务系统实例的管理字典。

### 下一次重构的建议目标，尚未实施

1. 按 development 的方法分组整理 miniwow：UI 安装、配置加载、实体装配、系统装配各自独立；保留退出失效保护。
2. 为 miniwow 明确装配清单/配置及稳定系统 ID，复用已有配置体系。不能直接导入 development 的所有业务模块、RPC 或 SDK。
3. 将 GamePlugin 的具体营地路线判断进一步缩减为明确的装配策略。Master 仍拥有会话决策；配置/注册位置可知道具体类型，业务事件不携带 Type。采用何种注册或配置形式应对照现有 FunctionSystem 能力设计，不在本文件凭空宣布已实现新管理框架。
4. 先统一 Type/泛型加载卸载语义，再复用 development 的功能门禁与 Install/Uninstall 策略。不能只复制 override 后继续保留旁路调用。
5. 明确应用关闭时实体销毁的所有者；返回 Login 保留本地运行状态与整个插件关闭不是同一件事。

管理层必须保证：事件枚举中不修改模块集合；延迟删除完成后再重新装配同名系统；实际就绪独立于帧末回调；取消使旧结果失效；失败清理部分资源；停止业务后再释放其依赖服务。这些是验收条件，不强制使用某种协程或每帧轮询写法。

## 8. 开发前后检查清单

开发前先读本文件、共享开发规范、目标工程 GamePlugin、实际使用的 XPluginManager/XEventDispatcher 重载及配置表。列明新增能力属于基础设施、模块装配、业务流程还是实体规则，再决定放置位置。

提交评审前检查：

1. Install/Startup 能否只读顶层就理解顺序？方法名称是否真实反映返回值、异步行为和副作用？
2. Load(Type)/Load<T>、Unload(Type)/Unload<T> 是否执行同一策略？功能门禁与生命周期有没有旁路？
3. 是否在插件里新增了角色规则、具体页面重置、关卡流程或任意每帧轮询？如有，重新检查职责。
4. 是否保留用户明确禁止的业务系统直接引用？是否把它伪装成查询、反射或回调？
5. 是否明确失败、重复启动、退出后迟到回调、快速重新进入和插件最终清理？
6. 执行 `verify code`、`verify architecture`，再按实际改动编译和运行验证。静态检查缺少 Unity 类型上下文、存在间接调用/反射等限制时补人工审查，不宣称自动全覆盖。

本文件是后续参考入口，不是要求立即修改所有历史工程。development 保留作为事实来源；任何实际重构、配置迁移或框架 API 调整均按后续明确任务执行。
