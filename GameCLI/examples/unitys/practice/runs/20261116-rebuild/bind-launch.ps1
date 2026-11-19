param(
 [string]$Cli='G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe',
 [string]$Project='G:/Works/GameCLI/GameCLI/examples/unitys/practice/client'
)
$ErrorActionPreference='Stop'
$asset='Assets/Data/Widgets/Launch/Launch.prefab'
$controller='Assets/Data/Widgets/Launch/Launch.controller'
$clip='Assets/Data/Widgets/Launch/Loading_Anim_01.anim'
$identity=Get-Content -LiteralPath (Join-Path $Project 'ProjectSettings/GameCLIProject.json') -Raw|ConvertFrom-Json
if($identity.projectId -ne 'p-9d60fb2d-eb5b-445b-9d1b-25b6a2207484' -or $identity.unityProjectId -ne '44f4f783-645c-41e7-80fe-aead8477c978'){throw 'Unexpected Unity project identity.'}
function Invoke-Cli([string[]]$tokens){
 $out=& $Cli --project $Project --format json @tokens
 $result=$out|ConvertFrom-Json
 if($result.data.endpoint){$result.data.endpoint.token='[REDACTED]'}
 Add-Content -LiteralPath "$PSScriptRoot/launch-cli.jsonl" -Value ($result|ConvertTo-Json -Depth 100 -Compress)
 if(!$result.ok){throw $result.message}
 if($result.data.operation -eq 'export-json'){
  Set-Content -LiteralPath "$PSScriptRoot/Loading_Anim_01.readback.json" -Value $result.data.details[0]
 }
}
function Modify([string]$path,[string]$component,[string]$property,[string]$value){Invoke-Cli @('modify','--asset',$asset,'--path',$path,'--component',$component,'--property',$property,'--value',$value)}
Invoke-Cli @('ping')
Invoke-Cli @('components','add','--asset',$asset,'--path','Launch','--component','Oathx.Core.XLauncher')
$fields=@(
 @('loading','Launch/Animator/Loading',''),
 @('update','Launch/Animator/Update',''),
 @('loadingText','Launch/Animator/Loading/DownloadTips','#Text'),
 @('tip','Launch/Animator/Loading/Tip','#Text'),
 @('downloadText','Launch/Animator/Update/TipsGather/DownloadTips','#Text'),
 @('fill','Launch/Animator/Update/Slider/FillArea/Fill','#RectTransform'),
 @('handle','Launch/Animator/Update/Slider/HandleSlideArea/Handle','#RectTransform')
)
foreach($field in $fields){Modify 'Launch' 'Oathx.Core.XLauncher' $field[0] ('@'+$field[1]+$field[2])}
