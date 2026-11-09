---
name: ui-studio
description: 在 Codex 中打开 GameCLI UI Studio，编辑并导出网页 UI 布局稿。
---

调用 `gamecli_ui_open` 打开工作台。使用 `gamecli_ui_snapshot` 读取设计和 revision。
保存时携带最新 revision；冲突时读取最新版本，不能覆盖用户未合并的修改。
导出使用 `gamecli_ui_export`，返回本机目录、HTML 和 Studio 草稿。
当前仅支持布局设计；不要把 Studio JSON 叫作 visual-ui.json，不要声称已生成 prefab、接入数据库或启动编排器。
MCP 未注册时，在本插件父目录的父目录找到 `scripts/register-codex.ps1`，使用 pwsh.exe 运行。插件清单本身不注册 MCP。
