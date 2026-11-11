# 标准动画库

数据库表：`gamecli_ui.standard_animations`。每条记录由 `(id, version)` 唯一标识，保存名称、分类、发布状态、完整通用定义、SHA256、创建身份和时间。数据库初始化会建表；发布脚本也可以在事务内创建。预检会回滚，不写数据库。

首个标准为 `dialog.open.scale` v1：220ms、XY 从 0.94 到 1、Z 保持 1、ease-out cubic、不循环、Unscaled Time。完整曲线、关键帧切线/权重、逻辑目标、控制器默认状态、用途和调整指导均在定义内。当前没有附加预览媒体，`preview=null`。

标准不保存窗口路径、项目 GUID 或项目专用 `.anim` / `.controller`。这些是使用方生成的资产；生成的动画片段必须保持标准中全部曲线信息。现有 schema 专门覆盖缩放入场，增加其他曲线类型时需要扩展校验，不能省略未知数据后宣称完整保存。

从 `integrations/codex` 执行：

```powershell
node scripts/standard-animation.mjs list
node scripts/standard-animation.mjs read dialog.open.scale 1
node scripts/standard-animation.mjs publish config/animations/dialog.open.scale.v1.json
node scripts/standard-animation.mjs publish config/animations/dialog.open.scale.v1.json --apply
node scripts/standard-animation.mjs bind dialog.open.scale 1 bindings.json output.json
```

`bindings.json` 由具体窗口提供，例如：

```json
{"dialogContent":["Animator/Panel","Empty"]}
```

绑定输出符合工程 UnityCLI 的 `animation-clip/1` 格式，可通过 `animator clip import-json --input output.json --clip Assets/...anim --dry-run` 校验后正式生成。控制器按标准的 `controller` 定义建立 Open 默认状态，绑定使用方的 clip；播放时间模式为 Unscaled Time。背景遮罩不绑定到 dialogContent。

发布需要现有模板管理员身份。同一版本同一内容重复发布是幂等操作；同一版本不同内容会被拒绝，不覆盖历史记录。AI 调整先读取指定版本，修改 spec 和对应 curves，保留来源说明，再保存新版本或新 ID 变体。发布命令在提交前回读完整定义并校验哈希。读取和绑定同样校验哈希；输出文件已存在时拒绝覆盖。

此流程提供数据库和命令行访问，尚未增加动画库页面或 MCP 专用工具。不要将“入库成功”描述为已完成这些界面。
