# GameCLI development instructions

Before writing, modifying, or reviewing Unity C# code in this directory, read and follow [the development-rules skill](skills/oathx/development-rules/SKILL.md). These are required project conventions confirmed by the user.

For com.oathx.* framework work, start with [the Oathx skill index](skills/oathx/SKILL.md) and read the relevant module skill. Keep all skills for this framework under skills/oathx, with one system per directory. New system skills must link to the shared development-rules skill and require agents to follow it.

For initial Oathx development environment setup, use [the framework setup skill](skills/oathx/framework/SKILL.md). It covers package installation, CLI setup, initialization, coding conventions, and acceptance checks for an independent agent.

For Launch work, also read [the launch skill](skills/oathx/launch/SKILL.md).

For SV Login work, also read [the sv-login skill](skills/oathx/system/sv-login/SKILL.md).

For Loading page work, also read [the loading skill](skills/oathx/system/loading/SKILL.md).

Use the latest installed PowerShell 7 (`pwsh.exe`) for Windows shell commands. Do not use `powershell.exe` unless the user explicitly requests compatibility testing.
