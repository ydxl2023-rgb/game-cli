$ErrorActionPreference = 'Stop'
$project = [IO.Path]::GetFullPath("$PSScriptRoot/../../client")
$cli = "$PSScriptRoot/../home-ui/cli/UnityCLI.exe"
$records = [Collections.Generic.List[object]]::new()
function Probe([string]$method, [string]$type = 'MiniwowRuntimeProbe') {
    $raw = & $cli --project $project --format json call --invoke-type $type --invoke-member $method --args-json '[]'
    $response = ($raw -join "`n") | ConvertFrom-Json
    $records.Add(@{ method = "$type.$method"; result = $response })
    $records | ConvertTo-Json -Depth 30 | Set-Content "$PSScriptRoot/manager-evidence.json" -Encoding utf8
    if (!$response.ok) { throw ($raw -join "`n") }
    return $response.data.returnSummary
}
function Wait-Runtime([bool]$ready) {
    $deadline = [DateTime]::UtcNow.AddSeconds(20)
    do {
        $state = Probe Snapshot | ConvertFrom-Json
        if ($ready -and $state.registered -ne 0 -and $state.instances.Count -eq 1 -and $state.instances[0].state -eq 'Ready') { return }
        if (!$ready -and $state.registered -eq 0 -and $state.instances.Count -eq 0) { return }
        Start-Sleep -Milliseconds 100
    } while ([DateTime]::UtcNow -lt $deadline)
    throw 'Runtime manager did not reach the expected ownership state.'
}
Probe Return MiniwowSessionProbe | Out-Null
Wait-Runtime $false
if ((Probe VerifyFailedStartup) -ne 'True') { throw 'Invalid scene startup was not rejected.' }
Wait-Runtime $false
Probe ClickStart MiniwowSessionProbe | Out-Null
Wait-Runtime $true
if ((Probe VerifyRegistry) -ne 'True') { throw 'Registry identity changed.' }
Probe Return MiniwowSessionProbe | Out-Null
Wait-Runtime $false
Write-Output 'PASS: failed startup cleanup, repeated creation identity, persistent runtime ownership and deferred destruction.'
