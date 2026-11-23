$ErrorActionPreference='Stop'
$cli='G:/Works/GameCLI/GameCLI/clis/unity-cli/bin/Release/net8.0/unity-cli.exe'
$project='G:/Works/GameCLI/GameCLI/examples/unitys/practice/client'
$file="$PSScriptRoot/config-source/propFunctionSystem.xlsx"
$rows=@(
 @{ID='SYS_LOGIN';NumID='10002';Name='Login';ClassType='LoginSystem'},
 @{ID='SYS_NOTICE';NumID='10009';Name='Notice';ClassType='NoticeSystem'},
 @{ID='SYS_SHOP';NumID='10010';Name='Shop';ClassType='ShopSystem'}
)
foreach($row in $rows){
 $row.TargetType='None';$row.InstallOnStartup='0';$row.Active='0';$row.Condtions='{Switch:1,Level:0}';$row.StartTime='';$row.StopTime='';$row.SceneId='';$row.Version=''
 $json=$row|ConvertTo-Json -Compress
 $args=@('excel','insert','--file',$file,'--header-row','2','--values-json',$json,'--project',$project,'--json')
 foreach($dry in @($true,$false)){
  $call=if($dry){$args+@('--dry-run')}else{$args}
  $output=& $cli @call
  $output|Add-Content "$PSScriptRoot/config-routing-repair.jsonl"
  if(!($output|ConvertFrom-Json).ok){throw $output}
 }
}
