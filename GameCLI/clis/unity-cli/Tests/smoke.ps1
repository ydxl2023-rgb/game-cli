$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot
$hostDll = Join-Path $root 'bin/Debug/net8.0/unity-cli.dll'
function Check($condition, $message) { if (!$condition) { throw $message } }
$output = & dotnet $hostDll --help
Check ($LASTEXITCODE -eq 0) 'Help failed'
Check (($output -join "`n") -match 'UnityPlugin') 'UnityPlugin was not registered'
$output = & dotnet $hostDll console --count 0 --json
Check ($LASTEXITCODE -eq 2) 'Invalid count must exit 2'
Check (($output | ConvertFrom-Json).ok -eq $false) 'Expected JSON error'
$output = & dotnet $hostDll logs --type invalid --json
Check ($LASTEXITCODE -eq 2) 'logs alias validation failed'
$output = & dotnet $hostDll unknown --json
Check ($LASTEXITCODE -eq 2) 'Unknown command must exit 2'
$output = & dotnet $hostDll --plugins-dir unused --json
Check ($LASTEXITCODE -eq 2) 'Dynamic plugin option should not exist'
Write-Output 'PASS: source plugin registration, help, validation, alias and unknown command.'
$output = & dotnet $hostDll compile --help
Check ($LASTEXITCODE -eq 0 -and ($output -join "`n") -match '--wait') 'Compile help failed'
foreach ($invalidArgs in @(@('ping','--wait'), @('console','--wait'), @('compile','--count','2'))) {
    $output = & dotnet $hostDll @invalidArgs --json
    Check ($LASTEXITCODE -eq 2) 'Command-specific option validation failed'
    Check (($output | ConvertFrom-Json).ok -eq $false) 'Expected JSON validation error'
}
Write-Output 'PASS: compile registration, help and command-specific options.'
