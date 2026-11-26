$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath("$PSScriptRoot/../../client")
$cli="$project/Library/UnityCLI/UnityCLI.exe"
$records=[Collections.Generic.List[object]]::new()
function Bridge([string[]]$arguments) {
    for($attempt=0;$attempt -lt 10;$attempt++) {
        $raw=& $cli --project $project --format json @arguments 2>&1
        $response=$raw -join "`n"
        if($response -match 'endpoint.json.*being used by another process') {
            Start-Sleep -Milliseconds 500
            continue
        }
        $result=$response | ConvertFrom-Json
        if($result.ok -or $result.error.code -ne 'unauthorized'){break}
        Start-Sleep -Milliseconds 500
    }
    $records.Add(@{arguments=$arguments;result=$result})
    $records | ConvertTo-Json -Depth 40 | Set-Content "$PSScriptRoot/village-evidence.json"
    if (!$result.ok) { throw ($raw -join "`n") }
    return $result
}
function Invoke([string]$method,[string]$arguments='[]',[string]$type='MiniwowVillageProbe') {
    return Bridge @('call','--invoke-type',$type,'--invoke-member',$method,'--args-json',$arguments)
}
function Snapshot { return (Invoke 'Snapshot').data.returnSummary | ConvertFrom-Json }
function Require($condition,[string]$message) {
    if (!$condition) { throw "Village acceptance failed: $message" }
    Write-Output "PASS: $message"
}
function Click {
    Invoke 'Click' '["ContinueClick"]' 'MiniwowOpeningProbe' | Out-Null
    Start-Sleep -Milliseconds 200
}
function Capture([string]$name) {
    Invoke 'Capture' '[]' 'MiniwowOpeningProbe' | Out-Null
    Start-Sleep -Milliseconds 300
    Copy-Item "$project/Library/OpeningQA/full-resolution.png" "$PSScriptRoot/village-$name.png" -Force
}
function Enter {
    Bridge @('run','play') | Out-Null
    Start-Sleep -Seconds 2
    Invoke 'SetResolution' '[1080,1920]' 'MiniwowHomeProbe' | Out-Null
    Invoke 'LoadScene' '["App"]' 'UnityEngine.SceneManagement.SceneManager' | Out-Null
    Start-Sleep -Seconds 2
    Invoke 'EnterGame' '[]' 'MiniwowTaskProbe' | Out-Null
    Start-Sleep -Seconds 2
}
function Restart {
    Bridge @('run','stop') | Out-Null
    Start-Sleep -Seconds 1
    Enter
}
Enter
Invoke 'PrepareReplay' | Out-Null
Restart
$s=Snapshot
Require ($s.task -eq 60004 -and $s.save.heroId -eq 0) 'Starts at rescued Garen without ownership'
$gold=$s.save.items.'70001'
Capture 'recruit'
Click
$s=Snapshot
Require ($s.task -eq 60004 -and $s.save.heroId -eq 125 -and !$s.save.recruitmentAcknowledged) 'Recruitment grants hero and waits for obtained-page acknowledgment'
Restart
$s=Snapshot
Require ($s.task -eq 60004 -and $s.save.heroId -eq 125 -and !$s.save.recruitmentAcknowledged) 'Unacknowledged hero acquisition survives restart'
Click
$s=Snapshot
Require ($s.task -eq 60101 -and $s.save.recruitmentAcknowledged -and !$s.built) 'Closing obtained page claims 60004 and preserves unrepaired hospital'
Click
$s=Snapshot
Require ($s.task -eq 60101 -and $s.built -and $s.save.buildings.'60018' -eq 1 -and $s.save.items.'70001' -eq ($gold-280)) 'Repair commits level one and charges exactly 280'
Capture 'hospital'
Restart
$s=Snapshot
Require ($s.built -and $s.save.items.'70001' -eq ($gold-280)) 'Built model and repair cost survive restart'
Click
$s=Snapshot
Require ($s.task -eq 60102 -and $s.save.items.'70001' -eq ($gold-260)) 'Claiming repair grants configured 20 gold'
Click
$s=Snapshot
Require ($s.save.items.'70001' -eq ($gold-290) -and $s.save.items.'77001' -eq 1 -and $s.save.purchases -eq 1) 'Buying consumes 30 gold and grants one herb without reward leakage'
Restart
$s=Snapshot
Require ($s.task -eq 60102 -and $s.save.items.'77001' -eq 1) 'Purchase survives restart without another purchase'
Click
$s=Snapshot
Require ($s.task -eq 60103 -and $s.save.items.'77001' -eq 51 -and $s.save.items.'70001' -eq ($gold-280)) 'Purchase task reward independently grants 50 herbs and 10 gold'
Capture 'craft'
Click
$s=Snapshot
Require ($s.save.items.'77001' -eq 50 -and $s.save.items.'77101' -eq 1 -and $s.save.crafts -eq 1) 'Production consumes one herb and creates one potion'
Restart
$s=Snapshot
Require ($s.task -eq 60103 -and $s.save.items.'77101' -eq 1 -and $s.save.crafts -eq 1) 'Production remains committed once after restart'
Click
for($i=0;$i -lt 35;$i++) {
    $s=Snapshot
    if($s.save.recovery -eq 2){break}
    Start-Sleep -Milliseconds 100
}
Require ($s.task -eq 60104 -and $s.save.recovery -eq 2 -and $s.save.items.'77101' -eq 0) 'Actual actor arrival consumes potion and starts treatment'
Require ($s.actor.phase -eq 'Waiting' -and $s.save.healthPercent -eq 20 -and $s.remaining -gt 0) 'Treatment waits for configured time and does not instantly heal'
Invoke 'Return' '[]' 'MiniwowTaskProbe' | Out-Null
Start-Sleep -Milliseconds 300
$cleanup=(Invoke 'CleanupSnapshot' '[]' 'MiniwowOpeningProbe').data.returnSummary | ConvertFrom-Json
Require ($cleanup.runtimes -eq 0 -and $cleanup.scenes -eq 0) 'Returning during healing releases runtime and scene'
Restart
$s=Snapshot
Require ($s.task -eq 60104 -and $s.save.recovery -eq 3 -and $s.save.healthPercent -eq 100) 'Persisted treatment deadline completes recovery after restart'
Require ($s.save.items.'77101' -eq 0 -and $s.save.hospitalRevenue -eq 50 -and $s.save.heroGold -eq 499950) 'Recovery does not consume or charge twice and revenue stays in hospital'
Capture 'healed'
Click
$s=Snapshot
Require ($s.task -eq 60105 -and @($s.tasks | Where-Object status -ne 'Claimed').Count -eq 0) 'All five tasks are claimed and next task is 60105'
Require ($s.save.items.'70001' -eq ($gold-260) -and $s.save.receipts.Count -eq 4) 'Four rewarded tasks have durable economy receipts'
Restart
$s=Snapshot
Require ($s.task -eq 60105 -and $s.save.items.'70001' -eq ($gold-260) -and $s.built) 'Final rewards, owned hero and building survive restart'
$rules=(Invoke 'ValidateRules').data.returnSummary | ConvertFrom-Json
Require (@($rules.PSObject.Properties | Where-Object { !$_.Value }).Count -eq 0) 'Isolated economy and actor lifecycle regression checks pass'
Require ((Invoke 'RepeatClaims').data.returnSummary -eq 'True') 'Repeated claims through real task events do not grant rewards again'
$errors=Bridge @('console','--type','error','--count','100')
Require ($errors.data.totalCount -eq 0) 'Unity Console has no errors'
Invoke 'RestoreCheckpoint' | Out-Null
Restart
$s=Snapshot
Write-Output "Five-task runtime acceptance completed. Restored player checkpoint at task $($s.task)."
