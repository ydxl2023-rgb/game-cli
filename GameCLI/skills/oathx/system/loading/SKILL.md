---
name: loading
description: 独立 agent 参考 SvLoading 模板开发产品的 Loading 展示模块，只实现 UILoading/VMLoading 与调用接口，不创建 LoadingSystem 或承担加载任务。
---

# Loading 产品开发

开发交付后由另一名 agent 按 [独立系统验收](../../system-acceptance/SKILL.md) 验收本模块的展示接口与页面；不为验收添加 LoadingSystem 或具体加载任务。

必须读取 [系统开发流程](../../system-development/SKILL.md)、[开发规范](../../development-rules/SKILL.md)、[Widget](../../widget/SKILL.md)、[Widget Extension](../../widget-extension/SKILL.md)。

## 默认设计

未明确提出设计变更时，保留选用标准模板的布局、视觉、交互和动画，不自行改版。仅做保持表现与行为一致的必要技术适配；产品契约缺失时不虚构业务。遵循[共享默认规则](../../system-development/SKILL.md#默认保留模板设计)。

## 范式与边界

- 模板 ID：SvLoading。它是可由独立 agent 负责的 UI 模块，不需要 LoadingSystem。
- 调用方拥有任务、进度、提示、取消和关闭时机。页面只展示，不模拟增长、不自动执行任务，不因进度达到 100% 自动切换系统。
- Loading 页面与 Launch 内部的加载区域是不同职责，不合并其生命周期。

## agent 开始时确定

产品页面样式、进度单位、提示格式、是否展示进度条、是否可取消以及调用方接口。不要把示例只有进度文本的布局限制为所有产品的要求。

## 实现路径

1. 读取标准模板及产品设计，将资源放入 Assets/Data/Widgets/Loading。
2. UIWidget 放 Loading/UI/UILoading.cs，VM 放 Loading/VM/VMLoading.cs；禁止沿用旧参考中 VM 位于 UI 的目录。
3. 用 VM 与 Metadata 绑定显示字段；以清晰接口接收真实进度与提示。若接口采用 0–1，明确 NaN、越界和未绑定 VM 的处理及 bool 返回语义。
4. 给调用方提供带显式回调的 OpenWidget 接入方式；系统调用方在打开前检查 IsActive，关闭时先声明 widgets 类型数组再调用 CloseWidget。
5. 业务任务、异步数据缓存和取消由调用方处理；产品需要取消按钮时只反馈取消意图，不在页面内部终止未知任务。

开发结果说明显示接口、输入范围和调用方责任，不增加自动启动系统配置。[参考实现](references/gamecli-example.md) 记录旧布局和动画细节，不作为新产品的路径或版本锁。
