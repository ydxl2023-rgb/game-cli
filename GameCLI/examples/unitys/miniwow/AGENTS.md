# Miniwow 项目约定

本文件适用于 miniwow 及其子目录。开始本项目的研究、资源整理、设计或开发前，读取 [miniwow-project Skill](skills/miniwow-project/SKILL.md)，再按任务读取其中的专项资料。

- 继承 [GameCLI 工程约定](../../../AGENTS.md)。本目录的 Skill 只维护产品约定，Oathx 框架规范以共享 Skill 为准。
- `client/` 是从干净基线建立的独立 Unity 工程，现已接入 Launch 与 Home。研究、文档或规范任务不授权修改客户端。用户明确要求实现某个系统时，即可在该任务范围内开发，无需再次索取相同授权。
- 原包、提取资源、反编译代码是参考资料；实际产品实现、配置和资源导入必须按项目 Skill 的证据与职责规则处理。
- Windows 命令使用最新已安装的 `pwsh.exe`，不使用 `powershell.exe`。
- 本规范不自动授权 Git 提交、推送、模板发布或原作线上服务调用；按当次用户任务执行。
