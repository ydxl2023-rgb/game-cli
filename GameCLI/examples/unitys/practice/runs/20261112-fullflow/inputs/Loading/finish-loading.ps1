$ErrorActionPreference='Stop'
$cli='G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
$project='G:/Works/GameCLI/GameCLI/examples/unitys/practice/client'
$asset='Assets/Data/Widgets/Loading/Loading.prefab'
function Invoke-Cli([string[]]$tokens) {
 $out=& $cli --project $project --format json @tokens
 Add-Content -LiteralPath "$PSScriptRoot/loading-cli.jsonl" -Value $out
 $result=$out|ConvertFrom-Json
 if(!$result.ok){throw $result.message}
}
Invoke-Cli @('prefab','restore','--json',"$PSScriptRoot/Loading.snapshot.json",'--prefab',$asset,'--replace','--dry-run')
Invoke-Cli @('prefab','restore','--json',"$PSScriptRoot/Loading.snapshot.json",'--prefab',$asset,'--replace')
Invoke-Cli @('widget-metadata','UILoading','--vm','VMLoading','--prefab',$asset,'--dry-run')
Invoke-Cli @('widget-metadata','UILoading','--vm','VMLoading','--prefab',$asset)
Invoke-Cli @('animator','clip','import-json','--clip','Assets/Data/Widgets/Loading/LoadingLoop.anim','--json',"$PSScriptRoot/LoadingLoop.clip.json",'--dry-run')
Invoke-Cli @('animator','clip','import-json','--clip','Assets/Data/Widgets/Loading/LoadingLoop.anim','--json',"$PSScriptRoot/LoadingLoop.clip.json")
Invoke-Cli @('animator','controller','create','--controller','Assets/Data/Widgets/Loading/Loading.controller','--dry-run')
Invoke-Cli @('animator','controller','create','--controller','Assets/Data/Widgets/Loading/Loading.controller')
Invoke-Cli @('animator','state','add','--controller','Assets/Data/Widgets/Loading/Loading.controller','--state','Loading_Anim_01','--clip','Assets/Data/Widgets/Loading/LoadingLoop.anim')
Invoke-Cli @('components','add','--asset',$asset,'--path','Loading/Animator','--component','Animator')
Invoke-Cli @('modify','--asset',$asset,'--path','Loading/Animator','--component','Animator','--property','m_Controller','--value','Assets/Data/Widgets/Loading/Loading.controller')
Invoke-Cli @('console','--type','error','--count','20')
