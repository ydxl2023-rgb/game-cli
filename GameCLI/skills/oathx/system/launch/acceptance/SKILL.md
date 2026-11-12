---
name: oathx-launch-acceptance
description: 独立验收产品 Launch 交付，检查XLauncher 序列化接入、启动与下载进度、背景切换及启动层级，记录证据并交回开发修复；不用于其他系统的通用验收。
---

# Launch 独立验收

由未参与本轮 Launch 实现的 agent 执行。本技能只定义本模块验收，不自动派发 agent 或启动试跑。阅读[本模块开发契约](../development/SKILL.md)、[编码规范](../../../development-rules/SKILL.md)、[Core](../../../core/SKILL.md)及 [CLI 命令说明](../../../../../integrations/codex/cli/README.md)。

## 开始前

- 收集产品需求、已确认设计变更、模板版本、实际源码/资源路径、接口与测试数据，记录提交及开始/结束文件哈希。模板是范式；没有明确改版时保留所选设计，不用历史示例强加业务。
- Studio 项目核实显式 projectId 的 verified Unity 绑定及 ProjectSettings/GameCLIProject.json 两个 ID，只在匹配工程操作，不默认选 development/practice。
- 对同一 Editor 串行操作；验收不修改业务代码、资源、基准、标准模板或规则。缺陷交回本模块开发 agent；交付改变后重新冻结并复验受影响项。

## 启动交付契约

读取 SvStart 实际版本、目标 XApp/XLauncher 源码、启动场景、阶段/进度来源、失败提示和启动完成后的展示要求。Launch 是业务 UISystem 就绪前可用的 UGUI 组件，不需要单例或 LaunchSystem。

## Launch 专属检查

| 检查项 | 操作与证据 |
| --- | --- |
| 真实入口引用 | 读取 XApp 私有序列化 launcher 指向的实际实例与 prefab 来源，排除 disabled 旧占位组件。区分 prefab 内绑定和场景实例绑定；禁止用运行时 Find 修复缺失引用。 |
| 引用完整性 | 按本次组件和模板核对加载/更新面板、文字、提示、fill/handle 等需要的字段。百分比独立标签仅在代码支持合并文字且设计一致时可空，不强制历史字段数量。 |
| 启动面板 | 在业务页面未就绪时实际观察 ShowLoading、提示和进度；0、0.375、1 及非法/越界值按接口约定核对实际文本。直接接口测试不代替真实启动阶段时序。 |
| 下载面板 | ShowUpdate 正确切换面板，提供明确测试进度，读取真实文本、RawImage/RectTransform fill 和 handle 的水平锚点/位置，垂直布局不被破坏。 |
| 完成后展示 | 分别测试 Hide、Show 和 ShowBackground；仅背景模式应根仍可见而前景两面板隐藏，后续 UI 无双层加载效果和点击遮挡。 |
| 场景层级 | 验证根 Stretch、Canvas 排序/缩放与真实 Game View，启动背景不能盖住业务 UI；重入启动场景不得保留错误面板状态。 |
| 动画与来源 | 按节点身份核对动画曲线路径、循环、参数及运行表现；既检查资源自身，也检查场景当前引用。进度由调用方提供，不能加入伪下载/自动增长来通过验收。 |

代码审查覆盖框架包内 XLauncher、XApp 变更和启动调用方；私有 SerializeField 引用、普通组件生命周期及返回语义按开发规范检查。没有 Widget/VM 时绑定检查为 not-applicable；零 Widget 的 bindings passed 不证明启动引用正确。

## 自动检查与取证

`verify code` 仅支持 Assets/Scripts/Systems/<目录>；XLauncher 位于框架包时明确记录自动覆盖缺口并独立逐文件审查，不为适配命令移动代码。对启动预制在 integrations/codex 下用 pwsh.exe 执行：

```powershell
.\gamecli.cmd verify prefab --project <项目ID> --prefab <Launch项目根节点ID> --asset <实际Launch预制路径>
```

保存原始 JSON、退出码及代码 reviewRequired；注释率/空行率只作统计，不设臆造门槛。执行目标 Unity 编译和 Console 检查，不能把静态检查当作运行通过。预制适用时分别读取 status、differences、unverified、bindings；绑定通过不代表整体通过，未知字体/资源身份/自定义组件/动画必须逐项补证，不能改写原始 incomplete。

运行证据记录步骤、输入、预期、实际、日志/截图或对象状态。按钮回调调用不证明真实点击不受遮挡，静态曲线不证明动画实际播放。临时探针不进入产品交付，结束清理测试实例/探针并记录 Play 状态；不保存测试修改到产品资源，不擅自调用真实服务或清除存档。

## 本模块报告与复验

交付单独的本模块报告：需求与用例映射、版本/哈希、源码和资源覆盖范围、原始 CLI 证据、运行证据、缺陷定位/复现/影响/负责人、未覆盖项及复验关联。证据放指定持久化目录，Library 临时路径不能当长期归档。

每项采用 passed / failed / incomplete / not-applicable。不满足明确需求是 failed；必要证据或契约不足是 incomplete；只有确实不适用的检查才能 not-applicable。存在缺陷则总体 failed，无缺陷但必需项有缺口则 incomplete；所有适用必需项充分证明才 passed。补证说明证明范围，保留 CLI 原始状态。修复交回本模块 development agent，复验修复及其依赖，不合并不同交付版本的通过项。

本模块通过不等于跨系统产品全流程通过。验收不授权 Git 提交推送、标准模板发布或上线。
