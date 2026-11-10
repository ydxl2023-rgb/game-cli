#Requires -Version 7.0
<#
.SYNOPSIS
Reports local prerequisites without changing the project or contacting services.
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][string]$Project,
    [Parameter(Mandatory)][string]$GameCliRoot,
    [string]$UnityEditor,
    [string]$BridgeSource
)

$ErrorActionPreference = 'Stop'
$projectRoot = [IO.Path]::GetFullPath($Project)
$sourceRoot = [IO.Path]::GetFullPath($GameCliRoot)
$checks = [Collections.Generic.List[object]]::new()
function Add-Check([string]$Name, [string]$Status, [string]$Detail) {
    $checks.Add([pscustomobject]@{ name = $Name; status = $Status; detail = $Detail })
}
function Read-Json([string]$Path) {
    if (!(Test-Path -LiteralPath $Path -PathType Leaf)) { return $null }
    try { return Get-Content -LiteralPath $Path -Raw | ConvertFrom-Json -AsHashtable }
    catch { Add-Check $Path 'invalid' 'Invalid JSON'; return $null }
}

foreach ($tool in 'pwsh','git','svn','dotnet','node') {
    $command = Get-Command $tool -ErrorAction SilentlyContinue | Select-Object -First 1
    if ($command) { Add-Check $tool 'found' $command.Source }
    else { Add-Check $tool 'missing' 'Locate an installed executable; Node is only required for database templates.' }
}
if ($UnityEditor -and (Test-Path -LiteralPath $UnityEditor -PathType Leaf)) {
    Add-Check 'unityEditor' 'found' ([IO.Path]::GetFullPath($UnityEditor))
} else { Add-Check 'unityEditor' 'unverified' 'Supply the matching Unity.exe path.' }

$versionPath = Join-Path $projectRoot 'ProjectSettings/ProjectVersion.txt'
if (Test-Path -LiteralPath $versionPath) {
    $version = Get-Content -LiteralPath $versionPath | Where-Object { $_ -match '^m_EditorVersion:' }
    Add-Check 'projectVersion' 'found' ($version -join '; ')
} else { Add-Check 'projectVersion' 'missing' 'Create or locate the target Unity project.' }

$manifest = Read-Json (Join-Path $projectRoot 'Packages/manifest.json')
foreach ($suffix in 'avatar','bundle','core','entity','rpc','widget','widget.extension','unitycli') {
    $name = "com.oathx.$suffix"
    $packagePath = Join-Path $projectRoot "LocalPackages/$name/package.json"
    $package = Read-Json $packagePath
    if ($package -and $package.name -eq $name) {
        Add-Check $name 'found' "Local package version $($package.version)"
    } else { Add-Check $name 'missing' "Expected $packagePath (a configured external package may exist)." }
    $expected = "file:../LocalPackages/$name"
    $dependency = if ($manifest -and $manifest.dependencies) { $manifest.dependencies[$name] } else { $null }
    if ($dependency -eq $expected) { Add-Check "$name.manifest" 'matched' $expected }
    elseif ($dependency) { Add-Check "$name.manifest" 'review' 'Nonstandard package reference; verify source and portability.' }
    else { Add-Check "$name.manifest" 'missing' 'Add the package dependency after installation.' }
}
if ($BridgeSource) {
    $bridge = Read-Json (Join-Path ([IO.Path]::GetFullPath($BridgeSource)) 'package.json')
    if ($bridge -and $bridge.name -eq 'com.oathx.unitycli') { Add-Check 'bridgeSource' 'found' "Version $($bridge.version)" }
    else { Add-Check 'bridgeSource' 'invalid' 'The source must contain com.oathx.unitycli/package.json.' }
}

foreach ($file in 'clis/unity-cli/UnityCLI.csproj','clis/unity-cli/UnityEditor/Editor/GameCliUguiImporter.cs','clis/unity-cli/UnityEditor/Editor/GameCliUguiAnimations.cs') {
    Add-Check $file $(if (Test-Path -LiteralPath (Join-Path $sourceRoot $file)) {'found'} else {'missing'}) 'GameCLI source prerequisite'
}
foreach ($file in 'Library/GameCLI/CLI/unity-cli.exe','Library/UnityCLI/UnityCLI.exe','Library/UnityCLI/endpoint.json','Assets/Editor/GameCLI/GameCliUguiImporter.cs','Assets/Editor/GameCLI/GameCliUguiAnimations.cs','Assets/WidgetGenerator.dll','Assets/Resources/XAppConfigure.asset') {
    Add-Check $file $(if (Test-Path -LiteralPath (Join-Path $projectRoot $file)) {'found'} else {'missing'}) 'Presence only; runtime behavior has not been tested.'
}
$analyzerMeta = Join-Path $projectRoot 'Assets/WidgetGenerator.dll.meta'
if (Test-Path -LiteralPath $analyzerMeta) {
    $metadata = Get-Content -LiteralPath $analyzerMeta -Raw
    $guidCount = [regex]::Matches($metadata,'(?m)^guid:').Count
    if ($guidCount -ne 1 -or $metadata -notmatch 'RoslynAnalyzer') {
        Add-Check 'analyzerMetadata' 'review' "Expected one GUID and RoslynAnalyzer label; GUID count=$guidCount."
    } else { Add-Check 'analyzerMetadata' 'found' 'Check actual PluginImporter platform settings in Unity.' }
}

[pscustomobject]@{
    project = $projectRoot
    gameCliRoot = $sourceRoot
    readOnly = $true
    runtimeVerified = $false
    checks = $checks
} | ConvertTo-Json -Depth 6
