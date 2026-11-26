$ErrorActionPreference = 'Stop'
$project = [IO.Path]::GetFullPath("$PSScriptRoot/../../client")
$cli = "$project/Library/UnityCLI/UnityCLI.exe"
$records = [Collections.Generic.List[object]]::new()
function Bridge([string[]]$arguments) {
    $raw = & $cli --project $project --format json @arguments
    $result = ($raw -join "`n") | ConvertFrom-Json
    $records.Add(@{arguments=$arguments;result=$result})
    $records | ConvertTo-Json -Depth 35 | Set-Content "$PSScriptRoot/second-evidence.json"
    if (!$result.ok) { throw ($raw -join "`n") }
    return $result
}
function Invoke([string]$method,[string]$arguments='[]') {
    return Bridge @('call','--invoke-type','MiniwowOpeningProbe','--invoke-member',$method,'--args-json',$arguments)
}
function Snapshot { return (Invoke 'Snapshot').data.returnSummary | ConvertFrom-Json }
function Require($condition,[string]$message) {
    if (!$condition) { throw "Second rescue failed: $message" }
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
Require ($state.phase -eq 'FetchSecondWater' -and $state.task -eq 60002 -and $state.first -eq 'Claimed') 'Existing 60001 claim enters 60002 without replaying the movie'
Require ($state.hotelDamaged -and $state.hospitalDamaged -and !$state.hotelOwnerVisible) 'Both second-task fires retain original damaged models'
Invoke 'Click' '["RescueClick"]' | Out-Null
$state=WaitPhase 'CarryHotelWater'
Require ($state.water -eq 10 -and $state.second -eq 'Active') 'Original second well route collects ten water'
Invoke 'Click' '["RescueClick"]' | Out-Null
$state=WaitPhase 'HotelThanks'
Require ($state.water -eq 5 -and $state.second -eq 'Active') 'Hotel consumes five without completing task 60002'
Require ($state.hotelRescued -and !$state.hotelDamaged -and $state.hotelOwnerVisible -and $state.hospitalDamaged) 'Hotel extinguishes and owner appears while hospital remains damaged'
Restart
$state=Snapshot
Require ($state.phase -eq 'HotelThanks' -and $state.water -eq 5 -and $state.hotelRescued) 'Hotel dialogue and remaining five water survive full restart'
Invoke 'Click' '["ContinueClick"]' | Out-Null
$state=WaitPhase 'CarryHospitalWater'
Invoke 'Click' '["RescueClick"]' | Out-Null
Start-Sleep -Milliseconds 500
Bridge @('call','--invoke-type','MiniwowTaskProbe','--invoke-member','Return') | Out-Null
Start-Sleep -Seconds 2
Bridge @('call','--invoke-type','MiniwowTaskProbe','--invoke-member','EnterGame') | Out-Null
Start-Sleep -Seconds 2
$state=Snapshot
Require ($state.phase -eq 'CarryHospitalWater' -and $state.water -eq 5 -and $state.hospitalDamaged) 'Interrupted hospital movement consumes no water and preserves hotel result'
Invoke 'Click' '["RescueClick"]' | Out-Null
$state=WaitPhase 'SecondClaim'
Require ($state.water -eq 0 -and $state.second -eq 'Completed' -and $state.task -eq 60002) 'Hospital consumes remaining five and task awaits explicit claim'
Require ($state.hospitalRescued -and !$state.hospitalDamaged -and $state.hotelRescued) 'Hospital switches specifically to original priest_initial_2'
Invoke 'Capture' | Out-Null
Start-Sleep -Milliseconds 500
Copy-Item "$project/Library/OpeningQA/full-resolution.png" "$PSScriptRoot/second-completed.png" -Force
Restart
$state=Snapshot
Require ($state.phase -eq 'SecondClaim' -and $state.second -eq 'Completed' -and $state.hospitalRescued) 'Unclaimed second task and both scene results survive restart'
Invoke 'Click' '["ClaimClick"]' | Out-Null
$state=WaitPhase 'ClearDebris'
Require ($state.task -eq 60003 -and $state.second -eq 'Claimed') 'Explicit claim advances to 60003'
Restart
$state=Snapshot
Require ($state.phase -eq 'ClearDebris' -and $state.task -eq 60003 -and $state.water -eq 0 -and $state.hospitalRescued) 'Claimed 60002 restores without repeating rescue'
$console=Bridge @('console','--type','error','--count','100')
Require ($console.data.totalCount -eq 0) 'No Unity runtime errors'
