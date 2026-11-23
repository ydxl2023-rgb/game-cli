---
name: oathx-settings-acceptance
description: 独立验收 Settings 的五页签、偏好校验与保存、滑块事件、账号服务边界、原版动画和模板配置完整性。
---

# Settings 验收

独立复验时按[模板提取与复验](../../../system-development/references/template-fidelity.md)对照来源、示例输入和运行证据，具体判定采用本系统用例。

由未参与实现的 agent 执行。读取[开发契约](../development/SKILL.md)与[开发规范](../../../development-rules/SKILL.md)，核实项目 verified 绑定、模板版本和源码/材料哈希。修改返回开发 agent，同一个 Unity Editor 串行操作。

| 项目 | 必需证据 |
| --- | --- |
| 五页签 | 默认画面；五个真实按钮命中、互斥面板、选中/未选中标签；声音、功能、隐私、下载内容无缺字或重叠，滚动可用。 |
| 质量 | 高中低预设、单项修改、自定义反推；Model 中档拒绝，中档预设映射Model低；全部可见控件有对应选中态。 |
| 声音 | 三滑块百分比、独立静音、恢复未静音音量；程序刷新不重复保存或递归派发；明确区分产品混音器实际生效与偏好记录。 |
| 数据 | 未知枚举、非法浮点/档位、损坏保存；关闭重开及新Entity恢复；只读快照不能改内部；仅修改本系统存储键。 |
| 服务边界 | 账号删除/切换、客服、协议等仅派发有名事件，未接真实服务不能报告成功；隐私、推送和下载不伪造远端生效。 |
| 动画 | 返回栏首次/重开入场、声音页首次/重复进入动画，实际采样而非只看资源存在；无动画目标丢失。 |
| 生命周期 | 反复打开关闭不重复实例/监听；加载期间停用、快速重开；关闭通过Master的SystemDetive和数组CloseWidget。 |
| 发布 | SYS_SETTINGS源表条目及导表结果、原生材料/字体/图片/动画与脚本SHA一致；GameCLIApp实例与标准版本同步，保留用户卡片布局。 |

运行实际编译、Console、verify code 与 verify prefab，保留原始输出。接口静态检查不能代替按钮点击、滚动、视觉和动画证据。探针不得进入模板业务源码。

逐项记录 passed / failed / incomplete / not-applicable；没有证据就是 incomplete。保留截图、JSON、复现步骤、已知差异和最终 Play 状态。验收不授权 Git 提交或数据库发布。

用户确认的模板精简：删除腾讯客服 TxKefu、暖心饭团 RiceRoll、体力 Power、周末福利 Weekend、豪强赛 HeadquartersMatch、极限特训 ExtremeTrain、合宿日常 HomeDaily 的节点、VM 绑定、事件/枚举入口，禁止后续从原始资料重新带回。保留其他未点名设置。偏好总数为 30；保留枚举数字不得因删除而移位，读取旧 JSON 必须逐键忽略废弃项，保留有效音量/画质。原始 source archive 是溯源资料，不是现行交付契约。
