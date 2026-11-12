# GameCLI 参考实现记录

本文件保存本轮整理前的实现说明，属于参考材料，不是新产品的强制需求或最新数据库状态。仅在维护 GameCLI 示例、复用具体实现细节时读取；可能包含历史目录、版本和绑定数量，使用前核对实际代码与数据库。产品开发入口见 [SKILL](../SKILL.md)。


# SV Login

开始前必须读取并遵守 [开发规范](../../../development-rules/SKILL.md)、[Core](../../../core/SKILL.md)、[Widget](../../../widget/SKILL.md) 和 [Widget Extension](../../../widget-extension/SKILL.md)。所有路径相对 GameCLI 目录（本 skill 目录向上四级）。先检查实际文件和模板最新版本，不能把本文件的历史状态视为最新数据。

## 已确认的职责与结构

- `examples/unitys/development/client/Assets/Scripts/Systems/Login/LoginSystem.cs` 继承 RuntimeSystem，由原 SV 页面系统重命名而来，保留该脚本的 GUID。旧 LoginSystem 的 Host/Login 通信代码已删除，不迁移到服务或其他系统。
- `OnActive` 先在回调外检查 `IsActive()`，未激活则返回 false，不调用 `OpenWidget`。`OpenWidget<UILogin>` 必须提供显式完成回调，禁止传 `null`，也不能直接把其即时返回值视为加载完成。回调确认 widget 有效后调用 XApp.ShowLaunchBackground，仅保留 Launch 背景，避免两个 UI 的前景叠加；OnActive 发起请求后返回 true，关闭页面由 OnDetive 负责。
- LoginEntity 仍为既有数据实体，UILogin 保留 Invalidate(LoginEntity) 展示入口；LoginSystem 按需读写 LoginEntity 的本地区服选择；开始游戏时查询本地 MasterEntity 的 PID 并投递 LoginSucceed，不发送网络请求。
- UI 使用 `UI/UILogin.cs`，VM 使用 `VM/VMLogin.cs`，以 VMSevenDayLogin 的 `partial + [Binding]` 为蓝本。Binding 字段为私有 camelCase，由 WidgetGenerator 生成的 VM 数据属性供系统赋值；这些生成属性不是对外暴露 Unity UI 引用。
- 页面通过 UISystem 加载 `Assets/Data/Widgets/SVLogin/SVLogin.prefab`，使用 WidgetLayer.center，关闭时不缓存，确保 Unref 释放点击监听。固定 UI 对应关系保存在 prefab 的 Metadata 中，不写运行时节点路径搜索。
- 当前 Metadata 有九项，顺序为 announcementClick、enterClick、canEnter、serverName、serverStatus、versionText、username、friends、changeClick。文本显式使用 SyncText，按钮可用状态使用 SyncInteractable，区域显隐使用 SyncVisible。

## 职责边界

用户最新要求：LoginSystem 在 OnActive 中加载打开 UILogin，成功后切换 Launch 为仅背景显示，并必须处理 OnDetive。不要重新加入 RPC、服务端连接、凭证、请求状态、重试、超时或自动卸载逻辑。

页面打开后必须绑定 vm.EnterClick，并按当前区服状态设置 vm.CanEnter（维护区服为 false，其余为 true）。EnterGame 保留激活、VM 和按钮状态检查，确认本地 MasterEntity 存在后通过 XPluginUtility.PostEvent 投递 LoginSucceed；返回值表示事件入队。Master 收到事件后加载本地数据，成功再 Launch 进入 Home。此流程无网络请求，不禁用其他按钮。MasterSystem 保留卸载 LoginSystem 的入口，由 OnDetive 关闭页面。公告、用户中心、适龄提示、换区等所有按钮保持 interactable=true；业务未接入不等于应禁用按钮。不得在派生模板或生成步骤中批量关闭按钮，避免默认 DisabledColor 造成图片变灰或半透明。未实现的业务回调保持未接入，不宣称功能已完成。
## 资源与配置

- 来源为数据库系统模板 `SvLogin`；透明按钮修复已发布 v4，GameCLIApp 生产副本同步保存为 v4。生成时在本地派生输入中把根节点改为 SVLogin、同名文本改为 ServerName / ServerStatus / VersionText，以满足 Metadata 唯一匹配。后续使用前读取实际最新版本。
- Change、FriendButton、Enter、Notice 为无图片透明按钮；导入器保留点击 Image，但 Graphic alpha 必须为 0，不能生成白色底板或禁用态灰块。修复同步维护 `clis/unity-cli/UnityEditor/Editor/GameCliUguiImporter.cs` 与目标工程安装副本，验证子节点图片及按钮点击区域仍保留。
- 保留原有动画。若本机导入器不支持模板里的缩放曲线，先检查并同步 `clis/unity-cli/UnityEditor/Editor` 的现有导入器，而不是删除动画数据。
- 使用现有 ugui CLI 先 dry-run 后生成，再使用 widget-metadata 先 dry-run 后绑定；必须检查九个字段及顺序均匹配。记录输出是完整 prefab 资源和 Metadata，UIWidget 由 UISystem 加载时挂载。
- 登录入口为 `examples/unitys/development/config/propFunctionSystem.xlsx` 的 SYS_LOGIN / ClassType，以及导出文件 `Assets/Data/Jsons/Excels/FunctionSystemConfig.bytes` 的 10002 项，当前均为 LoginSystem。通过 UnityCLI excel 更新源表，避免下次导表覆盖入口。

## 标准模板代码包

`SvLogin v6` 的 scripts 已归档 LoginSystem、LoginEntity、UILogin、VMLogin 四个源码及其 .meta（共八个文件），旧 LoginView 代码包保留在 v4 历史版本。清单位于 `integrations/codex/config/login-scripts.json`，保留原始 UTF-8 内容、GUID 和 SHA256。此代码包依赖项目现有 RuntimeSystem、Oathx 框架、WidgetGenerator、XApp.ShowLaunchBackground，以及 LoginEntity.Refresh 使用的 LoginResponse 协议类型；它不是完整框架安装包。

仅更新标准模板代码时，以标准模板当前布局创建候选、替换 scripts，审核布局未变后通过 workspaces.publish 发布，回读核对源码哈希与历史版本。不要直接执行旧 saveTemplateScripts 绕过发布流程。清单的 scripts:sync 命令默认仅预检；加 --apply 会更新 GameCLIApp 生产副本，必须符合当次任务范围。本次只发布标准模板，生产副本仍保持原内容。

## 验证

检查 Unity 编译与 Console，确认 OnActive 通过 UISystem 打开页面，OnDetive 关闭页面并清理订阅。页面关闭时沿用 cacheWidget=false 的 Unref 和销毁行为。不得将此前通信版本的验证结果当作当前实现的登录能力。

## 本地换区

- 换区是 LoginSystem 拥有的子页面，采用 UIServerSelect / VMServerSelect，不新增独立系统。LoginSystem 打开、选择回填、关闭和 OnDetive 清理；页面只负责分类与列表展示。
- SV 来源为 `Assets/data/UI/NewLogin/@NewLoginView/Prefab/ServerSelectView.prefab`。目标为 `Assets/Data/Widgets/ServerSelect/ServerSelect.prefab`，原始图片随目录保存。
- 分类沿用最近登录、推荐、每十个区服分组。当前默认十二条明确标注“本地测试”的数据，不代表真实区服；`LoginSystem.SetServers` 可原子替换非空、ID 唯一的合法目录，不包含通信。真实角色数据未接入前隐藏原始头像与等级示例。
- 选择更新登录页区服名称、状态及本地选择键 `login.loginentity.selectedserver`；维护状态禁止进入，其他状态允许。最近登录键 `login.loginentity.recentserver` 在本地 LoginSucceed 事件成功入队时记录。直接关闭不改变选择。
- 换区列表使用 `UIServerGroupItem/VMServerGroupItem` 与 `UIServerItem/VMServerItem`，通过 `VMServerSelect` 的 VList 和 FancyGridClick 绑定两个 FancyGridScrollView。行由 FancyGridScrollViewCell 复用；禁止 ServerSelectRow、手动 Instantiate/Destroy 行或手动注册按钮监听。LoginSystem 负责分类数据和选择回填，UIServerSelect 仅提供重置滚动的展示操作。
- GamePlugin 的 RegisterSyncActions 必须包含 SyncFancyGridScrollView 和 SyncFancyGridClick。行根 pivot 为 (0.5,0.5)，根 Button 提供 Fancy 点击入口，子 Button 组件移除且其 Image 不拦截射线。原始行模板通过 VM 的 SyncVisible 隐藏。右侧两列，顶部留出角标空间。
- VM 字段顺序必须与 Metadata 列表顺序一致。Login 的 Change 按钮下还有同名 Change 文本，自动匹配会歧义；保留原结构，通过 CLI 显式绑定 `SVLogin/Animator/Content/Server/Change#Button`，不能绑定子文本。
- 提取脚本 `integrations/codex/scripts/import-sv-server-select.mjs` 消费 SV 现有只读导出器的 `.data/imports/sv-server-select/unity/unity-prefab.json`。Unity CLI ugui 先 dry-run 再生成，随后对 VMServerSelect 生成 Metadata，并在 Unity 项目目录运行 `integrations/codex/scripts/bind-sv-server-select.ps1 -Cli <UnityCLI.exe>` 恢复 Fancy、行 Widget 和 Metadata 绑定。Login 第九项 changeClick 的显式绑定保留，重新生成 Login 时必须恢复。每个 Widget 仅保留一份 Metadata，字段顺序须与对应 VM 一致。
- 本次完成本地 Unity 页面，尚未将 ServerSelect 发布到数据库标准模板；不要将本地完成描述为已入库。

## Login 代码目录

- `Login/VM/`：VMLogin、VMServerSelect、VMServerGroupItem、VMServerItem。
- `Login/UI/`：UILogin、UIServerSelect、UIServerGroupItem、UIServerItem。
- `Login/`：LoginSystem、LoginEntity 等其余代码；ServerEntry 是登录实体专用的简单数据类型，与 LoginEntity 一起声明在 LoginEntity.cs 中，不单拆文件。
- 遵循共享开发规范中的系统代码目录规则；源码清单 `integrations/codex/config/login-scripts.json` 已按此结构收录当前 Login 全部 C# 和对应 .meta。数据库历史版本的原始路径保留，不将更新本地清单视为已经发布数据库模板。

- LoginSystem 不保留 VMLogin 成员引用。初始化、区服回填及进入游戏时通过 `UISystem.TryGetWidget<UILogin>` 按需获取已打开页面，页面不存在则不访问 VM。

- 区服选择和最近登录属于 LoginEntity 数据：selectedServer、recentServer 属性只允许外部读取；SelectServer / RecordLoginServer 使用传统方法统一完成赋值和原有 PlayerPrefs 持久化。LoginSystem 不保留 selectedServer 字段，不直接读写这些存储键。

- ServerEntry 仅提供 state 枚举，不包含状态文案转换。LoginSystem 将状态传入 UILogin.SetServerState，由登录页面决定显示文字；业务层仍按枚举判断能否进入。

- LoginEntity 读写键统一通过 `StorageKey.For<LoginSystem, LoginEntity>(entity => entity.selectedServer)` 等表达式反射生成；不手写键、不保留旧大小写键迁移。共享接口位于 `Assets/Scripts/Systems/StorageKey.cs`，归档代码包时一并包含。

## 固定文案

- Login 的固定文案使用 `propCommonText.xlsx`，遵循 development-rules 的固定文案与多语言规则。
- 当前新增 ID 1001–1007，别名为 LOGIN_RECENT_SERVER、LOGIN_RECOMMENDED_SERVER、LOGIN_SERVER_RANGE、LOGIN_LOCAL_SERVER_NAME、LOGIN_SERVER_NORMAL、LOGIN_SERVER_BUSY、LOGIN_SERVER_MAINTENANCE。
- `LoginSystem` 和 `UILogin` 通过共享 `Assets/Scripts/Systems/CommonText.cs` 的 `CommonText.Get(Config.Excel.LOGIN_..., args)` 获取显示文本；不在 Entity 中处理显示文案。
- 源码包需包含 CommonText 辅助代码；接入工程同时安装 CommonText 源表条目并执行导表，不能只复制脚本而遗漏配置依赖。

