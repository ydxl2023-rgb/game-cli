param()
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath("$PSScriptRoot/../../client")
$cli="$PSScriptRoot/cli/UnityCLI.exe"
$results=[Collections.Generic.List[object]]::new()
function Call([string]$method,[string]$arguments='[]') {
 $raw=& $cli --project $project --format json call --invoke-type MiniwowHomeProbe --invoke-member $method --args-json $arguments
 $response=($raw -join "`n")|ConvertFrom-Json
 if(!$response.ok){throw ($raw -join "`n")}
 $value=$response.data.returnSummary
 $results.Add(@{method=$method;arguments=$arguments;value=$value})
 $results|ConvertTo-Json -Depth 30|Set-Content "$PSScriptRoot/runtime-evidence.json" -Encoding utf8
 return $value
}
Call SetResolution '[1080,1920]'|Out-Null
for($attempt=0;$attempt -lt 30;$attempt++){ $state=Call Snapshot|ConvertFrom-Json; if($state.homeActive -and !$state.launchVisible){break}; Start-Sleep -Milliseconds 100 }
$initial=Call Presentation|ConvertFrom-Json
if($initial.state.homeInstances -ne 1 -or $initial.missingFonts -ne 0 -or $initial.state.launchVisible){throw 'Home startup contract failed'}
$before=Call ListenerCounts
Call RefreshRepeatedly|Out-Null
$after=Call ListenerCounts
if($before -ne $after){throw 'Refresh duplicated listeners'}
if((($after|ConvertFrom-Json)|Where-Object listeners -ne 1).Count){throw 'Entry listener count is not one'}
$ownership=Call CheckEntityOwnership|ConvertFrom-Json
if(!$ownership.isolated -or !$ownership.rejected){throw 'Entity ownership failed'}
Call SetEntryAvailability '[101,false,true]'|Out-Null
$reflow=Call Presentation|ConvertFrom-Json
if(($reflow.entries|Where-Object id -eq 102).position[0] -ne 0){throw 'Activity reflow failed'}
Call SetEntryAvailability '[101,true,true]'|Out-Null
$map=Call ClickEntry '[401]'|ConvertFrom-Json
if(!$map.state.mapVisible){throw 'Map did not open'}
Call Click '["ReturnClick"]'|Out-Null
$locked=Call ClickEntry '[403]'|ConvertFrom-Json
if(!$locked.feedbackVisible -or $locked.feedback -ne '该入口尚未解锁'){throw 'Locked feedback failed'}
Call Click '["CloseFeedbackClick"]'|Out-Null
$pending=Call ClickEntry '[402]'|ConvertFrom-Json
if(!$pending.feedbackVisible -or $pending.feedback -ne '该系统尚未接入'){throw 'Pending destination feedback failed'}
Call Click '["CloseFeedbackClick"]'|Out-Null
Call RapidRestart|Out-Null
Write-Output 'PASS: startup, one listener after 20 refreshes, snapshot ownership, invalid resources, activity reflow, map roundtrip, locked and pending destinations. Rapid restart requested; observe final state separately.'
