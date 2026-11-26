# Miniwow 项目约定

本文件适用于 miniwow 及其子目录。开始本项目的研究、资源整理、设计或开发前，读取 [miniwow-project Skill](skills/miniwow-project/SKILL.md)，再按任务读取其中的专项资料。

- 继承 [GameCLI 工程约定](../../../AGENTS.md)。本目录的 Skill 只维护产品约定，Oathx 框架规范以共享 Skill 为准。
- 修改 Unity 代码前必须阅读 [development 源码学习文档](../../../skills/oathx/references/development/README.md) 及对应章节，执行源码漂移检查，再核对参考方法与目标框架实现；配置和装配任务必读 CONFIGURATION.md。不得按个人偏好另建已有机制。
- `client/` 是从干净基线建立的独立 Unity 工程，现已接入 Launch 与 Home。研究、文档或规范任务不授权修改客户端。用户明确要求实现某个系统时，即可在该任务范围内开发，无需再次索取相同授权。
- 原包、提取资源、反编译代码是参考资料；实际产品实现、配置和资源导入必须按项目 Skill 的证据与职责规则处理。
- Windows 命令使用最新已安装的 `pwsh.exe`，不使用 `powershell.exe`。
- 后续新手流程与游戏循环开发先查 [开发任务数据库](tools/development-tasks/README.md)：`miniwow_resources.development_tasks` 保存执行状态、依赖和验收条件。按工单 ID 查询当前状态，不把本地种子或历史验收当作最新进度；任务建档不等于已开发或自动授权执行全部任务。
- 本规范不自动授权 Git 提交、推送、模板发布或原作线上服务调用；按当次用户任务执行。

- 配置表默认使用数字 ID，禁止自行添加 TASK_10104、TASK_GUIDE_1000001 等字符串 ID/别名常量。语言表可使用字符串 ID；其他配置表仅在用户明确要求时使用。数字 ID 是稳定配置键，不是数组下标；引用与顺序按原数据保留。
