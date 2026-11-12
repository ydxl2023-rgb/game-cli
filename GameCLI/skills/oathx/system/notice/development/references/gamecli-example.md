# GameCLI 参考实现记录

本文件保存本轮整理前的实现说明，属于参考材料，不是新产品的强制需求或最新数据库状态。仅在维护 GameCLI 示例、复用具体实现细节时读取；可能包含历史目录、版本和绑定数量，使用前核对实际代码与数据库。产品开发入口见 [SKILL](../SKILL.md)。


# 登录公告

遵守 [开发规范](../../../../development-rules/SKILL.md)。涉及预制体生成时使用 unity-cli skill。以下路径相对 GameCLI 根目录。

- 登录页 Announcement 调用 NoticeUtils.RequestLoginNotice，数据返回后 OpenNoticeWindow 打开 LoginNoticeView；其预制体是 SV `Assets/data/UI/Announcement/@AnnouncementView/Prefab/SystemAnnouncementView.prefab`。不要混用游戏内 GameAnnouncementView。
- 数据库模板 ID 为 `SvNotice`，位于 system / notice（公告）。导入脚本是 `integrations/codex/scripts/import-sv-notice.mjs`；默认预检，`--publish` 只允许首次管理员登记，已有模板通过候选发布更新。
- 原生导出器在隔离预制体场景导出；原始证据位于 `.data/imports/sv-notice/unity`，中文本地化的 1920×1080 布局快照位于 localized。源工程资源不得改写。
- 原始 31 个节点、15 张独立图片。Studio 增加全屏 Animator 组织节点后为 32 个节点。没有原生动画片段或控制器依赖；按钮缩放和音效保留在原始组件证据中，不宣称已移植。
- UI 包含标题图、关闭按钮、滚动列表、可展开公告条目、正文/图片单元及一周内不再提示。Mask 的 m_ShowMaskGraphic=false 必须保持图形隐藏，不能生成白色遮挡层。
- 正文、图片和标题运行时来自公告服务。保留预制体样本，不编造线上公告。Studio 的关闭、展开与勾选仅为本地预演，滚动裁剪、动态列表高度、富文本、持久化和外链尚未完整复现。
- 历史版本 v1–v3 的 scripts 归档原工程 LoginNoticeView 的配置、逻辑、View、VM 及 .meta 共八个文件；v4 已替换为 GameCLIApp 的 Notice 系统源码。
- GameCLI 实现使用 `Assets/Data/Widgets/Notice/Notice.prefab`，页面和 VM 在 `Assets/Scripts/Systems/Notice/UI/`，NoticeText、NoticeLayout 组件在 `Assets/Scripts/Systems/Notice/Components/`。NoticeSystem 位于 Notice 根目录，继承 RuntimeSystem，负责页面生命周期和本地公告内容，目前没有通信实现。

## Unity 实现

- Login 的 VMLogin.announcementClick 绑定 Announcement 按钮，LoginSystem.OpenNotice 检查 IsActive 后投递 SystemActive(type=NoticeSystem)，由 MasterSystem 加载。NoticeSystem.OnActive 在 OpenWidget 前检查 IsActive，并在显式异步回调中初始化 UINotice、绑定关闭按钮为投递 SystemDetive(type=NoticeSystem)，由 MasterSystem 卸载、填充本地测试公告。OnDetive 先 UnsubscribeAll，空行后关闭页面。Master 切换退出 Login 时卸载 NoticeSystem，Login 不直接管理公告生命周期。
- UINotice.Initialize 绑定标题/箭头展开、勾选/标签点击；关闭按钮由 NoticeSystem 绑定为系统卸载请求。初始化先清空正文，再由 NoticeSystem 填充默认测试公告；空内容仍显示“暂无公告”。SetContent(title, text) 提供单条本地文本入口，空白正文清空；支持 UGUI 富文本与 HTTP/HTTPS 超链接，暂不支持多条列表和图片公告。
- 一周免打扰到期时间写入 PlayerPrefs 的 GameCLI.Notice.SuppressUntil。IsReminderSuppressed 可供未来自动打开流程判断；手动公告按钮始终允许打开，目前没有自动弹公告逻辑。
- VMNotice 有 11 项 Metadata；VMLogin 增加公告入口后为 8 项。固定组件引用通过 Metadata 或 NoticeLayout 的 private SerializeField 绑定，不使用运行时路径搜索。
- NoticeLayout 根据正文 preferredHeight 更新条目和 ScrollRect 内容高度。Entries、Entry、Expanded、BodyContent、BodyRow、Body 保持顶部锚点/轴心，避免长文本将标题推离视口。
- Viewport 使用 ScrollRect + RectMask2D，Entries 为 content。背景 Bg 的 Image.raycastTarget=true，阻止公告打开时点击穿透登录页。
- 正式生成必须使用 UnityCLI ugui，先 dry-run。派生生成输入位于目标工程 Library/GameCLI/NoticeSource；生成后通过 CLI 绑定 Metadata、NoticeLayout 四个引用、顶部布局和背景射线，不能直接覆盖已绑定 prefab 后忽略这些步骤。
- 源码清单 `integrations/codex/config/notice-scripts.json` 包含 NoticeSystem、UINotice、VMNotice、NoticeLayout、NoticeText 及 .meta。2026-11-12 已验证 GameCLIApp 的 Unity 绑定，经独立草稿、候选发布为 SvNotice v4，scripts 与绑定工程的五个 C# 文件及对应 .meta（共十个文件）逐文件一致；布局和动画保持不变。后续源码修改仍需显式同步发布，项目实例不会自动升级。
- 已验证登录公告入口、空状态、关闭重开、勾选持久化、长文本滚动边界、展开收起和清空。临时测试脚本退出 Play 后从 Assets 删除，验证副本和截图在 Library/GameCLI。

## 正文超链接

- Body 使用 NoticeText（继承 UnityEngine.UI.Text），保持 VMNotice.body 的 SyncText 绑定；NoticeLayout.body 同样引用该组件。raycastTarget 必须开启。更换组件后通过 widget-metadata 重新绑定并验证，不能只替换脚本后保留失效引用。
- SetContent 的正文支持 `<a href="https://example.com/news">查看详情</a>`，href 可使用双引号、单引号或无引号格式；URL 内的 `&amp;` 会解码。仅允许绝对 HTTP/HTTPS URL，其他协议保留标签文字但不可点击。普通 URL 不自动转成超链接。
- 保留 UGUI 的 b/i/size/color 富文本；链接内部颜色统一为 linkColor。点击命中依据实际生成的字形区域，支持多链接与跨行链接，尊重父级 RectMask2D。拖动滚动列表不打开链接，只有完成左键点击才调用 Application.OpenURL。
- 生成预制体后须将 Body 的 Text 替换为 NoticeText，恢复字体、字号 35、MiddleLeft 对齐和正文颜色，再绑定 Metadata 和 NoticeLayout.body。不可把重新生成后丢失超链接组件的 prefab 当作完成品。
- 运行态验证覆盖多链接、跨行字形、富文本、URL 解码、视口裁剪、拖动抑制、不支持的协议及刷新清理旧链接。测试副本保存在 Library/GameCLI/NoticeLinkCheck.cs.txt；验证不实际打开浏览器。
- FunctionSystem 配置登记 SYS_NOTICE=10009、ClassType=NoticeSystem、InstallOnStartup=0、Active=1，按需加载时激活，不在启动时弹出公告。源表为 examples/unitys/development/config/propFunctionSystem.xlsx，导出结果须同步到客户端配置。

## 打开动画

- Notice 根节点 Animator 绑定 `Assets/Data/Widgets/Notice/Animations/Notice.controller`，默认状态 Open 使用 NoticeOpen.anim，220ms、XY 缩放 0.94→1、减速结束、不循环。Animator 使用 Unscaled Time。
- `Notice/Animator/Panel` 是窗口内容容器，包含 WhiteBg、TopBg、ScrollView、WeekNomoreShow；通过 CLI prefab group 创建，保留原有组件引用。遮罩 Bg 留在 Panel 外，不缩放；Empty 仍在根节点，由同一动画单独缩放。
- 不保留原 Animator 组织节点上的无控制器 Animator，避免嵌套动画组件。cacheWidget=false，每次打开新实例由默认 Open 状态播放，无需系统代码驱动。
- 动画编辑使用工程内 `Library/UnityCLI/unity-cli.exe animator`。仅支持场景绑定时，prefab 组件与控制器引用通过 UnityCLI components/modify 保存。重新生成 prefab 后须恢复分组、根 Animator 和以上引用。
- 数据库 SvNotice v3 已绑定标准 `dialog.open.scale` v1：新增 Panel 容纳 WhiteBg、TopBg、ScrollView、WeekNomoreShow，Bg 保持不缩放。预演使用 design.animations 的完整缩放曲线，不能仅同步 Unity .anim 而遗漏预演数据。同步脚本为 integrations/codex/scripts/sync-notice-animation.mjs，默认预检，--apply 经草稿/候选发布并回读校验；存在已有动画或不同草稿时停止覆盖。旧项目实例需要显式升级模板，不自动修改项目。

