# GameCLI UI Studio

独立的 Codex MCP 内嵌网页设计工作台，参考旧 Track 的 stdio、UI resource 与宿主 JSON-RPC 桥接方式。保留旧工程不动，不依赖旧业务数据库或审批流程。

## 启动

在当前目录使用 PowerShell 7：

```powershell
npm ci
npm run build
npm test
npm start
```

打开 `http://127.0.0.1:18091/studio`，兼容 `/track`。`GAMECLI_STUDIO_PORT` 可覆盖端口，仅绑定本机。

## Codex 侧边工作台

```powershell
pwsh.exe -File scripts/register-codex.ps1
```

注册新的 `gamecli-ui-studio` MCP，不覆盖 `gameai-track`。重新加载 MCP（必要时重启 Codex），调用 `gamecli_ui_open`。宿主支持 fullscreen 时可点击“展开工作台”。浏览器运行成功不代表 Codex 实际嵌入验收成功。

MCP 进程由 Codex 启动，直接使用相同本机草稿，无需启动 HTTP 预览。`plugin/` 是可分发的技能与展示清单，MCP 由注册脚本单独安装。

## 当前能力

- 背包布局示例；容器、按钮、文本、图片占位；层级选择、拖动和属性编辑。
- 添加、级联删除、撤销、预演显示/关闭/跳转/返回交互、画布缩放和设计分辨率切换。
- 本机草稿保存与重启恢复；revision 乐观并发控制和写锁避免多窗口覆盖。
- Studio JSON 导入；导出带 `data-ui-id` / `data-ui-type` 的 HTML 和 `.studio.json`。
- MCP 打开、查询、保存、导出工具，支持 inline/fullscreen 宿主模式。

### 简化的三栏编辑器

右侧预制控件新增 **道具框 ItemSlot**。一次拖入创建 Container 根及按绘制顺序排列的五个独立子节点：Background、Icon、Mask、Count、Frame。数量默认 99；Mask 是可调不透明度的覆盖层（不是 Unity 裁切 Mask）；Frame 使用空心线框布局占位。整组移动/缩放/删除与撤销均支持，左侧可逐层选择调整。正式图标、边框 Sprite 和 Unity Mask 映射尚未接入，导出的 `data-studio-role` 是 Studio 元信息。

- 左侧显示导出用的真实节点，包括界面根节点和按钮的独立 Text 子节点；双击节点展开属性。
- 中间是等比缩放的手机画布，默认 1080 × 1920，支持 1080 × 2400、1170 × 2532 及横竖屏切换。屏幕显示缩放不改变导出分辨率。
- 右侧是当前支持的标准控件：Panel、Text、Image 占位、Button。可拖入手机画布，拖到面板内自动建立父子关系，单击则在选中容器中居中添加。
- 控件可在画布继续拖动，落点按设计分辨率换算。画布外松手不会创建节点。
- 选中控件自动展开属性，支持宽高数值编辑、8 个边角手柄缩放；按钮的铺满文字子节点随按钮尺寸同步，自定义文字框保持独立。
- 文本框输入实时更新画布，支持换行和字号修改。选中按钮直接编辑其 Text 子节点；双击画布中的文本或按钮可聚焦文字编辑框。一次连续输入或拖动可整体撤销。
- 选中节点后按 Delete 删除节点及其子节点；方向键按设计坐标移动 1 像素，Shift＋方向键移动 10 像素。支持撤销；预览模式、文本/数值输入和输入法组合输入时不拦截快捷键。长按 Delete 不会连续删除其他节点。
- 原 960 × 640 默认示例适配为手机布局；自定义旧稿保留节点并等比居中，迁移须保存才落盘且可撤销。按钮内嵌文字会展开为真实 Text 子节点。

草稿存于 `.data/draft.json`；导出存于 `.data/exports/<唯一ID>/`。这些目录被 Git 忽略。当前是一个共享草稿，切换设计前可导出备份。

## 明确边界

这是一版布局编辑器，模板库已接入 PostgreSQL；尚未接入编排 Agent、生产美术资源或 Unity。图片控件是纯色占位；预演支持界面交互，不执行游戏服务端业务逻辑。分辨率切换保留坐标，不承诺运行时响应式适配。

`.studio.json` 是编辑器模型，不是 `visual-ui.json`。正式转换须通过经过验证的 DOM 提取器输出 visual-ui，再配合独立资源清单，调用 UnityCLI `ugui --dry-run` 和 `ugui`。不直接生成或编辑 prefab YAML。

后续接入：确认 DOM 提取器与 Unity 工程 → 资源清单 → UnityCLI dry-run → prefab 生成 → 截图及交互验收。

异常退出若遗留 `.data/draft.lock`，确认没有正在保存的进程后再移除此锁。保存失败会显示原因，未保存修改仍保留在当前页面。

## 验证

`npm test` 覆盖结构校验、循环依赖、HTML 文本转义、持久化、版本冲突、HTTP 同源限制和 MCP 资源/工具闭环。界面还须在浏览器以及实际 Codex 宿主中分别验收。


## 分类模板数据库

模板保存在远程 `192.168.63.29` 的独立 PostgreSQL 数据库 `gamecli`，使用专用角色 `gamecli_app`。SSH 隧道访问服务器本机 `127.0.0.1:5432`，数据库端口不对局域网开放；SSH 主机指纹固定校验。

- `gamecli_ui.template_categories`：分类（`prefab` 预制控件、`system` 系统模板）、显示名称、排序。
- `gamecli_ui.templates`：模板 ID、分类、名称、说明、版本、JSONB 节点结构和动作引用、创建/更新时间。
- `.env.database`：本机私有连接配置，已被 Git 忽略。格式见 `.env.example`，凭据不发送到网页。
- `npm run db:init`：事务内建表和导入初始 4 个模板；重复运行不覆盖已有模板数据。
- `GET /api/studio/templates` / MCP `gamecli_ui_templates`：读取分类和完整模板。右侧标注“数据库”，可点击“刷新模板库”。读取失败显示重试入口，不悄悄回退到代码模板；仅未配置数据库的开发环境使用内置模板。
- 拖入时从数据库返回的 JSONB 复制节点，生成独立内部 ID 并重映射父子关系和点击目标。模板实例随本机草稿保存，编辑实例不会自动修改模板库。

初始模板的代码生成器仍用于首次初始化；运行中的数据库记录是模板库的数据源。当前提供分类读取与初始化迁移，尚未提供网页端模板发布/版本编辑界面。当前设计草稿和导出文件仍存本机。

## 界面模板标准层级（必须遵循）

完整界面 / system 模板必须采用 `Canvas → 界面根节点 → Animator → 原始内容`。
例如 `Loading → Animator → LoadingBlackBg / Root / RateBg`；原始内容内部的父子关系和顺序不变。

- `Animator` 是根节点下唯一的直接内容容器，名称固定为大驼峰 `Animator`。
- Container、透明、全屏 Stretch：Anchor Min=(0,0)、Max=(1,1)、Pivot=(0.5,0.5)、AnchoredPosition=(0,0)、SizeDelta=(0,0)、Scale=(1,1)、Rotation=0。
- 所有原始直接子节点整体迁入；保留原节点 ID、RectTransform、资源和动作引用，不重排内部层级、不扁平化。
- `animationHost: true` 是未来 Unity 导出器挂载 Animator 组件的位置标记；网页不执行 Animator Controller，也不代表已经创建 Unity 组件。HTML 导出保留 `data-ui-animation-host="true"`。
- 标准化统一调用 `shared/animation-host.mjs` 的 `ensureAnimationHost`；系统模板用 `validateTemplate(payload, 'system')`。重复导入不得套多层 Animator。
- 单个 Image、Button、ItemSlot 等普通控件模板不强制增加 Animator。
- 原始 Unity 提取文件保留不变；标准模板是带上述容器的派生结果。旧 Python 静态提取输出属于中间数据，发布前也必须经过系统模板标准化。
- 动画应以 Animator 节点为绑定根；后续生成 Unity 动画绑定时使用调整后的相对路径，不能沿用迁移前路径。

### 模板分类层级

模板库采用 `类型 → 分类 → 模板` 三层结构：例如 `system（系统模板）→ loading（Loading）→ SvLoading`，同一分类可容纳多个独立模板及各自版本。

- `gamecli_ui.template_categories` 保留顶层类型 prefab/system。
- `gamecli_ui.template_groups` 保存分类，联合主键为 `(category_id,id)`，字段包括 name、sort。
- `gamecli_ui.templates.group_id` 指向分类，与 category_id 组成联合外键，禁止把系统模板放进预制控件分类。
- 新模板发布时同时指定 `category_id` 和 `group_id`。未指定分类时暂存 `general`（未分类），不会在界面丢失。不要为每个相似模板新增顶层类型。
- 当前分组：prefab/item（道具）、prefab/scroll（滚动面板）、prefab/dialog（对话框）、system/loading、system/inventory（背包）。
- `npm run db:init` 包含兼容旧库的分类迁移，不改写现有模板 payload 和版本。分类调整属于元数据修改，不需要重复复制图片资源。
- HTTP / MCP 模板列表增加 groups 与每个模板的 group 字段；UI 按组显示可折叠分类并保留刷新前的展开状态。

### 系统模板源码包

`gamecli_ui.templates.scripts` 是独立的 nullable JSONB 字段，不放在 UI payload 内。旧模板为 null，表示尚未保存源码包。已有库执行 `node scripts/migrate-template-scripts.mjs`；`npm run db:init` 也包含此幂等迁移。

代码包格式：`{ "schemaVersion": 1, "files": [{ "path": "Runtime/XLauncher.cs", "content": "原始源码文本", "sha256": "内容的 SHA256" }] }`。文件路径相对于代码包根目录；可以同时包含 `.cs`、`.meta` 等文本文件。保持 UTF-8 文本的 BOM、换行和空白，保存时计算 SHA256，传入已有 SHA256 时校验一致性。每包最多 128 个文件、4 MiB 文本，不支持二进制压缩包。

`gamecli_ui_templates` 和 HTTP 模板列表直接返回 scripts。通过 MCP `gamecli_ui_scripts_save({templateId,version,scripts})` 保存，仅允许 system 类别；版本不一致会拒绝，成功后模板 version 加一。传 scripts=null 可显式清空。更新代码包替换当前内容，不提供不可变历史版本；需要保留最初版本时应另行设计归档策略。布局同步保留 scripts，但不自动证明旧代码与新布局兼容。代码只作为数据存储，不执行，也不自动导入 Unity。

### 从工程同步独立源码

GameCLI 通过明确的文件清单关联系统模板与源码，不能从界面名称猜测并上传整个工程。Launch 清单为 `config/launch-scripts.json`，归档 `XLauncher.cs` 及其 `.meta`；`projectRoot` 相对于清单文件目录，文件路径相对于 Unity 工程。保留现有模板标识 `SvStart`，显示名称为 SV Launch。

在本目录运行：

```sh
npm run scripts:sync -- config/launch-scripts.json
npm run scripts:sync -- config/launch-scripts.json --apply
```

第一条只预检，第二条更新 `templates.scripts`。同步保留源码 BOM、换行和空白，计算 SHA256、检查版本、备份旧代码包并回读验证；内容相同不增加版本。不会修改 UI payload 或工程 UI，也不会执行源码。完成后点击 Studio 的刷新模板库图标，关系卡片右侧显示归档文件。其他系统使用各自的清单；独立系统代码交付后执行此同步步骤。

### 工程保存与模板同步行为

顶部「保存」先保留本机草稿，再将完整 Canvas、所有系统节点、交互和动画保存到 gamecli_ui.project_ui（project_id 主键、project_name、version、payload JSONB、created_at、updated_at）。保存不写 templates 表；projectVersion 防止覆盖其他窗口更新。数据库失败会保留本机草稿并明确提示。无本机草稿时可从数据库恢复当前工程设计。

当前工程默认 GameCLI；部署时可设置 GAMECLI_PROJECT_ID 和 GAMECLI_PROJECT_NAME，工程身份不随 UI 文件名或节点名称改变。

「同步」单独更新选中节点所属的顶层系统模板；仅一个系统时允许不选节点。多个系统必须选择目标。保留分类、来源与动画，校验模板版本并备份旧模板。未关联基础模板的生产 UI 可正常保存到工程，但不能覆盖任意模板。嵌套预制随所属系统处理。同步不会自动保存工程，也不会生成 Unity prefab。

### 编辑与关系分页

关系页将 Canvas 的直接子节点显示为 UI 缩略卡片。卡片标题可拖动；选择触发节点与动作后，从右侧圆点连到目标卡片左侧圆点（支持拖动或依次点击）。当前支持点击事件的跳转、打开、关闭、切换，每个触发节点保留一个动作，重新连线替换该动作。选择连线可删除，也可撤销。

连线直接使用 node.action，不维护第二份路由数据；卡片位置存入根节点 relationPosition。保存写入工程 project_ui；基础模板同步不携带卡片位置，跨系统引用会阻止单独模板发布。卡片的预演按钮可从该 UI 开始验证跳转。此阶段是网页预演关系，尚未生成 Unity 导航运行时代码。
