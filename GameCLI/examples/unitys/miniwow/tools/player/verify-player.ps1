$ErrorActionPreference = 'Stop'
$project = [IO.Path]::GetFullPath("$PSScriptRoot/../../client")
$cli = "$PSScriptRoot/../home-ui/cli/UnityCLI.exe"
$evidence = [Collections.Generic.List[object]]::new()
function Invoke-Probe([string]$method, [string]$arguments = '[]', [string]$type = 'MiniwowPlayerProbe') {
    $raw = & $cli --project $project --format json call --invoke-type $type --invoke-member $method --args-json $arguments
    $response = ($raw -join "`n") | ConvertFrom-Json
    if (!$response.ok) { throw ($raw -join "`n") }
    $evidence.Add(@{ method = "$type.$method"; arguments = $arguments; result = $response.data.returnSummary })
    $evidence | ConvertTo-Json -Depth 30 | Set-Content "$PSScriptRoot/runtime-evidence.json" -Encoding utf8
    return $response.data.returnSummary
}
function Reset-Town {
    Invoke-Probe RapidRestart '[]' MiniwowTownProbe | Out-Null
    for ($attempt = 0; $attempt -lt 40; $attempt++) {
        $state = Invoke-Probe Snapshot '[]' MiniwowTownProbe | ConvertFrom-Json
        if ($state.ready -and $state.home -and $state.actors -eq 4) { return }
        Start-Sleep -Milliseconds 100
    }
    throw 'Town did not become ready.'
}
Invoke-Probe SetResolution '[1080,1920]' MiniwowHomeProbe | Out-Null
Reset-Town
if ((Invoke-Probe VerifyPointerCapture) -ne 'True') { throw 'Fixed joystick or pointer capture failed.' }
if ((Invoke-Probe VerifySlope) -ne 'True') { throw 'Slope movement failed.' }
if ((Invoke-Probe VerifyCancellation) -ne 'True') { throw 'Focus or pause cancellation failed.' }
if ((Invoke-Probe VerifyRules) -ne 'True') { throw 'Entity or navigation rules failed.' }
Invoke-Probe Begin '[false]' | Out-Null
$moving = Invoke-Probe Snapshot | ConvertFrom-Json
if ($moving.phase -ne 'Moving' -or $moving.animation -ne 'run') { throw 'Player did not enter run.' }
$next = Invoke-Probe Snapshot | ConvertFrom-Json
if ($next.animation -eq 'run' -and $next.trackTime -le $moving.trackTime) { throw 'Stable animation restarted.' }
Invoke-Probe RenderPreview | Out-Null
Copy-Item "$project/Library/Player/native-preview.png" "$PSScriptRoot/player-moving.png" -Force
Invoke-Probe Stop | Out-Null
Start-Sleep -Milliseconds 150
if ((Invoke-Probe VerifyStopped) -ne 'True') { throw 'Release left retained movement.' }
Reset-Town
Invoke-Probe Begin '[true]' | Out-Null
Start-Sleep -Milliseconds 150
$hidden = Invoke-Probe Stop | ConvertFrom-Json
if ($hidden.shapeActive) { throw 'Shape was not hidden during the movement test.' }
if ((Invoke-Probe VerifyStopped) -ne 'True') { throw 'Hidden Shape changed stop behavior.' }
Reset-Town
Invoke-Probe Begin '[false]' | Out-Null
Invoke-Probe ClickEntry '[401]' MiniwowHomeProbe | Out-Null
$map = Invoke-Probe Snapshot | ConvertFrom-Json
Start-Sleep -Milliseconds 200
$mapLater = Invoke-Probe Snapshot | ConvertFrom-Json
if ($map.phase -ne 'Idle' -or ($map.position -join ',') -ne ($mapLater.position -join ',')) { throw 'Map retained player motion.' }
Invoke-Probe Click '["ReturnClick"]' MiniwowHomeProbe | Out-Null
Reset-Town
Invoke-Probe SetResolution '[1080,2400]' MiniwowHomeProbe | Out-Null
Invoke-Probe Begin '[false]' | Out-Null
Invoke-Probe Snapshot | Out-Null
Invoke-Probe Stop | Out-Null
if ((Invoke-Probe VerifyStopped) -ne 'True') { throw 'Tall-screen release failed.' }
Invoke-Probe SetResolution '[1080,1920]' MiniwowHomeProbe | Out-Null
Reset-Town
Start-Sleep -Milliseconds 800
Invoke-Probe RenderPreview | Out-Null
Copy-Item "$project/Library/Player/native-preview.png" "$PSScriptRoot/player-idle.png" -Force
Write-Output 'PASS: manual entity isolation, invalid input, speed, navigation boundary, run/idle, stable animation, camera follow, hidden Shape, map cancellation and tall portrait.'
