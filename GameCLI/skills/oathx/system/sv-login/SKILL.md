---
name: sv-login
description: 实现或维护 GameCLI 的 SV Login 页面，使用 LoginSystem、LoginEntity、VMLogin 与 com.oathx.widget，管理页面打开与关闭生命周期，不承担通信管理。
---

# SV Login

开始前必须读取并遵守 [开发规范](../../development-rules/SKILL.md)、[Core](../../core/SKILL.md)、[Widget](../../widget/SKILL.md) 和 [Widget Extension](../../widget-extension/SKILL.md)。所有路径相对 GameCLI 目录（本 skill 目录向上四级）。先检查实际文件和模板最新版本，不能把本文件的历史状态视为最新数据。

## 已确认的职责与结构

- `examples/unitys/client/Assets/Scripts/Systems/Login/LoginSystem.cs` 继承 RuntimeSystem，由原 SV 页面系统重命名而来，保留该脚本的 GUID。旧 LoginSystem 的 Host/Login 通信代码已删除，不迁移到服务或其他系统。
- `OnActive` 先在回调外检查 `IsActive()`，未激活则返回 false，不调用 `OpenWidget`。`OpenWidget<UILogin>` 必须提供显式完成回调，禁止传 `null`，也不能直接把其即时返回值视为加载完成。回调确认 widget 有效后调用 XApp.ShowLaunchBackground，仅保留 Launch 背景，避免两个 UI 的前景叠加；OnActive 发起请求后返回 true，关闭页面由 OnDetive 负责。
- LoginEntity 仍为既有数据实体，UILogin 保留 Invalidate(LoginEntity) 展示入口；LoginSystem 不查询或刷新实体，也不发送登录事件。
- UI 使用 `UI/UILogin.cs`，VM 使用 `UI/VMLogin.cs`，以 VMSevenDayLogin 的 `partial + [Binding]` 为蓝本。Binding 字段为私有 camelCase，由 WidgetGenerator 生成的 VM 数据属性供系统赋值；这些生成属性不是对外暴露 Unity UI 引用。
- 页面通过 UISystem 加载 `Assets/Resources/UI/SVLogin/SVLogin.prefab`，使用 WidgetLayer.center，关闭时不缓存，确保 Unref 释放点击监听。固定 UI 对应关系保存在 prefab 的 Metadata 中，不写运行时节点路径搜索。
- 当前 Metadata 有七项：enterClick、canEnter、serverName、serverStatus、versionText、username、friends。文本显式使用 SyncText，按钮可用状态使用 SyncInteractable，区域显隐使用 SyncVisible。

## 职责边界

用户最新要求：LoginSystem 在 OnActive 中加载打开 UILogin，成功后切换 Launch 为仅背景显示，并必须处理 OnDetive。不要重新加入 RPC、服务端连接、凭证、请求状态、重试、超时、登录事件或自动卸载逻辑。

页面打开后必须绑定 vm.EnterClick 并设置 vm.CanEnter=true。EnterGame 保留激活及按钮状态检查，当前返回 false，不发起通信、不加载自身、不跳转首页、不禁用按钮，也不发出成功事件；等待后续接入新登录流程。MasterSystem 保留卸载 LoginSystem 的入口，由 OnDetive 关闭页面。其他功能按钮继续保持 prefab 的禁用状态。
## 资源与配置

- 来源为数据库系统模板 `SvLogin`；透明按钮修复已发布 v4，GameCLIApp 生产副本同步保存为 v4。生成时在本地派生输入中把根节点改为 SVLogin、同名文本改为 ServerName / ServerStatus / VersionText，以满足 Metadata 唯一匹配。后续使用前读取实际最新版本。
- Change、FriendButton、Enter、Notice 为无图片透明按钮；导入器保留点击 Image，但 Graphic alpha 必须为 0，不能生成白色底板或禁用态灰块。修复同步维护 `clis/unity-cli/UnityEditor/Editor/GameCliUguiImporter.cs` 与目标工程安装副本，验证子节点图片及按钮点击区域仍保留。
- 保留原有动画。若本机导入器不支持模板里的缩放曲线，先检查并同步 `clis/unity-cli/UnityEditor/Editor` 的现有导入器，而不是删除动画数据。
- 使用现有 ugui CLI 先 dry-run 后生成，再使用 widget-metadata 先 dry-run 后绑定；必须检查七个字段均匹配。记录输出是完整 prefab 资源和 Metadata，UIWidget 由 UISystem 加载时挂载。
- 登录入口为 `examples/unitys/config/propFunctionSystem.xlsx` 的 SYS_LOGIN / ClassType，以及导出文件 `Assets/Data/Jsons/Excels/FunctionSystemConfig.bytes` 的 10002 项，当前均为 LoginSystem。通过 UnityCLI excel 更新源表，避免下次导表覆盖入口。

## 验证

检查 Unity 编译与 Console，确认 OnActive 通过 UISystem 打开页面，OnDetive 关闭页面并清理订阅。页面关闭时沿用 cacheWidget=false 的 Unref 和销毁行为。不得将此前通信版本的验证结果当作当前实现的登录能力。
