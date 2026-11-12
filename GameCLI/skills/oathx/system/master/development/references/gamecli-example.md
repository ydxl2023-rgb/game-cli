# GameCLI 参考实现记录

本文件保存本轮整理前的实现说明，属于参考材料，不是新产品的强制需求或最新数据库状态。仅在维护 GameCLI 示例、复用具体实现细节时读取；可能包含历史目录、版本和绑定数量，使用前核对实际代码与数据库。产品开发入口见 [SKILL](../SKILL.md)。


# Master

必须遵守 [开发规范](../../../../development-rules/SKILL.md)、[Core](../../../../core/SKILL.md)、[Entity](../../../../entity/SKILL.md)。以下工程路径相对 GameCLI 根目录。

## 源码和边界

- 当前实现位于 `examples/unitys/development/client/Assets/Scripts/Systems/Master`：MasterSystem 管理事件和启动流程；MasterEntity 包含 MasterPropertyDef、玩家初始属性及章节读取。
- 公共基类为 `Assets/Scripts/Systems/RuntimeSystem.cs`，封装 dispatcher 加载、卸载及下一帧完成回调。归档时携带源码和 .meta；目标工程已有基类时先比较，不创建重复类型或覆盖不同版本。
- Master 是纯代码系统，不创建 UIMaster、VMMaster、Canvas、Widget Config、空白 prefab 或占位 UI 节点。
- 保留用户确认的 OnActive / OnDetive / Load / Unload / Save / Launch 结构。Launch 卸载 LoginSystem 后仅加载 HomeSystem，不恢复此前的其他业务系统列表。

## 数据库和模板页面

源码清单为 `integrations/codex/config/master-scripts.json`，包含三个 C# 文件及 .meta。标准模板 ID 为 `MasterSystem`，分类 system，分组 master。payload.kind 为 code，root 为 null，design.nodes 为空；源码放 scripts，保留 UTF-8 原文和 SHA256。

首次注册运行 `node scripts/register-master-template.mjs` 预检，用户授权上传后加 `--apply`。已有模板只验证相同内容，不直接覆盖；更新需沿用管理员候选发布流程并递增版本。

系统模板页展示“纯代码”入口，允许查看和下载源码包，可拖入关系区作为逻辑系统卡片，不生成 UI 画布内容。验证数据库回读源码哈希、空节点和模板页展示；仅归档源码时不修改业务行为，不宣称完成登录联调。

## 当前生命周期契约

- MasterSystem 不依赖 RPC 或网络协议。保留 LoginSucceed、LoginFailure，系统切换事件统一使用 SystemActive、SystemDetive，不使用旧名称或保存事件。事件包含 type（RuntimeSystem 类型）和可选 complete（系统步骤完成回调，不代表 UI 加载完成）。
- GamePlugin 按 FunctionSystemConfig 的 InstallOnStartup/Active 配置创建并激活 Master。OnActive 订阅 LoginSucceed、SystemActive、SystemDetive；收到本地登录成功事件后先 Load，成功再 Launch。Load 本身不进入 Home。
- Load 检查 IsActive 和 MasterEntity；已有本地存档则 Deserialize，无存档则 Serialize 默认数据，成功后返回 true，由登录成功处理方法调用 Launch。加载失败返回 false，不覆盖损坏存档。
- OnDetive 在 UnsubscribeAll 后空一行调用 Unload；Unload 通过 dispatcher 卸载 Notice、Fight 和 Home，不自动保存或返回登录页。
- Save 检查激活状态和实体存在后直接写本地存档，返回 bool。
- 不添加 loaded、launched、activation 或替代状态字段。Launch 的异步完成回调仅检查 IsActive，不宣称具有重复请求拦截或跨激活版本取消能力。
- MasterEntity 依赖 Config.Excel.CHAPTER_HOME、Oathx.Entity；RuntimeSystem 依赖 Oathx.Core。不要在归档时打包业务 UI 或修改用户确认的结构。

- LoginSystem 的开始游戏按钮通过 XPluginUtility.PostEvent(new LoginSucceed { pid = ... }) 投递本地事件，无网络请求。Master 停用时取消订阅；数据读取失败不进入 Home。

- Master 是业务系统加载、卸载的统一执行入口。Login 打开公告投递 SystemActive，Notice 关闭投递 SystemDetive，Home 加载 Fight 同样投递 SystemActive 并保留完成回调。各业务系统仅处理自身 OnActive/OnDetive，不直接切换其他系统。Master 拒绝无效类型及针对自身的切换请求。
- SystemActive 的加载延后一帧，避免事件分发遍历 modules 时修改集合。Master 退出 Login 或 Launch 到 Home 时先卸载 Notice；Login.OnDetive 仅清理自身页面。

