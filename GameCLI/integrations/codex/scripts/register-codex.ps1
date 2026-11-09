$ErrorActionPreference = 'Stop'
$studioRoot = Split-Path -Parent $PSScriptRoot
$entry = Join-Path $studioRoot 'server/stdio.mjs'
if (-not (Test-Path -LiteralPath (Join-Path $studioRoot 'dist/studio.html'))) {
    throw '请先在 integrations/codex 执行 npm ci 和 npm run build。'
}
$nodeCommand = Get-Command node.exe -ErrorAction Stop
$codexCommand = Get-Command codex -ErrorAction Stop
& $codexCommand.Source mcp add gamecli-ui-studio -- $nodeCommand.Source $entry
if ($LASTEXITCODE -ne 0) { throw 'MCP 注册失败。' }
& $codexCommand.Source mcp get gamecli-ui-studio
if ($LASTEXITCODE -ne 0) { throw 'MCP 注册验证失败。' }
Write-Host '已注册 gamecli-ui-studio。重新加载 MCP 后调用 gamecli_ui_open。'
