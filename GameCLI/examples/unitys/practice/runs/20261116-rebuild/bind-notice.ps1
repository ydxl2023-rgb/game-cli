$ErrorActionPreference='Stop'
$cli='G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
$project='G:/Works/GameCLI/GameCLI/examples/unitys/practice/client'
$asset='Assets/Data/Widgets/Notice/Notice.prefab'
$panel='Notice/Animator/Panel'
$item="$panel/ScrollView/Viewport/TextCells/Item"
$body="$item/ExpandContent/Content/Item/MainText"
function Invoke-Cli([string[]]$tokens) {
 $out=& $cli --project $project --format json @tokens
 Add-Content -LiteralPath "$PSScriptRoot/notice-cli.jsonl" -Value $out
 $result=$out|ConvertFrom-Json
 if(!$result.ok){throw $result.message}
}
function Modify([string]$path,[string]$component,[string]$property,[string]$value){Invoke-Cli @('modify','--asset',$asset,'--path',$path,'--component',$component,'--property',$property,'--value',$value)}
function Add([string]$path,[string]$component){Invoke-Cli @('components','add','--asset',$asset,'--path',$path,'--component',$component)}

Modify 'Notice' 'Metadata' 'metadatas.Array.size' '12'


Modify 'Notice' 'Metadata' 'metadatas.Array.data[11].components.Array.data[0]' ('@'+$panel+'/Empty#Text')
Add "$panel/ScrollView" 'UnityEngine.UI.ScrollRect'
Add "$panel/ScrollView/Viewport" 'UnityEngine.UI.RectMask2D'
Modify "$panel/ScrollView" 'ScrollRect' 'm_Content' "@$panel/ScrollView/Viewport/TextCells#RectTransform"
Modify "$panel/ScrollView" 'ScrollRect' 'm_Viewport' "@$panel/ScrollView/Viewport#RectTransform"
Modify "$panel/ScrollView" 'ScrollRect' 'm_Horizontal' 'false'
Modify "$panel/ScrollView" 'ScrollRect' 'm_Vertical' 'true'
Add 'Notice' 'NoticeLayout'
Modify 'Notice' 'NoticeLayout' 'body' "@$body#NoticeText"
Modify 'Notice' 'NoticeLayout' 'expanded' "@$item/ExpandContent#RectTransform"
Modify 'Notice' 'NoticeLayout' 'entry' "@$item#RectTransform"
Modify 'Notice' 'NoticeLayout' 'entries' "@$panel/ScrollView/Viewport/TextCells#RectTransform"
