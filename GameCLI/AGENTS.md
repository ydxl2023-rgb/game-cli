# GameCLI development instructions

Before writing, modifying, or reviewing Unity C# code in this directory, read and follow [the development-rules skill](skills/oathx/development-rules/SKILL.md). These are required project conventions confirmed by the user.

For com.oathx.* framework work, start with [the Oathx skill index](skills/oathx/SKILL.md) and read the relevant module skill. Keep all skills for this framework under skills/oathx, with one system per directory. New system skills must link to the shared development-rules skill and require agents to follow it.

For product development based on the Launch, Login, Loading, Master or Notice standard templates, read [the independent system development workflow](skills/oathx/system-development/SKILL.md) and the specific module skill. Templates are reference patterns, not mandatory product behavior. Their historical GameCLI examples are optional references; do not copy business dependencies or install unrelated systems by default.

For initial Oathx development environment setup, use [the framework setup skill](skills/oathx/framework/SKILL.md). It covers package installation, CLI setup, initialization, coding conventions, and acceptance checks for an independent agent.

For independent acceptance of an Oathx system delivery, select the target system’s acceptance/SKILL.md from [the Oathx skill index](skills/oathx/SKILL.md). Each system owns sibling development/SKILL.md and acceptance/SKILL.md under skills/oathx/system/<system-name>; acceptance must use that system’s specific requirements. Use a reviewer distinct from the implementing agent; preserve raw CLI evidence and return defects to development rather than editing the delivery during acceptance.

For a clean GameCLI practice project based on development, use [the practice setup skill](skills/oathx/practice/SKILL.md). Use development's branch packages and retain only the base entry infrastructure, with no business systems.

For Launch work, also read [the launch skill](skills/oathx/system/launch/development/SKILL.md).

For SV Login work, also read [the sv-login skill](skills/oathx/system/sv-login/development/SKILL.md).

For Loading page work, also read [the loading skill](skills/oathx/system/loading/development/SKILL.md).

Use the latest installed PowerShell 7 (`pwsh.exe`) for Windows shell commands. Do not use `powershell.exe` unless the user explicitly requests compatibility testing.

## UI Studio project / Unity identity

For development, code synchronization, or prefab generation initiated from a UI Studio project, first call `gamecli_ui_unity_binding` with that explicit `projectId` and `requireVerified: true` (or validate the project's `/api/studio/projects/{id}/unity-binding` response has `status: verified`). Use only the returned local Unity path. Check `ProjectSettings/GameCLIProject.json` matches both IDs before writing. An unbound, unavailable, or mismatched binding blocks Unity writes; do not infer a target from the project name or the reference engineering directory. Preserve the identity file when moving the Unity project. A standalone task explicitly targeting another Unity project is not implicitly a UI Studio project task.
