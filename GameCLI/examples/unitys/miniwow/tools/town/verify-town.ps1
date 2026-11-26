$ErrorActionPreference = 'Stop'
$project = [IO.Path]::GetFullPath("$PSScriptRoot/../../client")
$cli = "$PSScriptRoot/../home-ui/cli/UnityCLI.exe"
$evidence = [Collections.Generic.List[object]]::new()
function Invoke-Probe([string]$method, [string]$arguments = '[]', [string]$type = 'MiniwowTownProbe') {
    $raw = & $cli --project $project --format json call --invoke-type $type --invoke-member $method --args-json $arguments
    $response = ($raw -join "`n") | ConvertFrom-Json
    if (!$response.ok) { throw ($raw -join "`n") }
    $evidence.Add(@{ method = "$type.$method"; arguments = $arguments; result = $response.data.returnSummary })
    $evidence | ConvertTo-Json -Depth 30 | Set-Content "$PSScriptRoot/runtime-evidence.json" -Encoding utf8
    return $response.data.returnSummary
}
function Wait-State([string]$phase) {
    for ($attempt = 0; $attempt -lt 80; $attempt++) {
        $state = Invoke-Probe Snapshot | ConvertFrom-Json
        if ($state.phase -eq $phase -and ($phase -eq 'Absent' -or ($state.ready -and $state.home))) { return $state }
        Start-Sleep -Milliseconds 150
    }
    throw "Town did not reach $phase"
}
Invoke-Probe SetResolution '[1080,1920]' MiniwowHomeProbe | Out-Null
Invoke-Probe RapidRestart | Out-Null
$initial = Wait-State Idle
if ($initial.contexts -ne 1 -or $initial.visitors -ne 1 -or $initial.skeletons.Count -ne 4) { throw 'Invalid scene cardinality' }
if (@($initial.skeletons | Where-Object { !$_.valid -or $_.vertices -le 0 }).Count) { throw 'Skeleton initialization failed' }
if ((Invoke-Probe RejectInvalidIds) -ne 'True') { throw 'Invalid IDs were accepted' }
$drag = Invoke-Probe FrameGesture '["building",true]' | ConvertFrom-Json
if ($drag.accepted) { throw 'Drag activated building' }
$click = Invoke-Probe FrameGesture '["building",false]' | ConvertFrom-Json
if (!$click.accepted -or $click.point[0] -lt 0 -or $click.point[0] -gt 1080 -or $click.point[1] -lt 0 -or $click.point[1] -gt 1920) { throw 'Visible building click failed' }
Start-Sleep -Milliseconds 150
$click.state = Invoke-Probe Snapshot | ConvertFrom-Json
if ($click.state.phase -ne 'Moving' -or ($click.state.skeletons | Where-Object name -eq 'OriginalHeroVisual').animation -ne 'walk') { throw 'Movement animation did not start' }
if ((Invoke-Probe FrameGesture '["building",false]' | ConvertFrom-Json).accepted) { throw 'Repeated visit accepted' }
$arrived = Wait-State Waiting
if ($arrived.arrivals -ne 1) { throw 'Arrival count was not exactly one' }
if (!(Invoke-Probe FrameGesture '["visitor",false]' | ConvertFrom-Json).accepted) { throw 'Visitor return click failed' }
Wait-State Idle | Out-Null
Invoke-Probe ClickEntry '[401]' MiniwowHomeProbe | Out-Null
if ((Invoke-Probe FrameGesture '["building",false]' | ConvertFrom-Json).began) { throw 'Map allowed scene gesture' }
Invoke-Probe Click '["ReturnClick"]' MiniwowHomeProbe | Out-Null
Invoke-Probe FrameGesture '["building",false]' | Out-Null
Invoke-Probe SetActive '[false]' | Out-Null
$absent = Wait-State Absent
if ($absent.contexts -ne 0 -or $absent.home) { throw 'Town teardown leaked scene or page' }
Invoke-Probe SetActive '[true]' | Out-Null
Wait-State Idle | Out-Null
Invoke-Probe RapidRestart | Out-Null
Wait-State Idle | Out-Null
Invoke-Probe ReplaceModule | Out-Null
$replaced = Wait-State Idle
if ($replaced.contexts -ne 1 -or $replaced.visitors -ne 1) { throw 'Module replacement duplicated scene objects' }
Invoke-Probe SetResolution '[1080,2400]' MiniwowHomeProbe | Out-Null
if (!(Invoke-Probe FrameGesture '["building",false]' | ConvertFrom-Json).accepted) { throw 'Tall-screen interaction failed' }
Wait-State Waiting | Out-Null
Invoke-Probe FrameGesture '["visitor",false]' | Out-Null
Wait-State Idle | Out-Null
Invoke-Probe SetResolution '[1080,1920]' MiniwowHomeProbe | Out-Null
Write-Output 'PASS: Spine initialization; framed visible click; drag rejection; repeated request; walk/arrival/return; map input gate; stop during movement; reactivation; rapid restart; module replacement; tall screen.'
