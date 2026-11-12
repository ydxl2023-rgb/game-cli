---
name: oathx-notice-acceptance
description: 独立验收产品 Notice 交付，检查公告内容、超链接、滚动裁剪、弹窗层级、标准动画与公告生命周期，记录证据并交回开发修复；不用于其他系统的通用验收。
---

# Notice 独立验收

由未参与本轮 Notice 实现的 agent 执行。本技能只定义本模块验收，不自动派发 agent 或启动试跑。阅读[本模块开发契约](../development/SKILL.md)、[编码规范](../../../development-rules/SKILL.md)、[Widget](../../../widget/SKILL.md)、[Widget Extension](../../../widget-extension/SKILL.md)及 [CLI 命令说明](../../../../../integrations/codex/cli/README.md)。

## 开始前

- 收集产品需求、已确认设计变更、模板版本、实际源码/资源路径、接口与测试数据，记录提交及开始/结束文件哈希。模板是范式；没有明确改版时保留所选设计，不用历史示例强加业务。
- Studio 项目核实显式 projectId 的 verified Unity 绑定及 ProjectSettings/GameCLIProject.json 两个 ID，只在匹配工程操作，不默认选 development/practice。
- 对同一 Editor 串行操作；验收不修改业务代码、资源、基准、标准模板或规则。缺陷交回本模块开发 agent；交付改变后重新冻结并复验受影响项。

## 公告交付契约

读取 SvNotice 的实际版本、产品公告结构、内容格式、空态/错误态、关闭调度接口、免打扰需求及标准动画版本。测试公告须明确标识，不能伪造正式内容。单公告、离线、一周免打扰不是所有产品的强制需求。

## 公告专属检查

| 检查项 | 操作与证据 |
| --- | --- |
| 内容及空态 | 输入正常、多行、长文本和空/空白正文；检查标题、正文、条目、空态的实际控件与布局。固定空文案由 propCommonText 解析，动态正文不硬编码成配置。 |
| 超链接 | 按产品协议白名单验证合法/非法 URL；实际生成字形后测试单行、换行、多链接中心命中，普通文字、区域外及被裁剪文字不命中。验证拖动滚动不误触；真实打开链接只在任务授权范围内执行，纯解析通过不等于浏览器跳转通过。 |
| 展开与滚动 | 用真实按钮展开/收起，核对内容高度、滚动边界、RectMask2D 和关闭按钮可用性，不只读 VM 数值。 |
| 遮罩层级 | 读 sibling、Canvas 排序和实际截图；背景遮罩应按产品设计遮挡底层输入，不意外覆盖/压暗正文与关闭按钮。沿用模板的视觉缺陷也要报告来源，不能因“与模板一致”便忽略。 |
| 标准动画 | 读取指定 dialog.open.scale 或产品替代动画版本；核对 controller/state、曲线目标、时长、关键帧/切线，并采样打开过程。窗口主体缩放、全屏遮罩不缩放；重开仍播放，关闭/快速重开不残留实例。 |
| 生命周期 | 从调用方入口打开，按确认的协调接口关闭；验证 OnDetive 清理、连续重开、回调次数及加载中停用，不能让 Login 代管公告内部状态。 |
| 偏好与数据 | 仅在产品要求时验证免打扰过期、保存恢复和手动入口策略；隔离测试存储并恢复本次测试值，不清除用户已有存档。通信未接入须区分占位、空态和失败。 |

代码审查聚焦 NoticeSystem/Entity 与 UI/VM 职责、NoticeText 的协议/点击逻辑及 NoticeLayout 的重排。Metadata 应逐字段核对目标；Text 子类导出为未知组件时，保留原始差异，再以继承关系、实际 Text 更新与链接命中补证。缺少 Empty 节点或新增节点必须与产品设计核对，不能修改基准消除差异。

## 自动检查与取证

在 integrations/codex 下用 pwsh.exe 执行，system 参数使用实际目录名：

```powershell
.\gamecli.cmd verify code --project <项目ID> --system Notice
.\gamecli.cmd verify prefab --project <项目ID> --prefab <页面项目根节点ID> --asset <实际预制路径>
```

保存原始 JSON、退出码及代码 reviewRequired；注释率/空行率只作统计，不设臆造门槛。执行目标 Unity 编译和 Console 检查，不能把静态检查当作运行通过。预制适用时分别读取 status、differences、unverified、bindings；绑定通过不代表整体通过，未知字体/资源身份/自定义组件/动画必须逐项补证，不能改写原始 incomplete。

运行证据记录步骤、输入、预期、实际、日志/截图或对象状态。按钮回调调用不证明真实点击不受遮挡，静态曲线不证明动画实际播放。临时探针不进入产品交付，结束清理测试实例/探针并记录 Play 状态；不保存测试修改到产品资源，不擅自调用真实服务或清除存档。

## 本模块报告与复验

交付单独的本模块报告：需求与用例映射、版本/哈希、源码和资源覆盖范围、原始 CLI 证据、运行证据、缺陷定位/复现/影响/负责人、未覆盖项及复验关联。证据放指定持久化目录，Library 临时路径不能当长期归档。

每项采用 passed / failed / incomplete / not-applicable。不满足明确需求是 failed；必要证据或契约不足是 incomplete；只有确实不适用的检查才能 not-applicable。存在缺陷则总体 failed，无缺陷但必需项有缺口则 incomplete；所有适用必需项充分证明才 passed。补证说明证明范围，保留 CLI 原始状态。修复交回本模块 development agent，复验修复及其依赖，不合并不同交付版本的通过项。

本模块通过不等于跨系统产品全流程通过。验收不授权 Git 提交推送、标准模板发布或上线。
