param([string]$Project='G:/Works/GameCLI/GameCLI/examples/unitys/development/client')
$ErrorActionPreference='Stop'
$cli='G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
$file="$Project/../config/propFunctionSystem.xlsx"
$items=Get-Content "$PSScriptRoot/../config/home-functions.json" -Raw | ConvertFrom-Json
function Invoke-Excel([string[]]$Arguments){
 $result=& $cli --project $Project --format json excel @Arguments --file $file | ConvertFrom-Json
 if(!$result.ok){throw ($result|ConvertTo-Json -Depth 8 -Compress)}
 return $result.data
}
$query=Invoke-Excel @('query','--count','200')
$ids=@($query.rows | ForEach-Object {($_.cells | Where-Object field -eq ID).value})
$rows=@(@{ID='SYS_FUNCTION_OPEN';NumID=10021;Name='FunctionOpen';TargetType='None';ClassType='FunctionManagerSystem';InstallOnStartup=1;Active=1})
foreach($item in $items){
 if($ids -contains $item.alias){continue}
 $switch=if($item.enabled){1}else{0}
 $rows+=@{ID=$item.alias;NumID=$item.id;Name=$item.name;TargetType='None';ClassType='';InstallOnStartup=0;Active=0;Condtions="{Switch:$switch,Level:0}"}
}
foreach($row in $rows){
 if($ids -contains $row.ID){continue}
 $json=$row|ConvertTo-Json -Compress
 Invoke-Excel @('insert','--values-json',$json,'--dry-run') | Out-Null
 Invoke-Excel @('insert','--values-json',$json) | Out-Null
}
foreach($dry in @($true,$false)){
 $arguments=@('set','--key-field','ID','--key','SYS_FUNCTION_OPEN','--field','ClassType','--value','FunctionManagerSystem')
 if($dry){$arguments+='--dry-run'}
 Invoke-Excel $arguments | Out-Null
}
Invoke-Excel @('validate','--key-field','NumID') | ConvertTo-Json -Depth 4
