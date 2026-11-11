param([string]$Project = 'G:/Works/GameCLI/GameCLI/examples/unitys/development/client')
$ErrorActionPreference = 'Stop'
$cli = Join-Path $Project 'Library/UnityCLI/unity-cli.exe'
$source = Join-Path (Split-Path $PSScriptRoot) 'Examples/ConfirmDialog.template.json'
$scratch = Join-Path $Project ('Library/GameCLI/DatabaseContractTests/' + [guid]::NewGuid())
New-Item -ItemType Directory -Force $scratch | Out-Null
function RunCase($name, $mutate, $expected) {
    $doc = Get-Content $source -Raw | ConvertFrom-Json -AsHashtable
    & $mutate $doc
    $inputFile = Join-Path $scratch ($name + '.json')
    $doc | ConvertTo-Json -Depth 50 | Set-Content $inputFile
    $output = & $cli ugui --input $inputFile --project $Project --prefab Assets/Data/Widgets/DatabaseTemplates/ConfirmDialog.prefab --overwrite --dry-run --json
    $code = $LASTEXITCODE
    $result = $output | ConvertFrom-Json
    if ($result.ok -ne $expected -or ($expected -and $code -ne 0) -or (!$expected -and $code -eq 0)) { throw "$name failed: $output" }
    Write-Output "PASS: $name"
}
$prefab = Join-Path $Project 'Assets/Data/Widgets/DatabaseTemplates/ConfirmDialog.prefab'
$before = (Get-FileHash $prefab).Hash
RunCase 'raw-database-row-legacy-bounds' { param($d) } $true
RunCase 'schema-version' { param($d) $d.payload.design.schemaVersion = 2 } $false
RunCase 'duplicate-node' { param($d) $d.payload.design.nodes[1].id = $d.payload.design.nodes[0].id } $false
RunCase 'invalid-root' { param($d) $d.payload.root = 'missing' } $false
RunCase 'hierarchy-cycle' { param($d) $d.payload.design.nodes[1].parent = $d.payload.design.nodes[1].id } $false
RunCase 'unknown-node-type' { param($d) $d.payload.design.nodes[1].type = 'Unknown' } $false
RunCase 'missing-action-target' { param($d) $d.payload.design.nodes[1].action = @{type='show';target='missing'} } $false
RunCase 'invalid-image-id' { param($d) $d.payload.design.nodes[1].image = '../escape' } $false
RunCase 'preserve-metadata' { param($d) $d.payload.source=@{note='test'}; $d.payload.standard=@{hierarchyVersion=1}; $d.payload.limitations=@('test') } $true
$record = Get-Content $source -Raw | ConvertFrom-Json -AsHashtable
$record.payload | ConvertTo-Json -Depth 50 | Set-Content (Join-Path $scratch 'payload.json')
$output = & $cli ugui --input (Join-Path $scratch 'payload.json') --project $Project --prefab Assets/Data/Widgets/DatabaseTemplates/ConfirmDialog.prefab --overwrite --dry-run --json
if ($LASTEXITCODE -ne 0) { throw "Payload input failed: $output" }
Write-Output 'PASS: direct-payload'
if ((Get-FileHash $prefab).Hash -ne $before) { throw 'Dry-run modified prefab.' }
Write-Output 'PASS: dry-run preserved prefab'
