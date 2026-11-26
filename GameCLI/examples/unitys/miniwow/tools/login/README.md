# 本地 Login 与 Master 调度

2026-11-25，GameCLI 项目 p-fac9bb57-0a72-4db3-9f48-7d42e95f37a3；Unity 身份 83c29c9f-21c1-4597-9691-6707ca146fa7。

数据库只读核对：MasterSystem v9（纯代码）、SvLogin v11（UIWidget/VM/Entity 页面模式）。本轮按用户已确认的本地进入模式适配，不引入模板的账号、服务器列表、Notice、示例初始货币或章节配置。模板库本身未修改。

## 职责和流程

1. XApp/Launch 初始化；GamePlugin 按 IConfigData 加载完整生成配置，安装启动实体，再按 FunctionSystem 配置装配 MasterSystem 和 LoginSystem。
2. Master 常驻，订阅 LoginSucceed、HomeReady、ReturnToLogin、SystemActive、SystemDetive，初始 Login 由 GamePlugin 按 InstallOnStartup 配置加载。
3. 登录成功保留 Login，Master 请求 Home 激活。HomeSystem.OnActive 启动协程，在协程中通过 RuntimeManager 创建 HomeRuntime；Load 阶段加载 Home 配置、场景与 Actor。Home 主 UI 初始化完成后发送 HomeReady，Master 收到后卸载 Login。TownSystem 已移除。
4. 返回登录卸载业务系统并恢复 Login，Master 实例保持。Home 启动失败只卸载 Home，重置原 Login 实例的重试提示。
5. SystemActive/SystemDetive 使用配置 ID，避免业务系统直接引用具体系统类型。激活按 development 在分发后执行，pending 只记录尚未完成的请求。关闭会取消相应请求。Master 拒绝通过普通业务开关事件关闭，Login 即使启动时加载也允许正常卸载。

GamePlugin 已删除 SystemCompositionRequested 和相关状态机。下列历史记录描述当时的版本，当前机制以本节及项目 Skill 为准。

2026-11-25 解耦复验：verify architecture 扫描 28 个源码文件、4 个业务系统，issues=[]；Master/Login/Home/Town 的 verify code 均通过。会话与玩家回归均通过。第一版分析器不加载框架程序集，曾漏报 Home 的泛型插件调用链，本轮额外人工检查并移除了全部业务侧 GetTown/GetMaster/QuerySystem 链；不能宣称静态检查覆盖所有间接调用。独立复核见 decoupling-review.md。

## UI 与资源

UILogin 位于 Assets/Data/Widgets/Login/UILogin.prefab，1080×1920。2026-11-25 根据用户原作截图修正：背景改为 assets/data/ui/background/003.png，独立 Logo、进入游戏图形、适龄标识、公告图标和底部说明取自 fairygui/Login/images。移除主界面棕色按钮与“本地体验”。003 的存储纹理为 512×2048，展示按原页面逻辑尺寸 720×1560 居中铺满；不能用存储宽高比作展示比例。截图比对排除微信顶部宿主栏。

入口使用原作 Login_button_1 图形直接作为 Button 的 Image，沿用 EnterClick Metadata 和原登录事件，不新增系统或生命周期状态。Logo 与顶部标识靠顶部锚定，入口与说明靠底部锚定。公告和适龄目前只恢复视觉，没有新增公告内容服务或适龄弹窗。底部说明是原图中的参考产品信息，不代表本 DEMO 的出版资质。字体复用 Common/Fonts/fangzhenglanting.ttf。

原资源路径、用途、导入位置与 SHA256 见 resources.json，导入清单 materials.json；原包与已提取文件不修改。restore_design.py 更新布局与资源清单，install.ps1 执行正式 UGUI dry-run、生成、Metadata 与原生绑定，prepare_studio.py 更新本地资源记录和源码包（不执行远程同步）。沿用 development Widget/Metadata 机制，差异仅为产品布局与美术来源。

本次视觉修复验证：正式生成与 Metadata dry-run 均通过；Unity 编译通过；Login verify code 与项目 verify architecture 通过；verify-session.ps1 完整通过，截图 login-ready.png、运行证据 runtime-evidence.json。失败重试用例主动注入无效 Town 配置，因此 Console 中的 “System HomeSystem refused activation” 是该负向用例预期日志。视觉独立复核确认六张资源逐字节一致、主要构图贴合；已修复复核指出的公告文字缺失。未制作公告/适龄子页，也未发布远程模板。

项目启用 USE_OLD_TEXT（Assets/csc.rsp 与原生 PlayerSettings 的当前构建组）。生成器原先直接引用可选 SyncTextMeshPro，已在本项目 LocalPackages 移除该特殊分支，仍由已有 TypeCache 检索可用同步器；没有改动其他工程或服务器标准模板。未使用本地自动导入的 TextMesh Pro 示例资源。

## 复现和验收

- 非 Play 模式运行 install.ps1：asset refresh、compile、UGUI dry-run/正式生成、Metadata dry-run/正式绑定，最后由 MiniwowLoginSetup 原生保存字体和锚点。新环境须先启用 USE_OLD_TEXT；可向 ../town/setup.request 写入 legacy-text，等待 Unity 重编译。
- 从配置 App 场景启动，运行 verify-session.ps1；该脚本要求开始时停留 Login。若已进入 Town，先调用 MiniwowSessionProbe.Return 并等 Login 就绪。
- 再运行 ../player/verify-player.ps1 检查主城与角色回归。
- 本轮通过：4/4 Metadata、Login/Master 代码检查、实际按钮射线点击、重复点击与 Active、内存配置故障回退和重试、真实就绪后交接、返回入口、进入代际取消、Master 停用/重启、1080×1920/2400、原角色与地图回归、Console error=0。
- 原始响应 runtime-evidence.json；独立报告 independent-review.md。login-ready/failed/tall.png 为 Camera/Canvas 原生渲染预览，不是物理触屏或桌面录屏。

新页面在 GameCLI 项目独立 UILogin 节点保存源码包，MasterSystem 为独立纯代码节点；共享启动源码随 UIHome 公共包，保留现有 Town 分包。运行本目录及 ../home-ui/prepare_studio.py 可重新生成源包。未自动提交 Git、推送或发布标准模板。

## 正式预制检查的边界

prefab-verification.json 保存项目版本 23 的正式 verify prefab 原始结果：status=failed，唯一 difference 为 Background.raycastTarget（比较器硬编码只有 Button 为 true；本页面 Image 必须阻挡对底层场景的点击）。保留正确的交互行为，不把背景伪装成 Button，也不修改检查器或报告结果。其余可比较的节点层级、矩形、文字、显隐与按钮属性没有差异；bindings 的静态校验与运行 4/4 Metadata 证据配套使用。

unverified 包含自定义 Widget/Metadata、字体引用、图片资源身份及源码目标映射等检查器未覆盖项。资源文件一致性由 resources.json 的源/目标 SHA256 补充，字体由 InspectFonts 与原生预览补充，具体绑定目标和点击/显隐由 runtime-evidence.json 与独立验收补充。正式预制检查不宣称全项通过；业务运行验收结果另见 independent-review.md。该基线由 login.json 和 MiniwowLoginSetup 的布局规则生成，不从验收导出反向填入期望值。

## GamePlugin 配置化重构（2026-11-25）

参考共享 GamePlugin 标准，将 UI、Entity、System 安装/卸载与配置读取分成独立方法。配置源为 config/propFunctionSystem.xlsx，稳定别名 SYS_MASTER / SYS_LOGIN / SYS_HOME / SYS_TOWN；只有管理层解析 ClassType。Master 发送配置 ID 集合，管理器不读取 MasterPhase、不校验玩家业务配置、不发送 Login 重置或游戏输入事件。Town 在自身激活时校验 Actor 配置。

Load(Type)/Load<T> 与 Unload(Type)/Unload<T> 使用同一生命周期策略；重复加载遵守 Active 配置；安装/激活失败或异常回滚，卸载分别尝试 Detive 与 Uninstall。当时采用配置完整验证后写入的实现（后续已按下节参考机制修复）；关闭使异步代际失效，并按系统、实体、UI 顺序清理。Startup 返回接受启动，不表示异步完成。RuntimeSystem.IsReady 表达真实准备状态。

不照搬 development 的 SDK/RPC、在线认证和已知 Try/命名问题。原参考比较文档保留历史快照，本节记录实际落地。新增 MiniwowSessionProbe.RestartPlugin 检查关闭后模块与实体清空，再验证重新进入。异常日志仍显式输出，不把异常伪装成成功。

重构实测：Unity 编译通过；GamePlugin/RuntimeSystem 与四个业务系统代码检查通过；完整插件 Shutdown/Install/Startup 返回成功，随后会话及玩家回归通过。证据为 plugin-restart.json、runtime-evidence.json、../player/runtime-evidence.json、code-plugin.json；静态独立复核见 plugin-refactor-review.md。异常抛出路径及 Active=0 由源码复核，本轮未做任意第三方模块异常注入。

## 代码规则表复验（2026-11-25）

代码检测改用 oathx-code/2 规则表，默认检查全项目手写 Assets/Scripts，包含 GamePlugin；规则 OX018 拦截多行初始化声明前后的空行缺失。本次仅修正 GamePlugin 与 TownScene 的空行，无运行逻辑变更。code-policy-v2.json 全量检查无 error；UIHome.FormatAmount 的中文单位“万”保留为人工复核项，不算自动通过文案规范。规则测试与架构测试串行执行 9 项通过；并行运行曾发生构建输出文件争用，未将那次失败计为通过。

## 行宽规则复验（2026-11-25）

规则表升级至 oathx-code/3，新增 OX019，maxCodeLineLength=120。本次将 GamePlugin 与 Actor/Home/Town 中的超长条件、参数和调用链换行，保留括号与求值顺序；对比上一版本源码包仅有空白字符变化。code-policy-v3.json 全量检查无 error，既有中文单位人工复核项保留。代码规则与架构检测测试共 10 项通过，覆盖 120/121 列边界、Tab、字符串、注释、原复合条件及换行后的正例。

## 配置来源修复（2026-11-25）

当前按 XAppConfigure.cfgBundle/bundleSource 选择本地目录或 Bundle，按资源名映射 IConfigData 并写入 Excel 字段；通用读取不再硬拼 Excels 路径，也不承担 FunctionSystem 业务校验。逐字段写入不是事务，中途失败不安装系统，成功重试重载全表。cfgBundle 已通过 Unity API 保存为 Assets/Data/Jsons。

必读依据为 ../../../../../skills/oathx/references/development/README.md 及 CONFIGURATION.md。缺表、重复表、缺目录及恢复启动测试见 config-source-evidence.json；代码、架构检查见 code-config-final.json、architecture-config-final.json，会话回归通过。独立复核和验证限制见 config-reference-review.md。Bundle Player 尚未实测。

本地源码包已重新生成，包含配置入口资产；远端同步被自动审批拒绝，当前 GameCLI 远端仍为旧版本，不能以旧 project-sync.json 宣称本次同步完成。

## 生命周期清理

Login 删除 generation、request 和 ready 字段，页面关闭由 UISystem.loadingMap 取消请求，IsReady 查询实际 UILogin 和 VM。LoginEntity/HomeEntity 通过 Initialize 统一启动，业务系统直接 QueryEntity。Home 已初始化状态的业务调用直接访问数据，缺失必需状态会暴露错误。Actor 移动成功分支移除已被上游排除的零输入判断和未使用局部变量。

Home/Town 的场景及资源协程仍保留代次检查，这些等待跨越系统停用与重新激活，SceneManager 的 AsyncOperation 未提供取消。本次未把这些字段当成普通页面加载标志删除。

退出清理已移除 SessionGameplayChanged(false) 和空 SystemCompositionRequested 广播，Master 按配置卸载业务模块。初始登录直接调用 OpenLogin。LocalGamePrepared 和 SessionGameplayChanged 已删除，Town 依据自身就绪和营地可见性控制输入。

2026-11-25 登录交接复验：verify-session.ps1 增加 BeginHomeTransitionCheck/VerifyHomeTransition，逐帧检查 Home 真正就绪前原 Login 始终存在，并断言失败重试保留相同 Login 实例。检查通过；证据 runtime-evidence.json。HomeReady 发出点为 OnHomeOpened 完成刷新之后，避免仅场景加载完毕便关闭 Login 导致 UI 空窗。
