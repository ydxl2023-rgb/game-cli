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
Invoke-Cli @('components','remove','--asset',$asset,'--path',$body,'--component','UnityEngine.UI.Text')
Add $body 'NoticeText'
Modify $body 'NoticeText' 'm_FontData.m_FontSize' '28'
Modify $body 'NoticeText' 'm_RaycastTarget' 'true'
Modify $body 'NoticeText' 'm_FontData.m_HorizontalOverflow' '0'
Modify $body 'NoticeText' 'm_FontData.m_VerticalOverflow' '1'
Modify $body 'NoticeText' 'm_FontData.m_Alignment' '0'
Modify $body 'NoticeText' 'm_Color' '0.2,0.2,0.2,1'
Modify 'Notice' 'Metadata' 'className' 'VMNotice'
Modify 'Notice' 'Metadata' 'classGuid' '55d4fe6c677586241ba6e31a5bbe55ad'
$fields=@(
 @('closeClick','SyncClick',"$panel/TopBg/Close",'Button'),
 @('expandClick','SyncClick',"$item/NoticeTitleList/TitleGreen",'Button'),
 @('arrowClick','SyncClick',"$item/NoticeTitleList/TitleArrow",'Button'),
 @('weekClick','SyncClick',"$panel/WeekNomoreShow/ImgFrame",'Button'),
 @('weekAreaClick','SyncClick',"$panel/WeekNomoreShow/WeekNomoreBtn",'Button'),
 @('noticeTitleName','SyncText',"$item/NoticeTitleList/NoticeTitleName",'Text'),
 @('body','SyncText',$body,'NoticeText'),
 @('entry','SyncVisible',$item,'RectTransform'),
 @('empty','SyncVisible',"$panel/Empty",'RectTransform'),
 @('expanded','SyncVisible',"$item/ExpandContent",'RectTransform'),
 @('selected','SyncVisible',"$panel/WeekNomoreShow/ImgFrame/Selected",'RectTransform')
)
for($index=0;$index -lt $fields.Count;$index++){
 $f=$fields[$index];$prefix="metadatas.Array.data[$index]"
 Modify 'Notice' 'Metadata' "$prefix.name" $f[0]
 Modify 'Notice' 'Metadata' "$prefix.type" $f[1]
 Modify 'Notice' 'Metadata' "$prefix.targets.Array.size" '1'
 Modify 'Notice' 'Metadata' "$prefix.components.Array.size" '1'
 Modify 'Notice' 'Metadata' "$prefix.targets.Array.data[0]" ('@'+$f[2])
 Modify 'Notice' 'Metadata' "$prefix.components.Array.data[0]" ('@'+$f[2]+'#'+$f[3])
}
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
Invoke-Cli @('animator','clip','import-json','--clip','Assets/Data/Widgets/Notice/NoticeOpen.anim','--json',"$PSScriptRoot/NoticeOpen.clip.json")
Invoke-Cli @('animator','controller','create','--controller','Assets/Data/Widgets/Notice/Notice.controller','--dry-run')
Invoke-Cli @('animator','controller','create','--controller','Assets/Data/Widgets/Notice/Notice.controller')
Invoke-Cli @('animator','state','add','--controller','Assets/Data/Widgets/Notice/Notice.controller','--state','Open','--clip','Assets/Data/Widgets/Notice/NoticeOpen.anim')
Add 'Notice/Animator' 'Animator'
Modify 'Notice/Animator' 'Animator' 'm_Controller' 'Assets/Data/Widgets/Notice/Notice.controller'
'Notice resource setup complete.'
