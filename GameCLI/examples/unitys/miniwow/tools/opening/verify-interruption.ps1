$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath("$PSScriptRoot/../../client")
$cli="$project/Library/UnityCLI/UnityCLI.exe"
$records=[Collections.Generic.List[object]]::new()
function Bridge([string[]]$arguments){
 $raw=& $cli --project $project --format json @arguments
 $r=($raw -join "`n")|ConvertFrom-Json
 $records.Add(@{arguments=$arguments;result=$r})
 $records|ConvertTo-Json -Depth 35|Set-Content "$PSScriptRoot/interruption-evidence.json"
 if(!$r.ok){throw ($raw -join "`n")}
 return $r
}
function Call([string]$type,[string]$method,[string]$arguments='[]'){
 return Bridge @('call','--invoke-type',$type,'--invoke-member',$method,'--args-json',$arguments)
}
function State {return (Call 'MiniwowOpeningProbe' 'Snapshot').data.returnSummary|ConvertFrom-Json}
function Cleanup {
 $s=(Call 'MiniwowOpeningProbe' 'CleanupSnapshot').data.returnSummary|ConvertFrom-Json
 if($s.movies -ne 0 -or $s.overlays -ne 0 -or $s.scenes -ne 0 -or $s.runtimes -ne 0 -or $s.textures -ne 0){throw "Cleanup failed: $($s|ConvertTo-Json -Compress)"}
 Write-Output 'PASS: Home video, texture, overlay, scene and runtime released'
}
Bridge @('asset','refresh')|Out-Null
Bridge @('compile','--wait')|Out-Null
Bridge @('run','play')|Out-Null
Start-Sleep -Seconds 2
Call 'MiniwowHomeProbe' 'SetResolution' '[1080,1920]'|Out-Null
Call 'UnityEngine.SceneManagement.SceneManager' 'LoadScene' '["App"]'|Out-Null
Start-Sleep -Seconds 2
Call 'MiniwowOpeningProbe' 'ResetLocalSave'|Out-Null
Call 'MiniwowTaskProbe' 'EnterGame'|Out-Null
Start-Sleep -Seconds 3
$s=State
if($s.phase -ne 'Movie' -or !$s.playing){throw 'Movie did not start'}
Call 'MiniwowTaskProbe' 'Return'|Out-Null
Start-Sleep -Seconds 1
Cleanup
Call 'MiniwowTaskProbe' 'EnterGame'|Out-Null
Start-Sleep -Seconds 2
$s=State
if($s.phase -ne 'Movie' -or !$s.playing){throw 'Interrupted movie did not resume'}
Write-Output 'PASS: Interrupted movie remains uncompleted and can replay'
Call 'MiniwowOpeningProbe' 'Click' '["SkipClick"]'|Out-Null
Call 'MiniwowOpeningProbe' 'Click' '["ConfirmClick"]'|Out-Null
Call 'MiniwowOpeningProbe' 'Click' '["ContinueClick"]'|Out-Null
Call 'MiniwowOpeningProbe' 'WalkToWater'|Out-Null
Start-Sleep -Seconds 2
$s=State
if($s.water -ne 10){throw 'Water was not collected'}
Call 'MiniwowOpeningProbe' 'Click' '["RescueClick"]'|Out-Null
Call 'MiniwowTaskProbe' 'Return'|Out-Null
Start-Sleep -Seconds 1
Cleanup
Call 'MiniwowTaskProbe' 'EnterGame'|Out-Null
Start-Sleep -Seconds 2
$s=State
if($s.phase -ne 'CarryWater' -or $s.water -ne 10 -or $s.first -ne 'Active'){throw 'Interrupted rescue consumed or completed prematurely'}
Write-Output 'PASS: Interrupted rescue keeps ten water and active task'
Call 'MiniwowOpeningProbe' 'Click' '["RescueClick"]'|Out-Null
Start-Sleep -Seconds 5
$s=State
if($s.phase -ne 'Thanks' -or $s.water -ne 0 -or $s.first -ne 'Completed'){throw 'Rescue did not recover'}
Call 'MiniwowOpeningProbe' 'Capture'|Out-Null
Write-Output 'PASS: Rescue resumes through original navigation and completes normally'
Call 'MiniwowOpeningProbe' 'Click' '["ContinueClick"]'|Out-Null
Call 'MiniwowOpeningProbe' 'Click' '["ClaimClick"]'|Out-Null
Call 'MiniwowOpeningProbe' 'Snapshot'|Out-Null
Bridge @('console','--type','error','--count','100')|Out-Null
Bridge @('run','stop')|Out-Null
