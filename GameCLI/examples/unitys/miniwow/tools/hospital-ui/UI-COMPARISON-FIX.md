# 医院 UI 对照修复（2026-11-26）

用户提供原作6级医院与本地1级医院截图。本轮修复表现和布局，保留本地实际等级、余额与库存。

## 已定位的根因

1. 原 `e_ui_bg_priest_1.prefab` 不是单个 skeleton：bg_1 使用 idle、bg_2 使用 idle2，另有 NPC_priest_2 独立骨骼。此前只导入 bg_1，遗漏牧师及前景柜台。
2. 原作零库存走空成品区域；此前始终展示药剂详情大卡片。
3. 材料图标、材料架、返回按钮和任务条被重新设计，未按原作层级与比例组织。
4. 新牧师 atlas 页名 NPC_priest.png 与提取文件名 npc_priest.png 大小写不一致。派生 atlas 统一页名，保留源文件；转换前后哈希记录于 imports.json。

## 修改

- 三层 SkeletonGraphic：背景 idle → 牧师 idle → 前景 idle2。根据原场景变换关系建立相对比例，再按截图视口调整整体缩放与构图。
- 房间上方金币、宝石；等级/收入/升级栏；独立收益/时间/容量数据；成品区空态与有库存态；居中材料及数量；原返回箭头。
- 非医院任务不再占用医院底部。医院任务领取仍保留，不改变业务状态。
- 新文案统一来自 propCommonText.xlsx，原有文案ID保留兼容。升级、自动生产业务尚未开发，入口禁用；未伪造原作6级数值。
- 尚未恢复容量的人群图标及原作全部装饰/动画，本轮不宣称逐像素一致。容量仍显示真实数值。

## 重建

collect.py → build.mjs → ../town/export-scene-config.ps1 → design.py → install.ps1 -PageOnly。
正式 UGUI dry-run/write 和 metadata dry-run/write 完成后，由 bind.request=page 驱动原生绑定。等待 bind-result.json 成功，不能在metadata后漏掉Spine绑定。全流程可不重建Home场景入口。

## 验证

Unity编译通过；任务、表达式、医院、商店导表验证通过；本地CodeVerify零error。原作资源来源、用途、输出及转换哈希见 imports.json。

真实进入医院、点击CraftClick、等待生产、CloseClick返回：草药50→49，药剂0→1；空态/库存态截图为 final-empty.png、final-stock.png。测试后重启会话恢复原检查点；保留60105 Active及原等级、经济数据。
