---
name: oathx-sv-login-acceptance
description: 独立验收产品 Login 交付，检查登录主流程、区服选择、入口路由、LoginEntity 数据及登录页面生命周期，记录证据并交回开发修复；不用于其他系统的通用验收。
---

# Login 独立验收

由未参与本轮 Login 实现的 agent 执行。本技能只定义本模块验收，不自动派发 agent 或启动试跑。阅读[本模块开发契约](../development/SKILL.md)、[编码规范](../../../development-rules/SKILL.md)、[Widget](../../../widget/SKILL.md)、[Widget Extension](../../../widget-extension/SKILL.md)及 [CLI 命令说明](../../../../../integrations/codex/cli/README.md)。

## 开始前

- 收集产品需求、已确认设计变更、模板版本、实际源码/资源路径、接口与测试数据，记录提交及开始/结束文件哈希。模板是范式；没有明确改版时保留所选设计，不用历史示例强加业务。
- Studio 项目核实显式 projectId 的 verified Unity 绑定及 ProjectSettings/GameCLIProject.json 两个 ID，只在匹配工程操作，不默认选 development/practice。
- 对同一 Editor 串行操作；验收不修改业务代码、资源、基准、标准模板或规则。缺陷交回本模块开发 agent；交付改变后重新冻结并复验受影响项。

## 登录交付契约

读取 SvLogin 的实际版本，明确本次登录是本地试跑还是指定认证服务、成功/失败载荷、是否需要选区、区服状态、入口用途及成功后的交接对象。没有接口时不把占位提示或默认 pid 当作真实登录成功；本地成功事件仅按已确认试跑契约验收。

## 登录专属检查

| 检查项 | 操作与证据 |
| --- | --- |
| 激活与停用 | 未激活时不发起异步 OpenWidget；激活后实际打开 UILogin；OnDetive 用局部类型数组关闭本系统全部页面并清理订阅。停用后重开、加载中停用均检查无遗留页面和重复回调。 |
| VM 与实体 | ServerName、状态、身份等实际控件随 VM 刷新；实体获取/VM 获取分别判空。System 不缓存 VM 或把选区数据留作系统字段；非缓存页面通过可查找已打开页面的接口读取。 |
| 登录结果 | 从真实开始游戏按钮验证约定请求/本地事件、载荷及接收方数据；检查重复点击、失败和恢复分支。不能为验收恢复旧 RPC 或临时生成 Home。 |
| 本地选区（需求包含时） | 分组切换、区服列表、推荐/最近区、选中回填、维护状态等按需求验证；选择写 LoginEntity，需保存时由实体方法完成。默认区服名称和“待接入”提示不算选区实现；模板缺少子页也不能豁免产品要求。 |
| 子页绑定（交付包含时） | 分别核对服务器选择页、分组/行 Widget 的 VM、Fancy 列表和复用后数据/点击对象；主页面绑定通过不覆盖子页与 cell。 |
| 其他入口 | 按产品定义检查公告、用户中心、适龄提示等按钮的可见性、interactable、射线首命中及路由；未接业务不等于应统一禁用。待接入口提示也必须实际可见。 |
| 启动交接 | 若本次接入 Launch，页面就绪后按契约保留背景/隐藏前景，不出现双层加载 UI 或射线遮挡。 |

对存储键的类型反射、ServerState 枚举、固定文案表 ID 和异步空值处理做源码审查。不要固定历史 VM 数量或 SV 资源路径；以本次发布源码、实际 Config 路径及批准的产品设计为依据。

## 自动检查与取证

在 integrations/codex 下用 pwsh.exe 执行，system 参数使用实际目录名：

```powershell
.\gamecli.cmd verify code --project <项目ID> --system Login
.\gamecli.cmd verify prefab --project <项目ID> --prefab <页面项目根节点ID> --asset <实际预制路径>
```

对主页面和本次交付的所有子页/列表项分别记录覆盖。没有独立项目根节点的 cell 不能冒充根节点调用，须独立补证并保留工具缺口。

保存原始 JSON、退出码及代码 reviewRequired；注释率/空行率只作统计，不设臆造门槛。执行目标 Unity 编译和 Console 检查，不能把静态检查当作运行通过。预制适用时分别读取 status、differences、unverified、bindings；绑定通过不代表整体通过，未知字体/资源身份/自定义组件/动画必须逐项补证，不能改写原始 incomplete。

运行证据记录步骤、输入、预期、实际、日志/截图或对象状态。按钮回调调用不证明真实点击不受遮挡，静态曲线不证明动画实际播放。临时探针不进入产品交付，结束清理测试实例/探针并记录 Play 状态；不保存测试修改到产品资源，不擅自调用真实服务或清除存档。

## 本模块报告与复验

交付单独的本模块报告：需求与用例映射、版本/哈希、源码和资源覆盖范围、原始 CLI 证据、运行证据、缺陷定位/复现/影响/负责人、未覆盖项及复验关联。证据放指定持久化目录，Library 临时路径不能当长期归档。

每项采用 passed / failed / incomplete / not-applicable。不满足明确需求是 failed；必要证据或契约不足是 incomplete；只有确实不适用的检查才能 not-applicable。存在缺陷则总体 failed，无缺陷但必需项有缺口则 incomplete；所有适用必需项充分证明才 passed。补证说明证明范围，保留 CLI 原始状态。修复交回本模块 development agent，复验修复及其依赖，不合并不同交付版本的通过项。

本模块通过不等于跨系统产品全流程通过。验收不授权 Git 提交推送、标准模板发布或上线。
