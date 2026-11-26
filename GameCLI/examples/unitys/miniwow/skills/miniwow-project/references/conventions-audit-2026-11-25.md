# 规范整理验收记录

本轮范围：整理既有用户要求，更新 Skill、参考文档及代码检查工具；未修改 Unity 业务代码、资源或配置表，未提交/推送/发布。

## 交付

- [确认台账](../../../../../../skills/oathx/references/development/CONFIRMED-CONVENTIONS.md)：57 项，涵盖工程/资源13项、编码20项、架构16项、Actor/交付8项。每项标明实际落点或人工边界。
- [当前运行时约定](runtime-conventions.md)：移出项目 Skill 尾部的历史累积说明，集中维护当前有效结构；明确废止方案和尚未开发范围。
- 共享 development-rules、Core、Home、Master Skill 更新事件发送、安装/卸载命名、实体必要性和枚举契约。
- development 的 SYSTEMS/CONFIGURATION 修正平行会话状态与强制代次字段的歧义；旧 GamePlugin 比较文档明确为历史分析。
- oathx-code/9 新增 OX024–OX030，实现、规则表、正反例测试及覆盖说明一起更新。

## 本轮实际验证

1. `node --test --test-concurrency=1` 运行 code-verification 与 architecture-verification：15 项通过，0 失败。新增测试表覆盖19种正反场景，既有无效规则文件/UTF-8/作用域/架构用例继续通过。
2. miniwow 全量 `verify code`：policy=oathx-code/9，passed=true，error=0；`verify architecture` passed=true。
3. 修改的五个 Skill 通过 skill-creator quick_validate；UTF-8 模式执行，并移除共享规范入口的 BOM 以保持 frontmatter 可识别。
4. 新增及核心修改文档的相对链接检查无缺失；development 源码漂移检查 current，559 个文件无变化。
5. 本轮没有 Unity C# 修改，不重复运行游戏编译/视觉测试；此前运行结果不能当作本轮重新执行。

代码/架构原始结果：tools/home-ui/conventions-code-verification.json、conventions-architecture-verification.json。工具测试输出位于仓库 GameCLI/clis/code-verify/test-results.log（本地验证日志）。

## 仍需人工判断的六条提示

静态通过不消除 review，也没有为让工具全绿而降级原规则。

|位置|提示|实际情况与处理边界|
|---|---|---|
|GamePlugin.ReadConfigFiles|OX020|空资源返回 false，经配置完成回调处理；属于配置失败协议，后续修改需保留明确失败，不静默进入游戏|
|ActorEntity.Initialize|OX020|初始化拒绝无效输入，供集合原子安装检查；不是单独 ArgumentNullException 包装。是否统一抛异常须按调用契约评估|
|HomeEntity.InitializeActors|OX020|拒绝重复/空角色集合，HomeRuntime 检查 false 后进入失败处理；生命周期状态与输入校验不可只按字段名删除|
|HomeScene.Initialize|OX020|对宿主/实体/序列化引用统一验证，HomeRuntime 检查 false；必需参数判空是否可进一步简化仍为人工复核项，本轮未改游戏行为|
|UIHome.FormatAmount|OX016|仍硬编码“万”单位；不是本轮新增。已登记后续语言表迁移项，不能声称固定显示文案全部完成语言表接入|
|GamePlugin 条件编译|OX017|含 UNITY_EDITOR 本地读取和诊断分支；工具不覆盖所有 Unity defines。Editor 本地成功不代表 Player Bundle 发布已验收|

另外，OX030 仅识别直接 RuntimeSystem 的指定字段；GamePlugin 的 startupGeneration 不在覆盖范围。它用于异步配置回调的关闭后失效控制，不能因新检查无提示就称全工程已无代次字段，也不能无检查地删除。

## 后续维护

新增规范先更新台账，再决定进入共享 Skill 还是产品约定。可靠语法约束进入检测实现和配置；依赖语义的要求保留 review 与验收。新增规则运行正反例与产品复查；遇到误报先修识别边界，不改代码名称逃避检查。不自动将本产品的 HomeReady、场景结构或 Spine 依赖推广到所有模板。

独立审查指出的 Runtime 失败/取消最终 Stopped 契约、OX003 覆盖描述、Home 名称适用范围已修正文档；OX026 对 OtherXCoroutine 的后缀误匹配已改为接收者末段标识符匹配，并增加负例。
