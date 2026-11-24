param([Parameter(Mandatory)][string]$Project, [Parameter(Mandatory)][string]$OutputDirectory)
$ErrorActionPreference = 'Stop'
$Project = (Resolve-Path -LiteralPath $Project).Path
New-Item -ItemType Directory -Force $OutputDirectory | Out-Null
$OutputDirectory = (Resolve-Path -LiteralPath $OutputDirectory).Path
$cli = Join-Path $Project 'Library/UnityCLI/UnityCLI.exe'
$name = 'ContractRegression' + [guid]::NewGuid().ToString('N')
$asset = "Assets/UnityCliContractTests/$name.prefab"
$contractPath = Join-Path $OutputDirectory 'contract.json'
$manifestPath = Join-Path $OutputDirectory 'resources.json'
$window = @{
    prefabPath = $asset
    rootNodeId = 'root'
    referenceResolution = @{width=128;height=128}
    previewStates = @(@{id='default';activeNodes=@{root=$true}},@{id='hidden';activeNodes=@{root=$false}})
    nodes = @(@{
        id = 'root'; name = $name; parentId = $null; siblingIndex = 0; active = $true
        localPosition = @(0,0,0); localRotation = @(0,0,0,1); localScale = @(1,1,1)
        rect = @{anchorMin=@(0,0);anchorMax=@(1,1);pivot=@(0.5,0.5);anchoredPosition=@(0,0);sizeDelta=@(0,0)}
        components = @(@{id='group';type='UnityEngine.CanvasGroup';properties=@{m_Alpha=0.7;m_Interactable=1;m_BlocksRaycasts=1}})
    })
    resourceSlots = @(); animations = @(); effects = @()
}
@{schemaVersion='ui-production-contract/1';windows=@($window)} | ConvertTo-Json -Depth 30 | Set-Content -LiteralPath $contractPath -Encoding utf8
$manifest = @{schemaVersion='ui-contract-resources/1';contractFileSha256=(Get-FileHash $contractPath -Algorithm SHA256).Hash.ToLowerInvariant();resources=@();assets=@()}
function Save-Manifest { $manifest | ConvertTo-Json -Depth 20 | Set-Content -LiteralPath $manifestPath -Encoding utf8 }
function Invoke-Build([string]$Case, [bool]$Expected, [string[]]$Extra) {
    $raw = & $cli --project $Project --format json ugui --json $contractPath --asset-manifest $manifestPath @Extra
    $raw | Set-Content -LiteralPath (Join-Path $OutputDirectory "$Case.json") -Encoding utf8
    $response = $raw | ConvertFrom-Json
    if ($response.ok -ne $Expected) { throw "$Case unexpected response: $raw" }
    return $response
}
Save-Manifest
Invoke-Build 'dry-run' $true @('--dry-run','--output',"$OutputDirectory/dry-report") | Out-Null
if ((Get-ChildItem "$OutputDirectory/dry-report" -Filter '*.png').Count -ne 0) { throw 'Dry-run rendered previews' }
if (Test-Path -LiteralPath (Join-Path $Project $asset)) { throw 'Dry-run wrote prefab' }
$manifest.contractFileSha256 = '0' * 64
Save-Manifest
Invoke-Build 'bad-contract-hash' $false @('--dry-run') | Out-Null
$manifest.contractFileSha256 = (Get-FileHash $contractPath -Algorithm SHA256).Hash.ToLowerInvariant()
$manifest.resources = @(@{slotId='unexpected';path='Assets/missing.ttf';sha256=('0'*64)})
Save-Manifest
Invoke-Build 'unexpected-slot' $false @('--dry-run') | Out-Null
$manifest.resources = @()
Save-Manifest
Invoke-Build 'create' $true @('--output',"$OutputDirectory/build-report") | Out-Null
$report = Get-Content -Raw "$OutputDirectory/build-report/build-report.json" | ConvertFrom-Json
if (!$report.passed -or $report.previews.Count -ne 2) { throw 'Unified build report or previews missing' }
foreach($preview in $report.previews) {
    if ((Get-FileHash -LiteralPath $preview.path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $preview.sha256) { throw 'Preview hash mismatch' }
}
$before = (Get-FileHash -LiteralPath (Join-Path $Project "$asset.meta")).Hash
$prefabHash = (Get-FileHash -LiteralPath (Join-Path $Project $asset)).Hash
Invoke-Build 'reject-overwrite' $false @('--dry-run') | Out-Null
Invoke-Build 'replace' $true @('--replace') | Out-Null
if ((Get-FileHash -LiteralPath (Join-Path $Project "$asset.meta")).Hash -ne $before) { throw 'Prefab GUID changed' }
if ((Get-FileHash -LiteralPath (Join-Path $Project $asset)).Hash -ne $prefabHash) { throw 'Repeated build changed prefab bytes' }
Invoke-Build 'cache-warm' $true @('--replace','--output',"$OutputDirectory/build-report") | Out-Null
Invoke-Build 'cache-hit' $true @('--replace','--output',"$OutputDirectory/build-report") | Out-Null
$cached = Get-Content -Raw "$OutputDirectory/build-report/build-report.json" | ConvertFrom-Json
if (!$cached.cacheHit) { throw 'Unchanged build did not hit cache' }
Add-Content -LiteralPath "$OutputDirectory/build-report/default.png" -Value 'corrupt-preview'
Invoke-Build 'cache-output-invalidated' $true @('--replace','--output',"$OutputDirectory/build-report") | Out-Null
$rebuilt = Get-Content -Raw "$OutputDirectory/build-report/build-report.json" | ConvertFrom-Json
if ($rebuilt.cacheHit) { throw 'Changed preview was reused' }
$changed = Get-Content -Raw $contractPath | ConvertFrom-Json
$changed.windows[0].nodes[0].components[0].properties.m_Alpha = 0.6
$changed | ConvertTo-Json -Depth 30 | Set-Content $contractPath -Encoding utf8
$manifest.contractFileSha256 = (Get-FileHash $contractPath -Algorithm SHA256).Hash.ToLowerInvariant()
Save-Manifest
New-Item -ItemType Directory -Force "$OutputDirectory/rollback-report/default.png" | Out-Null
Invoke-Build 'write-rollback' $false @('--replace','--output',"$OutputDirectory/rollback-report") | Out-Null
$rolled = Get-Content -Raw "$OutputDirectory/rollback-report/build-report.json" | ConvertFrom-Json
if ($rolled.rollback -ne 'restored' -or $rolled.stage -ne 'preview') { throw 'Post-save failure did not report rollback' }
if ((Get-FileHash -LiteralPath (Join-Path $Project $asset)).Hash -ne $prefabHash) { throw 'Rollback did not restore prefab bytes' }
if ((Get-FileHash -LiteralPath (Join-Path $Project "$asset.meta")).Hash -ne $before) { throw 'Rollback did not restore GUID metadata' }
Invoke-Build 'cache-input-invalidated' $true @('--replace','--output',"$OutputDirectory/build-report") | Out-Null
$changedReport = Get-Content -Raw "$OutputDirectory/build-report/build-report.json" | ConvertFrom-Json
if ($changedReport.cacheHit) { throw 'Changed contract was reused' }
$prefabHash = (Get-FileHash -LiteralPath (Join-Path $Project $asset)).Hash
$manifest.contractFileSha256 = '0' * 64
Save-Manifest
Invoke-Build 'failure-report' $false @('--replace','--output',"$OutputDirectory/failure-report") | Out-Null
$failure = Get-Content -Raw "$OutputDirectory/failure-report/build-report.json" | ConvertFrom-Json
if ($failure.passed -or $failure.stage -ne 'compile' -or !$failure.error) { throw 'Failure report missing stage/error' }
if ((Get-FileHash -LiteralPath (Join-Path $Project $asset)).Hash -ne $prefabHash) { throw 'Rejected pipeline modified existing prefab' }
"Passed: dry-run has no write, hash rejection, extra slot rejection, generation, overwrite guard, deterministic replacement and GUID preservation. Test asset: $asset" | Set-Content (Join-Path $OutputDirectory 'summary.txt')
Get-Content (Join-Path $OutputDirectory 'summary.txt')
