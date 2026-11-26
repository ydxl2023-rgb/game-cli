# 框架、工具与服务端学习记录

路径和声明位置统一见 [源码索引](SOURCE-INDEX.md)。以下结论来自静态源码；没有执行参考工程的导表、部署、服务器或数据库写入。

## Core：生命周期比类名更重要

`com.oathx.core/Runtime/XApp.cs`：应用配置、基础资源环境和插件入口；Update 推进资源与插件。ShutdownRuntime 有关闭保护。Launch 背景显示/隐藏不是 Town 就绪信号。配置中的清除本地实体档案选项具有实际删除效果，不能为测试启动随意打开。

`XEventModule`：Active 先设状态后调用 OnActive；Detive 清除状态并调用 OnDetive。业务失败应按框架入口清理。事件注册/移除有分发期间的暂存处理，但仍需检查异常路径；不应假定所有异常都会自动恢复内部标志。

`XEventDispatcher`：按类型短名称管理模块；Update 处理延后移除。Dispatch 遍历模块，不应在回调中同步修改该集合。基础 Load/Unload 不自动等价于业务 Install/Uninstall，因此 GamePlugin 需要负责装配契约。停用不保证分发器自动替你忽略所有事件，应配对取消订阅。

`XPluginManager`：负责插件集合和事件队列。泛型与 Type 重载的检查/关闭顺序存在差异，例如某些泛型卸载只调用 Uninstall。调用前必须查看实际重载，不能按方法名称推断语义。Post 延后分发不表示不同事件对象自动合并。

`XActionStateMachine` 与 AIStateMachine 是两套接口；ActorShape 采用用户指定的前者，需要明确推进 UpdateMachine。当前 reference 的 PopState 使用 count 作为末尾下标，存在越界风险；ClearOrBlockState 的返回值也需核对。不要因为框架里有方法就省略边界测试，更不要在本轮修 miniwow 配置时顺带修改框架状态机。

## Entity

`com.oathx.entity/Runtime/XEntityManager.cs` 管理实体实例；缺失时 QueryEntity 与 TryGetEntity 的行为不同。初始化必须先于查询，系统关闭后不能留下悬挂实体引用。

`XEntity.cs` 包含实体组件、通知、序列化及 MergeUtility。XEntitySerialize 的路径基于 persistentDataPath 和类型哈希，有主文件与备份恢复路径；版本字段存在不代表已实现每个产品的存档迁移。组件 Awake/Destroy 与实体创建销毁要配对。XNumerical 承担数值层，不应让 VM 接管数值计算。

数据实体与 ActorShape 分离：实体管理 ID、状态、位置和业务结果，Shape 管状态机、动画与可视对象。用场景 GameObject 是否存在决定存档规则，会把表现生命周期误当成业务生命周期。

## Widget 与扩展

`com.oathx.widget/Runtime/UIWidget.cs`：Ref 获取 Metadata 和类型化 VM，接入属性通知、延迟初始化及 Bind；失败清理与 Unref 负责解除绑定。Metadata 与 VM 字段契约必须一致；仅调整字段排序可能破坏生成的字段索引。

`ResourceManager/UIResourceManager.cs`：ResourceConfigure 提供 loader、cacher、sender；按规范化路径合并加载。返回 false 可能表示已有加载进行中而把回调排队，不一定是永久失败；反过来 true 也不等于完成。加载失败路径可能只记录错误，不调用成功回调。因此页面就绪要有明确失败处理，不能永远等待成功回调。

Shutdown 清除 sender/cache 不等同于取消全部在途外部操作；业务页面用代次/所有权判断过期结果。引用计数、缓存与实例 Destroy 是不同层次，不能只做其一。不要新建第二套 UILoader 或手工替代 UISystem 管理的页面。

`Metadata/MetadataSyncFactory.cs` 注册同步适配器；注册必须早于依赖它的 Ref。`com.oathx.widget.extension` 提供同步器、列表、滚动与布局能力，选用前看具体字段类型和释放契约；VList/Fancy 列表要按稳定业务标识处理点击。固定引用用序列化字段；动态列表复用不能省略旧 VM 的解绑。

编辑器生成器位于 widget/Editor 和 WidgetGenerator~，负责绑定元数据/生成桥接代码。修改 VM、Sync 类型或字段顺序后必须通过正式工具重新生成并查看绑定报告，不能手改生成结果来凑编译。

## Bundle、Avatar、RPC 的使用边界

`com.oathx.bundle` 管理资源索引、依赖、下载和加载。此次重点核对 XRes.LoadBundleAsync：找不到已登记 Bundle 时可能只打印错误而不进入回调，调用者须处理协程结束后的失败。Local Editor 能读 AssetDatabase 不代表 Player 能读取同一路径；构建资源索引/Bundle 后才可验证发布路径。

`com.oathx.avatar` 包含角色与 AI/行为相关基础类型。业务 Fight 的空 AI 类不能证明框架行为树/移动已接好。此次只将相关状态机契约纳入实施依据，Avatar 其他算法不能据此视为逐方法验收通过。

`com.oathx.rpc` 包含会话、协议、序列化和传输实现；XNetSession 定义连接、断开、Send/Post 等契约，RPC 特性决定本地/服务端分派方式。Reference 的 Protocols 和服务器代码只证明一条样例链路存在。当前 miniwow 的离线需求不需要自动接入它；未来接入需专门验证超时、重连、认证和响应归属。

## 配置生成和编辑器工具

`tools/e2cs`：命令参数选择输入、输出、语言/格式；生成 C#/JavaScript、JSON 以及 Excel 表字段。Program 会清理部分输出目录后重新导出，不能把“学习工具”理解成允许对 reference 运行导表。readonly 表字段、IConfigData.GetName/GetData 是插件读取契约的来源。配置副本与生成文件不是两份独立手写逻辑。

`config/export.bat` 先删除旧生成数据，再将 C#、bytes、JS 复制到客户端及服务端；相对目录与当前工作目录相关，不能直接套用到产品。`client/LocalPackages/install.bat` 是 SVN gamecli 分支 checkout 清单，不表示复制后的 miniwow 自动成为 SVN 分支。证书 generate.sh 调用 OpenSSL 生成并写入密钥/证书，只阅读脚本，没有运行或读取密钥文件。

`Assets/Editor/GameCLI/GameCliUguiImporter.cs`：读取 Library 请求队列，检查过期请求、Assets 路径、节点类型、单根、循环层级、资源哈希；支持 dry-run，明确预览 action 不会自动生成业务导航。制作工具可建预制，但不负责补齐 System 交互逻辑。当前产品制作以已安装 UnityCLI 的正式命令为准，不能把旧 importer 的节点类型范围当成新 CLI 的完整能力。

同目录 Probe/Repair/Animation 工具分别服务实体契约、Function/Home 展示、Profile/Currency、RedDot、Shop 和场景验收。它们是检查或制作用代码，不应复制到 Runtime。发现修复工具先检查其实际写入范围，不因名称含 Probe 就默认只读。

UI 资源继续使用 AssetDatabase/UnityCLI 保存，保留 .meta GUID；不手写 YAML。代码源码包应包含实际依赖、配置资产及 meta，不能只同步 C# 后宣称模板可独立安装。

## Server：存在样例实现，不是成熟线上后端

源码入口 `server/http/package.json` 的 exports 映射 core/db/player/resmng 等；`server/app/index.js` 加载路由，`server/core/core.js` 建立 HTTPS、session/MongoStore，按请求名调用路由，已登录请求按 pid 加锁。XMaster 可启动多个子进程，这与客户端 MasterSystem 不是同一职责。

`routePlayer.js` 的 ReqLogin 根据 username 查询/注册账号并写 session.pid；不能把这一流程描述成完整凭证认证。ReqSave/ReqLoad 把命名 archive 交 XPlayer 存取。playermgr 使用 TTLCache，过期时尝试保存；异步保存和进程退出可靠性须另行验证。

`order.js` 有订单状态骨架，但存在未导入 db/os、包装对象与 schema 字段使用不一致、调用参数不匹配等问题；不能照搬为支付系统。`routeAdmin.js` 可分发管理命令，研究时不执行。resmng 用 VM 加载生成配置，reload 含 `svn up` 子进程；阅读不授权运行它。

服务端环境配置、证书和凭证不是文档内容，不复制值，不启动线上连接。本次未做网络部署、安全或数据库一致性验收。
