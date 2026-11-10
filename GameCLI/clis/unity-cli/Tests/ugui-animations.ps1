param([string]$Project = 'G:/Works/GameCLI/GameCLI/examples/unitys/client')
$ErrorActionPreference = 'Stop'
$cli = Join-Path $Project 'Library/UnityCLI/unity-cli.exe'
$integration = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../../integrations/codex'))
$snapshot = Join-Path $Project ('Library/GameCLI/AnimationTests/' + [guid]::NewGuid())
& node (Join-Path (Split-Path $PSScriptRoot) 'Plugins/UguiPlugin/read-template.mjs') $integration SvLoading $snapshot
if ($LASTEXITCODE -ne 0) { throw 'Database snapshot failed.' }
$source = Join-Path $snapshot 'template.json'
$prefab = Join-Path $Project 'Assets/Resources/UI/Loading/Loading.prefab'
$before = (Get-FileHash $prefab).Hash
function RunCase($name, $mutate, $expected) {
    $doc = Get-Content $source -Raw | ConvertFrom-Json -AsHashtable
    & $mutate $doc
    $file = Join-Path $snapshot ($name + '.json')
    $doc | ConvertTo-Json -Depth 80 | Set-Content $file
    $output = & $cli ugui --input $file --assets-dir $snapshot --project $Project --prefab Assets/Resources/UI/Loading/Loading.prefab --overwrite --dry-run --json
    $code = $LASTEXITCODE; $result = $output | ConvertFrom-Json
    if ($result.ok -ne $expected -or ($expected -and $code -ne 0) -or (!$expected -and $code -eq 0)) { throw "$name failed: $output" }
    if ($expected -and $name -eq 'database-animation' -and $result.animationCount -ne 1) { throw 'Animation ignored.' }
    Write-Output "PASS: $name"
}
RunCase 'database-animation' {param($d)} $true
RunCase 'missing-host' {param($d) $d.payload.design.animations[0].hostNodeId='missing'} $false
RunCase 'missing-target' {param($d) $d.payload.design.animations[0].clip.curves[0].targetNodeId='missing'} $false
RunCase 'target-outside-host' {param($d) $d.payload.design.animations[0].clip.curves[0].targetNodeId=$d.payload.root} $false
RunCase 'unsupported-property' {param($d) $d.payload.design.animations[0].clip.curves[0].property='bad'} $false
RunCase 'weighted-curve' {param($d) $d.payload.design.animations[0].clip.curves[0].keys[0].weightedMode=1} $false
RunCase 'unsorted-keys' {param($d) $d.payload.design.animations[0].clip.curves[0].keys[1].time=0} $false
RunCase 'invalid-duration' {param($d) $d.payload.design.animations[0].clip.stopTime=0} $false
RunCase 'legacy-no-animation' {param($d) $d.payload.design.Remove('animations')} $true
if ((Get-FileHash $prefab).Hash -ne $before) { throw 'Dry-run modified prefab.' }
Write-Output 'PASS: animation dry-run preserves prefab'
