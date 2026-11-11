---
name: launch
description: 在 GameCLI 项目中实现或维护 Launch 启动系统，拉取项目编辑好的 UI、制作 Unity UGUI 预制体并编写和验证运行时代码。供负责 Launch 的独立 agent 使用。
---

# Launch 系统

负责从项目 UI 到可运行 Launch 系统的完整交付。此目录是 Launch 系统的独立 skill；后续系统使用 `skills/oathx/system/<系统名>/SKILL.md`，不要把其他系统业务加入此 skill。独立 agent 可以直接读取本文件执行任务，不依赖父 agent 的聊天记忆；本文件不负责自动创建或调度 agent。

## 必须遵守的开发规范

开始工作前必须读取并遵守 [GameCLI 开发规范](../development-rules/SKILL.md)，完成后按其清单检查。不得恢复本次迭代已被用户否定的单例、公开 UI 属性、`m_` 命名、自动属性序列化或路径搜索绑定。

## Launch 已确认的架构

- `XLauncher` 是继承 `UIBehaviour` 的普通 UGUI 组件，不使用单例，不抽象 `ILaunch`，不复用 `XBootstartup`，不负责全局查找、自动实例化或资源加载。
- `XApp` 通过 `[SerializeField] private XLauncher launcher;` 引用场景组件并控制启动流程。其他系统通过 `XApp.HideLaunch()` 等操作入口协作，不公开 launcher 字段或属性。
- 以 `examples/unitys/development/client/Assets/Data/Widgets/Launch/Launch.prefab` 的当前内容实现功能。已确认存在 Loading、Update 两个区域；下载条 Fill 为 RawImage，Slider 节点未挂 Slider 组件；不存在版本文本。资源变化后重新检查，不凭这些记录覆盖新设计。
- UI 引用包括 loading、update、loadingText、loadingPercent、tip、downloadText、downloadPercent、fill、handle，全部使用私有序列化字段绑定。downloading 是私有运行时状态。
- `ShowLoading`、`ShowUpdate` 切换展示区域；`UpdateProgress` 返回 bool，更新对应文本、百分比及下载条；`SetTip` 更新提示。默认下载提示使用 `Downloading resources...`。
- `ShowBackground` 隐藏 Loading、Update，保留 Launch 根节点及独立 Background；通过 XApp.ShowLaunchBackground 暴露操作入口。SV Login 成功打开后调用此入口，避免启动提示、球和 Logo 与登录页重叠。进入不需要背景的游戏页面仍可调用 HideLaunch 隐藏整个 Launch。
- 当前组件和字段引用绑定在 `Assets/Scenes/App.unity` 的 Launch 实例中。检查实际挂载与引用；不要宣称 prefab 资产已包含这些绑定。

## 定位项目与任务

- 从本 skill 目录向上三级定位 GameCLI 目录；以下路径均相对此目录。Windows 命令使用 `pwsh.exe`。涉及 XApp 时同时读取 [Core 规范](../core/SKILL.md)。
- 读取目标工程适用的 AGENTS.md，检查工作区改动。优先采用任务指定的 Unity 工程；仓库内示例为 `examples/unitys/development/client`，使用前确认它确实是任务目标。
- 从任务和已有代码确定 Launch 的入口、启动步骤、完成后去向及异常行为。先查工程现有 UI、启动框架和资源加载方式，再决定实现；缺少会影响业务行为的信息时询问，不凭界面外观发明登录、联网更新等需求。
- 独立 agent 的任务应携带目标工程、项目 UI 标识或来源、Launch 根节点和业务验收条件。可从配置或工程唯一确定的内容直接解析；多个候选无法区分时才询问。

## 拉取项目编辑好的 UI

先读取 `integrations/codex/server/project-ui.mjs`、`server/workspaces.mjs` 和 `server/mcp.mjs`，理解当前项目身份和读取接口。可用 MCP 时通过 `gamecli_ui_snapshot({projectId})` 查看对应项目已保存的 UI 及版本。

- 生产 UI 存在 `gamecli_ui.project_ui`，由项目 ID 标识；基础模板存在 `gamecli_ui.templates`。默认使用目标项目已保存的 UI，不以基础模板代替项目修改。
- snapshot 读取指定项目数据库记录。浏览器未保存草稿独立保留在标签页，不能将其当成数据库版本。区分项目版本和来源模板版本。
- 当前 `ugui --template` 只读取基础模板。读取项目记录时复用 `integrations/codex/server/database.mjs` 的连接配置，采用参数化只读查询；不为生成 prefab 调用 UI 保存或模板同步。
- 留存原始记录和来源版本，定位 Launch 根节点，按父子关系提取完整子树，保留原节点 ID、布局、图片引用和相关动画。检查动作与动画目标是否越出子树；明确处理跨系统引用，不能静默丢弃。
- 为现有 CLI 文件入口准备 `{root, design, ...}` 输入，保留 design 的画布信息和 schemaVersion。原始项目快照另存，不能伪装成基础模板记录。图片从 assets 表按 image ID 读取，校验哈希，保存为 `<image ID>.png`，并保留 `asset-names.json` 的来源文件名映射。
- `gamecli_ui_export` 仅输出 HTML 和 studio.json，不能把导出结果当作已生成的 prefab 或 visual-ui.json。
- 系统模板的原始代码包存于 `gamecli_ui.templates.scripts`，与 payload 并列；模板列表返回该字段。结构为 `{schemaVersion:1,files:[{path,content,sha256}]}`，拉取时保留源码文本并校验哈希，不重新生成已有源码。null 表示未归档，不能声称已取得原始代码。生产代码使用项目根节点的 scripts 副本；标准库代码通过候选审核发布，template_versions 保存不可变历史。旧 gamecli_ui_scripts_save 已关闭。布局与源码的匹配仍需验证。

## 制作预制体

读取 `clis/unity-cli/README.md` 和 `clis/unity-cli/docs/ugui-format.md`，命令能力以当前源码和帮助为准。复用现有 UguiPlugin，不引入新的 UI 持久化格式。

1. 检查 Unity 编译状态、CLI 端点及 Editor 导入器。需要安装导入器时使用 `clis/unity-cli/UnityEditor/Editor/` 中的现有实现。
2. 使用 `ugui --input <输入JSON> --assets-dir <图片目录> --project <工程> --prefab <工程内路径> --dry-run --json` 预检；修复错误并逐项处理 warnings 后执行正式生成。
3. 示例工程已有 `Assets/Data/Widgets/Launch/Launch.prefab`，先检查资源与调用方再确定输出。更新已有资源时检查差异，必要时使用 `--overwrite`，保留 GUID 和用户已有的业务绑定。
4. 生成器负责布局、受支持的动画与静态组件。它不会把网页 action 自动实现成运行时逻辑。Prefab 本身不新增 Canvas，应接入工程现有 UI 容器。
5. 再次生成可能影响手工添加的组件与引用。采用项目已有的绑定方式，使绑定可重复执行，并核对最终保存的 prefab 没有丢失业务组件。

## 编写功能代码

- 在目标工程现有模块结构内实现 Launch 视图和业务逻辑，沿用命名、生命周期、异步和事件约定；不另建平行框架。
- 将固定 UI 节点绑定到私有序列化字段，不使用运行时路径搜索；处理监听注册与释放、重复打开和关闭后的异步回调。
- 按已确定的启动流程驱动提示和进度。进度应来自实际工作；仅当明确要求演示时才使用模拟进度，并标明其用途。
- 将页面动作接到真实业务。按需求处理失败提示、重试、防重复启动和完成后的页面切换，避免完成事件触发多次。
- 对接已有启动入口和资源加载链路，检查 prefab 引用与脚本挂载；不只交付未接入的 C# 文件。

## 验证与交接

Launch 独立代码完成或更新后，由 GameCLI 将源码同步到对应项目的系统副本。先在 `integrations/codex` 运行 `npm run scripts:sync -- config/launch-scripts.json` 核对文件，再在用户已授权源码同步时加 `--apply` 写入。清单关联 `SvStart`（显示名 SV Launch），只包含 XLauncher.cs 及其 .meta，不打包整个框架。新增独立依赖时显式更新清单。确认数据库回读哈希一致，再重新加载对应项目验证文件列表；需要更新标准库时加 --candidate 提交候选，由模板管理员单独审核发布；不得把仅保存本机源码描述为已经归档到模板。

在 Unity 中确认编译通过、无新增相关 Console 错误，检查 prefab 层级、图片、文本、动画和字段引用。运行实际启动路径，对照 UI 验证显示效果，并验证任务涉及的正常完成、失败重试及重复打开行为。只运行与实际改动相关的测试。

交接说明：UI 的项目 ID/来源/版本及根节点、生成的 prefab 和资源路径、代码入口、实现的业务行为、实际执行的验证和未解决的问题。若 Unity 或数据库不可用，明确区分已完成的文件工作与尚未验证的运行结果，不宣称完整链路已通过。
