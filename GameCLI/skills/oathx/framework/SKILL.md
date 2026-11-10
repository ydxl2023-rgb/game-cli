---
name: framework
description: 为新项目或已有 Unity 项目一次性建立 Oathx 基础开发环境，安装 com.oathx.*、Unity CLI 与 WidgetGenerator，初始化 XApp、插件及 UI，落实编码规范并验证启动。供独立 agent 执行环境搭建任务。
---

# Oathx 初始开发环境

目标是交付能编译、能通过 CLI 操作、能启动最小页面且遵循工程规范的 Unity 工程，不止安装包。此 skill 是环境搭建入口；业务系统仍各用一个目录。本 skill 不自动创建 agent。

必须读取 [开发规范](../development-rules/SKILL.md)。需要运行时初始化时读取 [Core](../core/SKILL.md)、[Widget](../widget/SKILL.md)、[Widget Extension](../widget-extension/SKILL.md)；涉及启动视图时读取 [Launch](../launch/SKILL.md)。框架索引在 [Oathx](../SKILL.md)。

## 输入与定位

独立 agent 的任务需明确目标 Unity 工程绝对路径。GameCLI 根目录可由本 skill 目录向上三级定位。参考工程是其 `examples/unitys/client`，不能默认修改参考工程来代替用户目标工程。

读取任务、目标 AGENTS.md 和已有配置，确定：

- 新建还是接入已有工程、目标 Unity 版本与平台；已有工程以 ProjectVersion.txt 为准。
- GameCLI 源码根目录、七个 Oathx 主干包来源/分支，以及独立 `com.oathx.unitycli` 桥接包来源。
- 可用 Unity Editor 路径、.NET SDK、SVN/Git；需要模板数据库时再确认 Node.js 与该服务的已有连接配置。
- 需要最小本地启动还是完整现有项目启动。默认最小本地启动，不主动接入登录、付费、追踪服务。

当前参考基线是 Unity **2022.3.62f2**，七个包使用 **gamecli** 分支。已有工程不自动升级 Unity、不自动切换包分支。缺失目录可创建；已有目录先识别内容和版本控制状态，不覆盖用户工程。身份、目标路径或私有包来源确实无法确定时才询问；同时继续独立的只读检查。

## 执行顺序

1. 运行 [预检脚本](scripts/Test-OathxEnvironment.ps1)，记录工具、项目、包、CLI 和生成器状态。它只检查，不安装、不连接数据库，也不代表 Unity 运行验证。
2. 按 [包安装](references/packages.md) 准备工程、七个包、manifest 与实际依赖，记录版本和来源。
3. 按 [CLI 安装](references/cli.md) 安装 Editor 桥接、构建客户端和 UGUI 导入器，打开目标 Editor，验证编译与 Console。
4. 按 [初始化](references/initialization.md) 安装生成器，建立最小 XApp/插件/UI/Launch 启动链，保存组件引用，处理已有工程集成。
5. 将目标工程的 AGENTS.md 接入编码规范；原有指令保留。异机交付时携带所需 `skills/oathx` 目录，不写仅本机有效的绝对 skill 链接。
6. 按 [验收](references/acceptance.md) 完成真实编译、CLI、VM 生成、页面和生命周期检查，交付环境报告与未完成项。

不要因一个可选服务不可用放弃已可完成的本地安装。必需包、Editor 或桥接来源缺失时明确阻塞位置，不能虚构安装地址、API 或成功结果。认证失败不绕过权限，也不将 token 写入脚本或报告。

## 重复执行

每个阶段先检查当前状态：已有包验证来源，已有 manifest 只合并所需键，已有 DLL/导入器比较版本和哈希，已有场景保留 GUID/引用。相同输入重复执行不重复创建 XApp、Canvas、EventSystem、组件或监听。

预检示例（使用 PowerShell 7）：

```powershell
pwsh.exe -File <skill>/scripts/Test-OathxEnvironment.ps1 -Project <target-project> -GameCliRoot <gamecli-root> -UnityEditor <Unity.exe> -BridgeSource <com.oathx.unitycli>
```

给独立 agent 的任务可直接写：

> 使用 `skills/oathx/framework/SKILL.md`，在指定目标路径建立 Oathx 初始开发环境。读取现有工程及已提供的包/CLI 来源，完成安装、最小初始化、编码规范接入和实际验收。不要导入无关游戏业务；结果列出版本、配置、验证证据和阻塞项。

此次整理 skill 不等于已在干净机器完成全量安装。后续执行者必须重新验收目标环境。
