# UI Studio 系统设计

底部“预制控件 / 系统模板 / 系统设计”共用可拖动高度的窗口。系统设计中的聊天按项目存储在 `gamecli_ui.design_chat`。发送需求只产生方案，不修改项目。消息中的“应用到关系图”将具体操作保存为项目新版本；必须匹配生成方案时的项目版本。有未保存草稿时，先保存并重新讨论。

## 设计助手

本机安装并登录 Codex CLI 后，服务通过 `codex exec` 生成结构化方案。可用 `GAMECLI_CODEX_BIN` 指定 codex.exe 的绝对路径。使用独立临时工作目录、只读沙箱、关闭 shell/apps/multi_agent/hooks、忽略用户 MCP 配置；不执行项目开发。参考文件支持 txt/md/json/csv/cs，最多 4 个、总计 100,000 字符。发送时会将当前项目的节点摘要、标准模板目录、近期对话及参考文本交给本机 Codex 所连接的模型。

每个项目同时只能生成一条回复；可停止；三分钟超时。数据库保留失败/停止记录。进程重启后未完成记录最迟五分钟标记失败。不会重放旧操作，不存在的模板只在讨论中标记待开发。新系统内部按钮的实例 ID 在添加后才能确定，需要下一轮补充连线。

官方接入说明：[Codex 非交互模式](https://developers.openai.com/codex/noninteractive)。本机 CLI 需要支持 `--ignore-user-config` 与 `--output-schema`。

## 接口

所有请求必须明确项目 ID，并经过项目成员权限校验。

- `GET /api/studio/projects/:id/design-chat`：按时间返回最近 100 轮对话。
- `POST` 同一路径，JSON 包含 projectId、command：
  - send：text、projectVersion、可选 attachments（name/content 数组）。立即返回运行 ID；轮询 GET 获取回复。
  - stop：turnId。
  - apply：turnId、projectVersion。只应用服务器已保存的方案，不能由客户端替换操作。
- MCP `gamecli_ui_design_chat({projectId})` 读取对话；`gamecli_ui_design_message` 提供上述 send/stop/apply。
- MCP `gamecli_ui_compose({projectId,projectVersion,operations})` 保留基础编排能力；HTTP POST `/composition` 仅接受 command=apply。

编排操作：add（key/templateId/version）、remove（系统根 id）、connect（sourceId/targetId/action）、arrange。仅允许 show/navigate/toggle，关闭留在窗口内部。设计助手的 UI 连线必须使用真实按钮 ID；逻辑系统可用系统根 ID。

## 已移除的旧流程

关系区只显示只读任务开发状态；没有检查、提交确认、审核、下载、刷新按钮。原 submit_work/work MCP 工具、composition GET/submit/approve 接口及任务包生成流程已删除。旧 composition_work 历史表不再使用，保留数据不做破坏性删除。当前尚未接入开发执行器，状态明确显示“未启动”，不把聊天生成或应用方案误报为开发执行。
