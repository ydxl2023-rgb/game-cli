# 遮罩绘制顺序检查

公告背景遮罩通过显式契约描述，不依赖 `Bg` 名称推测其用途：

```json
{"name":"Bg","role":"backdrop","mustRenderBehind":["Panel"]}
```

为兼容旧 Studio，现有 `role:"background"` 节点也可通过 `mustRenderBehind` 声明相同契约；未声明契约的普通背景不受此规则约束。

`mustRenderBehind` 引用同一父节点下唯一的节点名称（区分大小写），因此模板实例 ID 重映射不改变契约。改名时必须同步修改契约；缺少、重复、自身或非同级目标均报错。已声明的遮罩必须排在目标之前。此版本仅支持同级约束。

- `gamecli verify layout --project <ID> --prefab <根节点ID>` 检查项目数据库中的布局契约，返回 failed / incomplete / passed。
- `gamecli verify prefab ...` 同时检查设计契约和 Unity 导出的一致性。兄弟索引已纳入完整节点路径比较；报告增加 `renderEvidence`，包含显隐、兄弟索引、Graphic 颜色/透明度、射线开关与 Canvas 属性。存在确定差异时 failed 优先于 incomplete。
- 候选发布事务及旧同步入口在更新模板之前执行契约检查，失败时回滚。草稿允许保留错误，便于继续编辑。
- `verify integration` 把项目对应系统的绘制顺序错误纳入“返回修复”，运行通过声明不能豁免结构错误；设计内容和检查结果进入验收指纹，旧签名不能用于新设计。
- `unity-cli ugui` 在 dry-run 和正式生成前验证契约。无论 JSON 和 Unity 是否一致，错误的设计不能通过。
- 旧模板未标注、名称疑似遮罩且位于内容之后时只发出警告；已有其他语义角色的节点不按名称猜测。警告需要人工审核，不自动修改。

## 保留组件的修复

已有 prefab 不必为修正层级重新生成全部组件：

```powershell
unity-cli ugui --input corrected.json --project <Unity目录> --prefab Assets/Data/Widgets/Notice/Notice.prefab --overwrite --repair-layer-order --dry-run --json
unity-cli ugui --input corrected.json --project <Unity目录> --prefab Assets/Data/Widgets/Notice/Notice.prefab --overwrite --repair-layer-order --json
```

修复模式要求正确设计、已有 prefab、明确 backdrop 契约和 Edit Mode。通过 AssetDatabase/PrefabUtility 加载资源，仅调整受契约约束的兄弟顺序，不重新生成图像、Metadata、组件或动画。dry-run 仅修改临时加载对象，卸载后资源不变。存在独立 Canvas 排序时拒绝自动修复，以免错误地把兄弟索引当作最终渲染顺序。额外产品节点予以保留。

更新 CLI 时需要同时更新 Unity 中的 `Assets/Editor/GameCLI/GameCliUguiImporter.cs`。源码位于 `clis/unity-cli/UnityEditor/Editor/`。

## 验收边界

静态层级检查不证明实际 Canvas/相机合成、透明材质、动画动态排序和输入遮挡正确。最终仍需截图、实际 Canvas 排序和 EventSystem 命中证据。存在未知组件或未覆盖项时，原始 prefab 报告保留 incomplete；不能用绑定通过替代视觉验收。

流程保持：检查失败 → 返回修复；检查通过但未运行验收 → 待验收；运行验收通过 → 才能完成。

本次 Practice 修改项目版本 3 → 5，既有按版本签署的验收结果失效，需要重新检查；不沿用旧版本“已完成”。SvNotice 标准修复通过独立草稿提交候选，不自动发布或批量升级其他项目。
