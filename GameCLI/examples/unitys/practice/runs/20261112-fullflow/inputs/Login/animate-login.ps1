$ErrorActionPreference='Stop'
$cli='G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
$project='G:/Works/GameCLI/GameCLI/examples/unitys/practice/client'
$asset='Assets/Data/Widgets/SVLogin/SVLogin.prefab'
$panel='Notice/Animator/Panel'
$item="$panel/ScrollView/Viewport/TextCells/Item"
$body="$item/ExpandContent/Content/Item/MainText"
function Invoke-Cli([string[]]$tokens) {
 $out=& $cli --project $project --format json @tokens
 Add-Content -LiteralPath "$PSScriptRoot/login-cli.jsonl" -Value $out
 $result=$out|ConvertFrom-Json
 if(!$result.ok){throw $result.message}
}
function Modify([string]$path,[string]$component,[string]$property,[string]$value){Invoke-Cli @('modify','--asset',$asset,'--path',$path,'--component',$component,'--property',$property,'--value',$value)}
function Add([string]$path,[string]$component){Invoke-Cli @('components','add','--asset',$asset,'--path',$path,'--component',$component)}

Invoke-Cli @('animator','clip','import-json','--clip','Assets/Data/Widgets/SVLogin/LoginLoop.anim','--json',"$PSScriptRoot/LoginLoop.clip.json",'--dry-run')
Invoke-Cli @('animator','clip','import-json','--clip','Assets/Data/Widgets/SVLogin/LoginLoop.anim','--json',"$PSScriptRoot/LoginLoop.clip.json")
Invoke-Cli @('animator','controller','create','--controller','Assets/Data/Widgets/SVLogin/Login.controller','--dry-run')
Invoke-Cli @('animator','controller','create','--controller','Assets/Data/Widgets/SVLogin/Login.controller')
Invoke-Cli @('animator','state','add','--controller','Assets/Data/Widgets/SVLogin/Login.controller','--state','LoginView_Animation_Loop','--clip','Assets/Data/Widgets/SVLogin/LoginLoop.anim')
Add 'SVLogin/Animator' 'Animator'
Modify 'SVLogin/Animator' 'Animator' 'm_Controller' 'Assets/Data/Widgets/SVLogin/Login.controller'
Invoke-Cli @('console','--type','error','--count','20')
