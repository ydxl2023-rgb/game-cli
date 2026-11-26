$ErrorActionPreference = 'Stop'
$project = [IO.Path]::GetFullPath("$PSScriptRoot/../../client")
$cli = "$project/Library/UnityCLI/UnityCLI.exe"
$records = [Collections.Generic.List[object]]::new()
function Bridge([string[]]$arguments) {
    $raw = & $cli --project $project --format json @arguments
    $result = ($raw -join "`n") | ConvertFrom-Json
    $records.Add(@{arguments=$arguments;result=$result})
    $records | ConvertTo-Json -Depth 35 | Set-Content "$PSScriptRoot/third-evidence.json"
    if (!$result.ok) { throw ($raw -join "`n") }
    return $result
}
function Invoke([string]$method,[string]$arguments='[]') {
    return Bridge @('call','--invoke-type','MiniwowOpeningProbe','--invoke-member',$method,'--args-json',$arguments)
}
function Snapshot { return (Invoke 'Snapshot').data.returnSummary | ConvertFrom-Json }
function Require($condition,[string]$message) {
    if (!$condition) { throw "Third rescue failed: $message" }
    Write-Output "PASS: $message"
}
function WaitPhase([string]$phase) {
    for ($i=0;$i -lt 25;$i++) {
        $state=Snapshot
        if ($state.phase -eq $phase) { return $state }
        Start-Sleep -Milliseconds 400
    }
    throw "Did not reach $phase : $($state | ConvertTo-Json -Compress)"
}
function Enter {
    Bridge @('run','play') | Out-Null
    Start-Sleep -Seconds 2
    Bridge @('call','--invoke-type','MiniwowHomeProbe','--invoke-member','SetResolution','--args-json','[1080,1920]') | Out-Null
    Bridge @('call','--invoke-type','UnityEngine.SceneManagement.SceneManager','--invoke-member','LoadScene','--args-json','["App"]') | Out-Null
    Start-Sleep -Seconds 2
    Bridge @('call','--invoke-type','MiniwowTaskProbe','--invoke-member','EnterGame') | Out-Null
    Start-Sleep -Seconds 2
}
function Restart {
    Bridge @('run','stop') | Out-Null
    Start-Sleep -Seconds 2
    Enter
}
Enter
$state=Snapshot
Require ($state.phase -eq 'ClearDebris' -and $state.task -eq 60003 -and $state.third -eq 'Active') 'Existing 60003 save enters the beam clearing stage'
Require ($state.hospitalRescued -and !$state.hospitalCleared -and !$state.hospitalInitial) 'Collapsed hospital is retained before clearing'
Invoke 'Click' '["RescueClick"]' | Out-Null
for ($i=0;$i -lt 30;$i++) {
    $state=Snapshot
    if ($state.clearingSmoke) { break }
    Start-Sleep -Milliseconds 200
}
Require ($state.clearingSmoke -and $state.phase -eq 'ClearDebris' -and $state.third -eq 'Active') 'Arriving starts smoke and the three-second operation without completing task'
Require ($state.countdown -match '秒') 'Actual countdown text is visible'
Bridge @('call','--invoke-type','MiniwowTaskProbe','--invoke-member','Return') | Out-Null
Start-Sleep -Seconds 2
$cleanup=(Invoke 'CleanupSnapshot').data.returnSummary | ConvertFrom-Json
Require ($cleanup.scenes -eq 0 -and $cleanup.runtimes -eq 0 -and $cleanup.overlays -eq 0) 'Leaving during clearing releases owned scene, runtime and UI'
Restart
$state=Snapshot
Require ($state.phase -eq 'ClearDebris' -and $state.hospitalRescued -and $state.third -eq 'Active') 'Interrupted clearing does not remove beam or complete task'
Invoke 'Click' '["RescueClick"]' | Out-Null
$state=WaitPhase 'RescuePerson'
Require ($state.hospitalCleared -and !$state.hospitalRescued -and !$state.clearingSmoke -and $state.third -eq 'Active') 'Three seconds clears beam to priest_initial_3 but leaves rescue task active'
Invoke 'Capture' | Out-Null
Start-Sleep -Milliseconds 500
Copy-Item "$project/Library/OpeningQA/full-resolution.png" "$PSScriptRoot/third-cleared.png" -Force
Restart
$state=Snapshot
Require ($state.phase -eq 'RescuePerson' -and $state.hospitalCleared -and $state.third -eq 'Active') 'Cleared beam survives restart and second operation remains pending'
Invoke 'Click' '["RescueClick"]' | Out-Null
Start-Sleep -Milliseconds 400
Bridge @('call','--invoke-type','MiniwowTaskProbe','--invoke-member','Return') | Out-Null
Start-Sleep -Seconds 2
Bridge @('call','--invoke-type','MiniwowTaskProbe','--invoke-member','EnterGame') | Out-Null
Start-Sleep -Seconds 2
$state=Snapshot
Require ($state.phase -eq 'RescuePerson' -and $state.third -eq 'Active' -and $state.hospitalCleared) 'Interrupted rescue navigation preserves cleared beam without false completion'
Invoke 'Click' '["RescueClick"]' | Out-Null
$state=WaitPhase 'AwaitRecruit'
Require ($state.task -eq 60004 -and $state.third -eq 'Claimed') 'One-second rescue completes and automatically claims 60003 without ClaimClick'
Require ($state.hospitalInitial -and !$state.hospitalCleared -and !$state.clearingSmoke) 'Recruitment entry restores original priest_initial and stops smoke'
Invoke 'Capture' | Out-Null
Start-Sleep -Milliseconds 500
Copy-Item "$project/Library/OpeningQA/full-resolution.png" "$PSScriptRoot/third-completed.png" -Force
Restart
$state=Snapshot
Require ($state.phase -eq 'AwaitRecruit' -and $state.task -eq 60004 -and $state.third -eq 'Claimed' -and $state.hospitalInitial) 'Automatic claim and model result survive full restart'
Require (!($state.buttons -contains 'RescueClick') -and !($state.buttons -contains 'ClaimClick')) 'Completed rescue exposes no repeat operation or manual claim button'
$console=Bridge @('console','--type','error','--count','100')
Require ($console.data.totalCount -eq 0) 'No Unity runtime errors'
