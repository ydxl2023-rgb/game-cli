# 场景表接入独立审查

日期：2026-11-25。审查者：独立子 agent town_review。

结论：修订后静态审查通过，未发现本轮新增阻断问题。本审查未操作 Unity、未修改业务代码。

## 独立静态检查

- GamePlugin 在异步读取 SceneConfig.bytes 后检查 startupGeneration；Shutdown 递增代际，旧加载结果不会继续注册配置或加载系统。注册和 Load 调用之间没有异步让出点。
- 初审发现 TryGetValue 可返回 null 行，随后访问 Path 会抛异常。现已确认在访问路径之前添加 !app / !home，缺表、缺行、null 行均进入配置无效分支。
- GamePlugin 与 TownSystem 均使用 IsNullOrWhiteSpace 拒绝空白路径；GamePlugin 另拒绝 App/Home 使用相同路径。
- TownSystem 先检查 sceneConfig，再按 SCENE_HOME/SCENE_APP 别名读取。加载、查询和卸载使用表内 Path，未保留 App/Home 场景路径字面量；单次 Reconcile 捕获配置行后跨等待使用同一对象。
- 生成常量 ID、SceneConfig.bytes 两条记录及 EditorBuildSettings 已启用场景路径一致。readonly 配置字段沿用现有反射注册约定，未改框架或生成器。

## 运行证据归属

主 agent 报告修订后 Unity 编译与启动 Snapshot 成功：Home=true、3 个实体、Idle，控制台 0 错误；此前完整 Town 回归 PASS。以上运行由主 agent 执行，本审查者未独立重跑，也未注入异常配置进行运行测试。

## 审查版本

- GamePlugin.cs SHA256：`0661AF2DEBCC32BB56387780C55835357DF189B08223C0E2B1977BD77B24308D`
- TownSystem.cs SHA256：`514C3EF6DD1DE9DCFC276D1B7EDEE301B386BBFC21AC9A484E8FF348923C6304`
