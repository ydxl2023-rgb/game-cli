---
name: currency-acceptance
description: Independently verify reusable currency widgets, native asset isolation, host callbacks and binding cleanup.
---

# Currency 独立验收

独立复验时按[模板提取与复验](../../../system-development/references/template-fidelity.md)对照来源、示例输入和运行证据，具体判定采用本系统用例。

读取 [开发规范](../../../development-rules/SKILL.md) 和 [组件契约](../development/SKILL.md)。验收 agent 不修改交付；保留原始 CLI 证据，缺陷退回开发。

1. 核查 Currency 不引用 HomeEntity、HomeSystem 或 Home 私有资源；四类源码、图片、字体、两个预制及 meta 完整。检查所有 GUID，而不只检查 Image。
2. Bar 为一个 SyncGroup，三个直接子 Widget；每项七个绑定：amount、icon、visible、canAdd、interactable、openClick、addClick。图标显示 Image 与图标点击 Button 不是同一节点，不混淆。
3. 独立实例验证三项数值、图标替换、两种按钮回调次数、显隐及禁用；确认无 Home 实例也能工作。隐藏装饰不能遮挡点击。
4. 验证解绑后旧 VM 不再更新画面、旧按钮不回调、重复 Ref 被拒绝，销毁无异常；宿主关闭重开得到正常新实例。
5. 在实际宿主验证嵌套预制引用、停靠位置、字体、三图标尺寸、数值、点击路径和已有红点；保留截图与 Console 证据。
6. 数据库逐文件 SHA256 对照工程，材料 GUID 与宿主依赖清单一致；预览资源存在。明确浏览器预览不执行 C#，原始静态布局验收与运行时绑定验证分别报告。

GameCLI 示例可在 Play 中执行 `CurrencyProbe.Check` 和 `ReadHome`；纯独立组件项目只执行前者。模板配置的三项顺序是参考设计，产品改变数量时同步预制与 SyncGroup 数量契约后再验收。
