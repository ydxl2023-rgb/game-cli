$ErrorActionPreference = 'Stop'
$cli = Join-Path $PSScriptRoot '../bin/Debug/net8.0/unity-cli.dll'
foreach ($arguments in @(@('animator','clip','create','--duration','NaN'), @('animator','invalid'), @('animator','clip','delete'))) {
    $result = & dotnet $cli @arguments --json | ConvertFrom-Json
    if ($LASTEXITCODE -ne 2 -or $result.ok) { throw 'Animator validation failed' }
}
Write-Output 'PASS: animator argument validation'
