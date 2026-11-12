---
name: oathx-loading-acceptance
description: 独立验收产品 Loading 交付，检查UILoading/VMLoading 的真实进度展示、边界输入、提示更新与页面打开关闭，记录证据并交回开发修复；不用于其他系统的通用验收。
---

# Loading 独立验收

由未参与本轮 Loading 实现的 agent 执行。本技能只定义本模块验收，不自动派发 agent 或启动试跑。阅读[本模块开发契约](../development/SKILL.md)、[编码规范](../../../development-rules/SKILL.md)、[Widget](../../../widget/SKILL.md)、[Widget Extension](../../../widget-extension/SKILL.md)及 [CLI 命令说明](../../../../../integrations/codex/cli/README.md)。

## 开始前

- 收集产品需求、已确认设计变更、模板版本、实际源码/资源路径、接口与测试数据，记录提交及开始/结束文件哈希。模板是范式；没有明确改版时保留所选设计，不用历史示例强加业务。
- Studio 项目核实显式 projectId 的 verified Unity 绑定及 ProjectSettings/GameCLIProject.json 两个 ID，只在匹配工程操作，不默认选 development/practice。
- 对同一 Editor 串行操作；验收不修改业务代码、资源、基准、标准模板或规则。缺陷交回本模块开发 agent；交付改变后重新冻结并复验受影响项。

## 加载展示交付契约

读取 SvLoading 实际版本，确认进度单位、舍入、文本/进度条格式、提示、取消意图及调用方的关闭职责。此模块只需要 UILoading/VMLoading，不因验收新增 LoadingSystem、任务或模拟进度。

## Loading 专属检查

| 检查项 | 操作与证据 |
| --- | --- |
| 异步打开 | 由明确完成回调取得页面，再更新 VM；回调前不能把加载请求当作就绪。页面不绑定 VM 时的接口结果须符合约定。 |
| 进度边界 | 对 0–1 契约输入 -1、0、0.375、1、2、NaN、负/正无穷，核对约定的截断/非法值处理；示例舍入应为 0/0/38/100/100/0/0/100%。若产品格式不同，以产品契约为准并列出预期。 |
| 实际刷新 | 读 VM 和实际 Text/进度条/指示器，检查连续更新、消息替换、null/空提示；只看到 VM 数值不算展示通过。 |
| 调用方职责 | 保持空闲时进度不自行增长；100% 不自动关闭或切系统，除非产品明确另有契约。取消按钮如有，仅按契约发出意图，不终止未知任务。 |
| 关闭重开 | 从调用方关闭后再次打开、提供新的进度和提示，检查旧数据/监听/任务不泄漏；加载中关闭按框架异步契约验证。 |
| 布局与动画 | 进度文字、提示、背景在实际 Game View 可见；读取动画节点身份核对曲线目标，不能盲用模板中过时的 sourcePath。循环动画与进度展示职责分开。 |

检查 UI/VM 目录结构、Config 资源路径和 Metadata 字段顺序/实际目标，不固定两项字段作为所有产品要求。图片资源校验实际解码格式、尺寸和导入设置，不能仅根据扩展名或 MIME 判断。Loading 与 Launch 的加载区域分别验收，不混用测试结果。

## 自动检查与取证

在 integrations/codex 下用 pwsh.exe 执行，system 参数使用实际目录名：

```powershell
.\gamecli.cmd verify code --project <项目ID> --system Loading
.\gamecli.cmd verify prefab --project <项目ID> --prefab <页面项目根节点ID> --asset <实际预制路径>
```

保存原始 JSON、退出码及代码 reviewRequired；注释率/空行率只作统计，不设臆造门槛。执行目标 Unity 编译和 Console 检查，不能把静态检查当作运行通过。预制适用时分别读取 status、differences、unverified、bindings；绑定通过不代表整体通过，未知字体/资源身份/自定义组件/动画必须逐项补证，不能改写原始 incomplete。

运行证据记录步骤、输入、预期、实际、日志/截图或对象状态。按钮回调调用不证明真实点击不受遮挡，静态曲线不证明动画实际播放。临时探针不进入产品交付，结束清理测试实例/探针并记录 Play 状态；不保存测试修改到产品资源，不擅自调用真实服务或清除存档。

## 本模块报告与复验

交付单独的本模块报告：需求与用例映射、版本/哈希、源码和资源覆盖范围、原始 CLI 证据、运行证据、缺陷定位/复现/影响/负责人、未覆盖项及复验关联。证据放指定持久化目录，Library 临时路径不能当长期归档。

每项采用 passed / failed / incomplete / not-applicable。不满足明确需求是 failed；必要证据或契约不足是 incomplete；只有确实不适用的检查才能 not-applicable。存在缺陷则总体 failed，无缺陷但必需项有缺口则 incomplete；所有适用必需项充分证明才 passed。补证说明证明范围，保留 CLI 原始状态。修复交回本模块 development agent，复验修复及其依赖，不合并不同交付版本的通过项。

本模块通过不等于跨系统产品全流程通过。验收不授权 Git 提交推送、标准模板发布或上线。
