param([Parameter(Mandatory)][string]$Project, [Parameter(Mandatory)][string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$Project = (Resolve-Path -LiteralPath $Project).Path
$cli = Join-Path $Project 'Library/UnityCLI/UnityCLI.exe'
New-Item -ItemType Directory -Force $OutputDirectory | Out-Null
$OutputDirectory = (Resolve-Path -LiteralPath $OutputDirectory).Path
$layout = Get-Content "$PSScriptRoot/task-layout-v2.json" -Raw
$fixture = Join-Path $OutputDirectory 'layout.json'
Set-Content -LiteralPath $fixture -Value $layout
$asset = 'Assets/GameAI/Tests/UIPrefabCreateRegression.prefab'
function Invoke-Cli([string[]]$Arguments) {
    $result = & $cli --project $Project --format json @Arguments | ConvertFrom-Json
    if (!$result.ok) { throw ($result | ConvertTo-Json -Depth 8) }
    return $result
}
Invoke-Cli @('prefab','create','--json',$fixture,'--prefab',$asset,'--replace','--dry-run') | Out-Null
Invoke-Cli @('prefab','create','--json',$fixture,'--prefab',$asset,'--replace') | Out-Null
$inspection = Invoke-Cli @('prefab','inspect','--prefab',$asset,'--recursive')
$inspection | ConvertTo-Json -Depth 20 | Set-Content (Join-Path $OutputDirectory 'inspection.json')
$nodes = $inspection.data.nodes
$viewport = $nodes | Where-Object path -eq 'UIPrefabCreateRegression/Scroll/Viewport'
if ($viewport.components.type -contains 'UnityEngine.UI.Image') { throw 'Viewport has an unwanted Image' }
if ($viewport.components.type -notcontains 'UnityEngine.UI.RectMask2D') { throw 'Viewport mask missing' }
if (($nodes | Where-Object path -like '*/Hidden').activeSelf) { throw 'Initial hidden state lost' }
if (($nodes | Where-Object path -like '*/Stretch').rectTransform -notmatch 'anchorMax=\(1,1\)') { throw 'Stretch lost' }
if ($inspection.data.missingReferenceCount -ne 0 -or $inspection.data.nodeCount -ne 10) { throw 'Hierarchy/reference failure' }
$serialized = Get-Content (Join-Path $Project $asset) -Raw
foreach ($expected in @('m_Interactable: 0','m_enableAutoSizing: 1','m_overflowMode: 1','m_enableWordWrapping: 0')) {
    if (!$serialized.Contains($expected)) { throw "Missing serialized setting: $expected" }
}
foreach ($case in @('unknown','reference','anchor','duplicate')) {
    $bad = $layout | ConvertFrom-Json
    switch ($case) {
        unknown { $bad.root | Add-Member NoteProperty unexpected 1 }
        reference { $bad.root.children[0].content = 'missing' }
        anchor { $bad.root.children[4].anchorMin = @(2,0) }
        duplicate { $bad.root.children[1].name = 'Scroll' }
    }
    $bad | ConvertTo-Json -Depth 20 | Set-Content $fixture
    $result = & $cli --project $Project --format json prefab create --json $fixture --prefab $asset --replace --dry-run | ConvertFrom-Json
    if ($result.ok) { throw "Invalid $case accepted" }
}
# Legacy payloads still use IDs as names and retain their default Viewport Image.
$legacy = @{schemaVersion=1;taskId='legacy';source='regression';width=100;height=100;root=@{id='Legacy';type='Panel';width=100;height=100}}
$legacy | ConvertTo-Json -Depth 10 | Set-Content $fixture
Invoke-Cli @('prefab','create','--json',$fixture,'--prefab',$asset,'--replace','--dry-run') | Out-Null
Set-Content $fixture $layout
Write-Output 'PASS: generated hierarchy, serialized settings, invalid inputs and v1 compatibility'
