---
name: oathx-home-development
description: 独立 agent 参考 SvHome 基础大厅模板开发 Home 的玩家信息、货币、功能入口及角色和季节扩展契约，不携带 SV 网络或活动业务。
---

# Home 开发

先读 [系统开发流程](../../../system-development/SKILL.md)、[编码规范](../../../development-rules/SKILL.md)、[Core](../../../core/SKILL.md)、[Entity](../../../entity/SKILL.md)、[Widget](../../../widget/SKILL.md) 和 [Widget Extension](../../../widget-extension/SKILL.md)。由另一名 agent 按 [Home 验收](../acceptance/SKILL.md) 独立检查。

## 默认范围

读取数据库 SvHome 的实际版本、scripts、configurations 和生产资料。未提出设计变更时保留所选版本的布局、独立图片、入口和行为；不依赖原 SV 工程重新提取。模板是产品开发范式，不把其静态夏季背景或示例数值强加给产品。

首版为用户批准的基础大厅：玩家信息、三组货币、聊天摘要和主要导航入口。用户明确暂缓的是角色和季节动态展示。活动入口、NEW、红点、掉落倍率和商城广告提示属于大厅展示，不得因暂不接运营服务而删除。平台 SDK 和远端运营逻辑不随模板自动安装。源 prefab 的 activeSelf 是编辑状态，不是完整的运行时状态，禁止只提取激活节点。

- 提取时同时核对原版截图、预制、运行时代码及配置换图；默认图片不一定是实际活动图片，Cn/Tw 资源必须区分。截图中的活动集合是可替换的本地示例，不是永久运营日程。
- 按入口独立控制 NEW、红点、是否展示及图片；活动网格须随可见项重排。掉落状态使用 None/Double/Triple，三倍使用其独立原图。商城蓝色广告提示与底部广告轮播是不同展示，不能混用。
- 保留字体文件、字号、BestFit、描边颜色／宽度、材质和 shader 依赖，不使用默认字体静默替代。保存独立生产资料和引用哈希，确保未来 agent 无需原 SV 工程。
- 未完成的字体绑定、状态绑定、动画和运行验证写入候选限制，不发布为已完成标准。导出节点数量增加不等于恢复成功。

## 实现契约

- HomeSystem 管理打开、刷新、关闭；HomeEntity 持有玩家、货币、聊天及展示资源键。所有 VM 放 Home/VM，UIWidget 放 Home/UI，其余代码在 Home 根目录。
- UIHome 使用 Assets/Data/Widgets/Home/Home.prefab。按需获取 vm，不缓存成员；元数据按实际字段生成，逐项检查目标和点击命中。
- 基础大厅必须保留入场动画，不能将其归入暂缓的角色／季节动态。当前 HomeOpen 采用 SV 原版区域滑入及左上信息淡入，保留分段时序，默认 Open 状态不循环；背景保持静止。原生 .anim/.controller、预制 Animator 引用及数据库 design.animations 必须一起交付，确保重新打开及网页预演均播放。曲线完成时回到基础布局，禁止最终位置漂移。
- HomeSystem.Refresh 在实体数据更新后由产品调用；不推测数据来源，不创建登录、不自动进入 Fight、不发 RPC。
- HomeEntryRequested 只表示入口被请求，不能解释为目标系统已打开。产品协调者将其映射为 SystemActive/SystemDetive，由 Master 统一管理系统。Notice 入口是否使用登录公告或游戏公告必须按产品契约确定。
- HomePresentationRequested 携带角色和季节资源键，供产品渲染器接入；空键保持无角色及静态背景。不得将事件存在描述为 Spine 已安装或季节已切换。
- 保持所有已有基础入口可交互；未接业务的入口发明确事件，不统一禁用或变灰。嵌套头像/玩家、聊天频道/聊天按钮必须分别验证命中。
- 固定 Text 文案合并进 propCommonText，使用生成别名与 CommonText.Get。图片内文字是美术本地化依赖，不得声称已支持任意语言。动态昵称、数值和聊天来自 Entity。

## 资料与交付

遵循 [配置包规范](../../../system-development/references/configurations.md)。交付独立源表、结构化条目、代码别名、导表及加载依赖；只合并本系统条目，不覆盖产品整表。

当前提取工具为 integrations/codex/scripts/import-sv-home.mjs；完整 SV 原生导出仅为制作证据，基础版排除路径和原因记录于 report.json。透明 Graphic 不得通过父节点 opacity 隐藏后代；原 UGUI Graphic alpha 不影响子控件。

交付包含版本、工程身份、适配差异、源码/资源哈希、入口映射、配置和运行证据。更新标准须走候选与版本流程；未完成的验收如实标注，不把开发自测当独立验收。

## 红点所有权

真正红点由独立 [RedDotSystem](../../red-dot/development/SKILL.md) 配置和计算，Home 不再在 HomeEntity.indicators 或 UIHome.indicators 中控制红点。宿主根节点通过 RedDotScope 序列化配置 ID 和目标。NEW、广告、掉落及活动入口显隐继续属于 Home。
