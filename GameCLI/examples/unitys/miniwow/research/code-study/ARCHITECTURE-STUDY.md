# 小骑士冲冲冲：客户端架构学习

本篇依据 HotUpdate.dll 静态反编译，证据与哈希见 managed/rea-summary.json。下列源码路径均以 managed/decompiled/HotUpdate 为根，行号指本次导出。没有执行客户端、请求原作服务器，也未修改干净 Unity 工程。这里的“已观察”指代码内容，不表示运行验证。

## 从入口理解系统，而不是先背类名

本程序集找到的入口类叫 StartGame，并非 Startup。`Zombie/StartGame.cs:183 OnStart` 依次初始化计时、焦点事件、屏幕/存储、BootModule、资源状态、UI、配置、视频和 Shader，再初始化场景/音效等，最后进入 Login。`StartGame.cs:42 InitializeUI` 注册组件扩展、页面工厂并预加载 Common、图标和 Loading 包。加载进度是在步骤间写入固定阶段值，并非实际下载字节百分比。

学习点：启动顺序体现依赖关系。配置未就绪时不要让业务开始读表；UI 公共包未就绪时不要打开依赖其组件的页面。可以练习画一张“哪个阶段需要哪个阶段”的图，再解释为什么不能把所有初始化无条件并行。

`Zombie/BootModule.cs:83 Initialize` 设置渠道地址和设备信息；`:139 Connect` 先完成 SDK/HTTP 登录，再从登录结果创建 NetworkClient 并连接。它把“身份认证”和“游戏长连接”区分为两个步骤。具体第三方 SDK 成功条件、服务端真实性和上线调用时序未运行核验。

## 网络传输和业务状态分开读

`Zombie/NetworkClient.cs:79 Initialize` 注册 WebSocket 回调；`:182 OnMessage` 跳过 4 字节头后交给 ProtocolManager 解码。随后按响应类型分流：已有请求上下文的回调、注册的 Proxy 通知、ErrorNotice、普通 Message。`:419 Request<T>` 与 `:348 Context` 配对；Context 用响应 Type 作键，并拒绝重复占用，因此同响应类型的并发请求受到限制。这是可直接从条件分支看到的设计约束，不能把它说成具有任意并发请求编号匹配。

`:275 Update` 使用 3 秒心跳发送门槛和 40 秒心跳超时检查，还对请求的 0.5 秒、7.5 秒等待进行反馈；`:128 Reconnect` 负责重连流程。学习重点是让用户看见请求是否仍在等待，以及明确断线时哪些等待上下文必须被清理，而不是照抄这些秒数。服务端幂等、丢包重发语义以及奖励是否可重复领取不能从客户端单方面证明。

`Zombie/ProxyManager.cs:61 Register<T>` 创建业务 Proxy，注入网络，再调用初始化和消息注册；`:52 OnReconnect` 重新注册并转交各 Proxy 的重连处理；`:82 WaitForProxies` 等待网络响应队列清空。`Zombie/PlayerProxy.cs:84 OnInitialize` 注册多类通知并 RequestData；`:117 RequestData` 是进一步追踪玩家状态更新的入口。Proxy 中有玩家缓存和业务规则，不能把它简单理解为只转发 HTTP 的工具。

## 生命周期也是业务正确性的一部分

`Zombie/Module.cs:13` 为模块创建计时信号并保存消息注册 ID；`:63 OnDispose` 注销消息并停止计时。学习时要同时追踪“谁注册”与“谁释放”：页面关闭后仍收到消息、退出游戏场景后旧计时器仍在执行，往往比一条伤害公式更难排查。当前只读到基类的清理契约，未证明每个派生类和每条退出路径都正确调用。

## UI 管理处理展示秩序，状态不应被页面私藏

`Zombie/UIManager.cs:68` 构造 Screen、Popup、MsgBox、Top 栈；`:247 Initialize` 接收页面创建表；`:280 LoadPackage` 加载 FairyGUI 包；`:315 ShowViewAwait` 是异步开页入口，`:420 CloseView` 处理关闭；`:613 ShowComponent` 与 `:662 CloseComponent` 管理复用组件。它集中处理页面创建、层级、包资源与组件池，页面不必各自重新实现这些行为。

原作采用 FairyGUI，目标 GameCLI 工程使用自身 Widget/VM 规范，两者不能按文件名直接替换。原作一些代码使用全局单例、可变公开字段或运行时查找节点，这是观察到的实现方式，并不自动成为目标框架最佳实践。池化收益、内存峰值及 UI 阻塞情况需要性能测量，不能由“有对象池”推断性能一定好。

## 配置是明确的数据契约

`Zombie/StartGame.cs:19 InitConfig` 读取 ConfigHead.txt 和 Configs.bytes，按头文件的名字、偏移、长度切片为 Luban ByteBuf，再交给 Tables。`Zombie.Table/Tables.cs:363 Tables(Func<string, ByteBuf> loader)` 为各个 Tb 类型提供数据。它把资源加载、二进制分片和强类型表访问分开。

这解释了此前提取出的 ConfigTable 资源如何进入业务。练习时选一条表记录，从 ConfigHead 切片定位，经生成表字段，追到调用方；不要只凭表名解释含义。原作 Excel 源文件、生成器版本及服务端同版配置仍缺失；反编译读取器不等于拿到了策划源表。

## 未来 GameCLI 模板映射建议（不是原作代码）

依据 GameCLI/skills/oathx/development-rules/SKILL.md 与 system-development/SKILL.md，本次只提出改写边界，不落入 client。

|原作职责|建议目标职责|避免直接搬入的部分|
|StartGame 启动阶段|由已选 Launch/Loading 等模板编排资源、配置和进入业务条件|微信 SDK、硬编码页面名、原渠道连接地址|
|Proxy 玩家缓存和规则|业务 Entity 管数据、校验、操作、条件计算；网络适配负责传输|公开可改集合和依赖页面的业务状态|
|模块流程、通知|System 编排流程和通知，Entity 返回操作结果|把数值与库存校验塞进 System/UI|
|FairyGUI 页面和组件|UI 发意图，VM 绑定展示，正式 UnityCLI 生成 UGUI 绑定|手抄 FairyGUI 层级、运行时查找节点|
|ConfigHead/Luban 表|先建立产品 Excel、稳定 ID 与配置包，再接 Entity|临时硬编码样例或直接覆盖既有表|
|请求响应与通知|定义明确业务接口及完成/失败语义；以后选用在线或离线实现|假装登录成功或复用原作生产服务器|

每个系统按 `<System>/System、Entity、VM/、UI/` 的职责组织，公共协议只由一个明确位置定义。后续真正制作模板时还需读取所选模板版本、源码包、配置包及专项 skill，记录输入/输出、存档所有权和依赖；本篇没有进行模板发布或独立验收。

## 给学生的三个阅读练习

1. 从 PlayerProxy.RequestData 选一个响应，记录“请求类型 → 解码 → 状态赋值 → 通知 → 页面刷新”，每一步附文件和方法。遇到未恢复依赖就标未知。
2. 画登录中断时的状态表：尚未认证、已认证未连上、已连上未取完数据、断线重连。分别列出可以执行的按钮操作；对照代码找缺失处理，先作为待验证问题。
3. 选一个页面写职责清单，把显示格式、条件判断、库存修改分别归到 VM、Entity 和 UI 意图，再设计一个只含失败/成功结果的接口。先解释边界，再写自己的代码。

核验限制：C# 来自编译产物，原注释和组织可能丢失；部分外部类型存在反编译提示，可用 managed/il/HotUpdate.il 核对原始 IL。没有获得服务端实现，没有验证原作全部功能可达，也没有证据证明这些客户端类覆盖整个产品。

