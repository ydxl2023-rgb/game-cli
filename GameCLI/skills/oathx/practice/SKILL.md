---
name: oathx-practice
description: 从 development 创建干净的 GameCLI Unity 实践工程，复用 development 的 Oathx 分支包，移除全部业务系统，仅保留基础启动入口与 CLI 测试能力。用于初始化或核验 examples/unitys/practice，不用于添加业务模板或清理 development。
---

# GameCLI 干净实践工程

目标是在 `examples/unitys/practice/client` 建立可编译、可进入 Play Mode、可由 GameCLI 操作的干净 Unity 工程。它是后续验证模板安装和生成流程的基线，不是 development 的业务演示副本。

必须遵守 [开发规范](../development-rules/SKILL.md)，并使用 [基础环境安装](../framework/SKILL.md) 的包依赖、CLI、生成器与基础入口流程。本任务覆盖其“默认添加最小 RuntimeSystem 和演示页面”的步骤：干净基线不安装任何业务系统或演示页面。需要测试生成能力时创建临时探针，验证后移除。

## 路径与执行边界

- 从本目录向上三级定位 GameCLI 根目录，不硬编码本机盘符。
- 源工程：`examples/unitys/development/client`；源配置、服务端和工具在 development 的同级目录中。
- 目标工程：`examples/unitys/practice/client`；`practice/README.md` 已存在，不代表 Unity 工程已初始化。
- 只在目标目录修改或删除资源。保留 development 全部业务、未提交改动、包分支与配置；不得对源工程执行清理、切换或回退。
- Windows 使用 `pwsh.exe`。删除、移动前解析绝对路径，验证目标严格位于 practice 内；检查 junction/symlink 的实际指向，禁止通过链接清理 development。
- 已有实践工程先检查状态并增量处理，不覆盖用户新增代码和测试成果。用户只要求编写 SKILL 时只交付规范，不自动执行安装。

## 1. 确定基线与来源

1. 读取 development 的 ProjectVersion、manifest、packages-lock、入口场景、XAppConfigure、GamePlugin，以及实际 XApp/XPlugin/UISystem 源码。
2. 区分外层 GameCLI Git、原工程 SVN 和 LocalPackages 内独立 SVN 工作副本。用 `svn info`、`svn status` 和 Git 状态核实实际来源，不能把外层 Git main 当成原工程分支。
3. 本次核实的原工程 URL 是 `svn://gitee.com/oathxv/u3dx/developments/v0`；七个运行时包均在各自仓库的 `branches/gamecli`。这些是定位参考，执行时重新读取 URL/revision/local changes，不直接假定 HEAD 等于 development。
4. 默认以当前 development 工作树为基线，在 practice 创建独立副本，保留必要资源 GUID；不复制 Library、Temp、Logs、obj、UserSettings、IDE 缓存、生成 csproj/sln、构建产物或 `.svn/.git` 元数据。必需的生成器和 CLI 工具按安装规范重新构建/安装。
5. 若采用原始分支 checkout 或派生分支，先确认其 URL 和 revision，并在目标创建独立工作副本；不对 development 执行 switch。远端原始分支可能缺少本地 GameCLI 入口修复，必须逐项补齐后再验收。远端分支创建不是默认步骤，已有本地副本路径足以完成任务时无需创建远端分支。
6. 记录基线来源、版本、包来源以及保留的本地差异。普通模板安装/数据库同步配置仍指向 development；测试时显式传 practice 路径，不能全局替换生产配置。

## 2. 初始化 LocalPackages

必须使用 development 内的分支包，不能退回 trunk/main 或下载另一套“最新版”。七个包为 avatar、bundle、core、entity、rpc、widget、widget.extension，完整安装依赖见 [包安装规范](../framework/references/packages.md)。

- 对每个源包记录独立仓库 URL、revision、混合 revision 情况及本地修改，实际 package.json/asmdef 为准。
- 默认将源包有效内容独立复制到目标 `LocalPackages/<package>`，排除 VCS 元数据和构建缓存，包含 development 当前实际使用的修复及必要未版本化文件。逐包校验文件清单/哈希；不能只复制远端旧版本而漏掉 TryGetWidget、批量 CloseWidget 等本地能力。
- 如需独立 SVN 工作副本，在目标 checkout 相同分支和版本，再保留并应用源包本地差异（包括新增/删除文件）；混合 revision 必须复现或明确采用工作树快照。网络不可用时可使用本地工作树快照，并如实记录其并非独立 checkout。
- 不复制源 `.svn` 到实践包，不用 junction/symlink 共享可写包，不让 practice 的 manifest 指回 development/LocalPackages。实践修改不得改变 development 包。
- 目标 manifest 使用 `file:../LocalPackages/com.oathx.<name>`；其他依赖按实际程序集需求安装。保留 UGUI、必要渲染配置、DOTween/LitJson 等框架依赖，不能为“干净”删除到无法编译。
- `com.unity.ai.navigation@1.1.6` 是 Unity 包，不套用 Oathx 分支规则；仅在保留依赖需要时安装。
- `com.oathx.unitycli` 的来源从 development manifest 实际解析。本次 development 使用外部本地桥接包，不能虚构同名 Gitee 分支；安装目标可访问的同版本桥接，记录来源。
- WidgetGenerator 按基础安装规范构建并验证，检查 AfterTargets 输出路径，禁止构建时覆盖源工程资源。

## 3. 移除全部业务，仅保留基础入口

先列出目标资源的保留清单和删除清单，再执行清理。不要仅删除几个 System.cs 文件：清理依赖闭包，同时保留框架类型与工具链。

| 保留 | 移除 |
| --- | --- |
| XApp、XLauncher 及必要启动资源 | Login、Master、Home、Notice、Loading、ServerSelect 及其他全部业务系统 |
| 最小插件、UI loader、Canvas/UISystem、EventSystem、必要相机和层级 | 业务 Entity、VM、UIWidget、事件、RPC 协议注册与游戏 SDK 接口 |
| LocalPackages、CLI Editor 桥接、WidgetGenerator、必要插件依赖 | 业务预制体、美术、演示场景、旧业务编辑器工具与失效序列化引用 |
| 必要 ProjectSettings、包配置、最小 XAppConfigure | 自动安装系统配置、业务配置数据与生成代码、登录和网络启动流程 |

- `Assets/Scripts/Systems` 中不保留任何业务系统。框架包中的 UISystem、实体管理器和运行时基类属于基础设施，不能删除；XLauncher 属于入口组件，不是 Login/Loading 业务页面。
- 当前 GamePlugin 包含 SDK、RPC、配置表自动加载、Entity 扫描和 FunctionSystem 自动安装，不能原样保留。替换为遵循 XPlugin 当前契约的最小插件，仅初始化实际需要的 UI/资源基础设施并对称清理；不加载任何 RuntimeSystem。
- 清理 GamePlugin 对 Excel、FunctionSystem、Master/Login 等类型的编译依赖，以及 XAppConfigure 中失效插件名称、自动安装/热更/远程版本配置。基础入口不依赖服务端连接或业务配置表才能就绪。
- 保留必要的 XApp 启动场景和最简 Launch；不将旧登录背景/服务器列表等业务界面作为基线。启动结束不能无限显示下载、加载进度；使用当前入口提供的完成/隐藏能力达到空闲状态。
- 所有保留 UI 资产仍放在 `Assets/Data/Widgets/`，引用使用序列化字段。入口场景保留一个有效 XApp，禁止缺失脚本、重复 EventSystem、重复启动插件。
- 资产增删与场景引用修复通过 UnityCLI 或受控 Unity Editor/AssetDatabase API，保留成对 .meta；禁止手改 prefab/scene YAML。大规模移除业务代码前确认 CLI 桥接不依赖这些程序集，必要时先准备独立 Editor 清理入口，避免编译失败后工具不可用。
- 源配置、服务端目录不默认复制到 practice。只在 CLI Excel/导表能力确需时建立独立测试夹具；不要带入生产凭证、服务地址或已有玩家存档。

## 4. 验收干净基线

1. 用目标 ProjectVersion 对应的 Unity Editor 打开 practice/client；记录实际目标路径，确认桥接连接的不是 development。
2. 验证包来源与源包一致，目标包无指回 development 的可写链接；检查目标脚本/资源未引用旧业务类型。
3. 执行 CLI ping、compile --wait、console error，确认 Package Manager 完成解析、WidgetGenerator 正常且无编译错误。
4. 进入 Play Mode：XApp 和最小插件就绪，不自动加载业务系统，不发起登录/版本/下载等业务网络请求，无 Missing Script/空引用，退出后能再次进入。
5. 用 CLI 执行只读层级检查和截图，证明目标工程可操作。需要检查 VM/UGUI 生成时在临时测试目录创建最小 Widget/VM，不新增业务 System；测试后用工具移除探针并重新确认基线干净。
6. 对照安装前记录，确认 development 未被修改。编译成功不等于运行成功，CLI ping 成功不等于业务依赖已清空；分别记录实际证据。
7. 更新 practice/README，记录目标路径、Unity 版本、源工程版本、各包分支/revision/本地差异、保留入口和验收结果。交付可复用的干净基线，不自动安装 Login 等标准模板、不自动同步数据库或提交推送。

后续 GameCLI 功能测试应在此基线上按需安装系统模板；测试结果与干净基线分开保存，不能把已经安装业务模板的工程继续称作干净基线。
