$ErrorActionPreference = 'Stop'
$project = [IO.Path]::GetFullPath("$PSScriptRoot/../../client")
$cli = "$PSScriptRoot/../home-ui/cli/UnityCLI.exe"
$records = [Collections.Generic.List[object]]::new()
function Probe([string]$method, [string]$arguments = '[]', [string]$type = 'MiniwowSessionProbe') {
    $raw = & $cli --project $project --format json call --invoke-type $type --invoke-member $method --args-json $arguments
    $result = ($raw -join "`n") | ConvertFrom-Json
    $records.Add(@{ method = "$type.$method"; arguments = $arguments; result = $result })
    $records | ConvertTo-Json -Depth 30 | Set-Content "$PSScriptRoot/runtime-evidence.json" -Encoding utf8
    if (!$result.ok) { throw ($raw -join "`n") }
    return $result.data.returnSummary
}
function Wait-Phase([string]$phase) {
    $deadline = [DateTime]::UtcNow.AddSeconds(45)
    do {
        $state = Probe Snapshot | ConvertFrom-Json
        if ($state.phase -eq $phase -and (($phase -eq 'Login' -and $state.loginOpen -and $state.startVisible) -or ($phase -eq 'Town' -and $state.homeOpen -and $state.input) -or $phase -eq 'Stopped')) { return $state }
        Start-Sleep -Milliseconds 150
    } while ([DateTime]::UtcNow -lt $deadline)
    throw "Session did not reach $phase."
}
Probe SetResolution '[1080,1920]' MiniwowHomeProbe | Out-Null
$entry = Wait-Phase Login
if ($entry.homeOpen -or $entry.townReady -or $entry.input -or $entry.loginInstances -ne 1) { throw 'Login must precede town startup.' }
Probe VerifyDuplicateActivation | Out-Null
Probe InspectFonts | Out-Null
Probe RenderPreview | Out-Null
Copy-Item "$project/Library/Login/native-preview.png" "$PSScriptRoot/login-ready.png" -Force
try {
    Probe SetInvalidConfiguration '[true]' | Out-Null
    Probe ClickStart | Out-Null
    $failed = Wait-Phase Login
    if (!$failed.failureVisible -or $failed.input) { throw 'Failure did not restore a retryable entry.' }
    if ($failed.loginInstance -ne $entry.loginInstance) { throw 'Failed Home startup replaced the retained Login page.' }
    Probe RenderPreview | Out-Null
    Copy-Item "$project/Library/Login/native-preview.png" "$PSScriptRoot/login-failed.png" -Force
} finally {
    Probe SetInvalidConfiguration '[false]' | Out-Null
}
Probe BeginHomeTransitionCheck | Out-Null
Probe ClickStart | Out-Null
$town = Wait-Phase Town
Probe VerifyHomeTransition | Out-Null
if ($town.loginOpen -or $town.loginInstances -ne 0 -or !$town.townReady) { throw 'Town committed before closing Login.' }
Probe VerifyDuplicateActivation | Out-Null
Probe Return | Out-Null
Wait-Phase Login | Out-Null
Probe CancelEntering '[false]' | Out-Null
Wait-Phase Login | Out-Null
Start-Sleep -Milliseconds 500
$canceled = Probe Snapshot | ConvertFrom-Json
if ($canceled.homeOpen -or $canceled.townReady -or $canceled.input) { throw 'Canceled entry resumed gameplay.' }
Probe CancelEntering '[true]' | Out-Null
Wait-Phase Login | Out-Null
Probe SetResolution '[1080,2400]' MiniwowHomeProbe | Out-Null
Probe RenderPreview | Out-Null
Copy-Item "$project/Library/Login/native-preview.png" "$PSScriptRoot/login-tall.png" -Force
Probe ClickStart | Out-Null
Wait-Phase Town | Out-Null
Probe StopMaster '[false]' | Out-Null
Wait-Phase Stopped | Out-Null
Start-Sleep -Milliseconds 500
$stopped = Probe Snapshot | ConvertFrom-Json
if ($stopped.loginOpen -or $stopped.homeOpen -or $stopped.input -or $stopped.townReady) { throw 'Master stop left owned gameplay active.' }
Probe StopMaster '[true]' | Out-Null
Wait-Phase Login | Out-Null
Probe SetResolution '[1080,1920]' MiniwowHomeProbe | Out-Null
Probe ClickStart | Out-Null
Wait-Phase Town | Out-Null
Write-Output 'PASS: local Login, duplicate entry/activation, failure retry, real Home/Town readiness, return, entering cancellation, Master stop/restart and portrait adaptation.'
