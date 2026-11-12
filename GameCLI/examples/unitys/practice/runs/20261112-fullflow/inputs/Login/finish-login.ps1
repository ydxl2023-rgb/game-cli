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

Add 'SVLogin' 'Metadata'
Modify 'SVLogin' 'Metadata' 'className' 'VMLogin'
Modify 'SVLogin' 'Metadata' 'classGuid' '68a650d213c8d7c42ae59d51dc4f7fcb'
Modify 'SVLogin' 'Metadata' 'metadatas.Array.size' '17'
$fields=@(
 @('enterClick','SyncClick','SVLogin/Animator/Content/Enter','Button'),
 @('canEnter','SyncInteractable','SVLogin/Animator/Content/Enter','Button'),
 @('serverName','SyncText','SVLogin/Animator/Content/Server/Name','Text'),
 @('serverStatus','SyncText','SVLogin/Animator/Content/Server/Status/Status','Text'),
 @('versionText','SyncText','SVLogin/Animator/ButtonGroup/Version','Text'),
 @('username','SyncText','SVLogin/Animator/Account/Username','Text'),
 @('friends','SyncVisible','SVLogin/Animator/Content/Friends','RectTransform'),
 @('announcementClick','SyncClick','SVLogin/Animator/ButtonGroup/LayoutGroup/Announcement','Button'),
 @('changeClick','SyncClick','SVLogin/Animator/Content/Server/Change','Button'),
 @('friendClick','SyncClick','SVLogin/Animator/Content/Friends/FriendButton','Button'),
 @('userCenterClick','SyncClick','SVLogin/Animator/ButtonGroup/LayoutGroup/UserCenter','Button'),
 @('cancellationClick','SyncClick','SVLogin/Animator/ButtonGroup/LayoutGroup/Cancellation','Button'),
 @('clearClick','SyncClick','SVLogin/Animator/ButtonGroup/LayoutGroup/Clear','Button'),
 @('legalClick','SyncClick','SVLogin/Animator/ButtonGroup/LayoutGroup/Legal','Button'),
 @('noticeClick','SyncClick','SVLogin/Animator/NoticeCN/Notice','Button'),
 @('ageIconBtnClick','SyncClick','SVLogin/Animator/AgeIconParent/AgeIconBtn','Button'),
 @('exitClick','SyncClick','SVLogin/Animator/Account/Exit','Button')
)
for($index=0;$index -lt $fields.Count;$index++){
 $f=$fields[$index];$prefix="metadatas.Array.data[$index]"
 Modify 'SVLogin' 'Metadata' "$prefix.name" $f[0]
 Modify 'SVLogin' 'Metadata' "$prefix.type" $f[1]
 Modify 'SVLogin' 'Metadata' "$prefix.targets.Array.size" '1'
 Modify 'SVLogin' 'Metadata' "$prefix.components.Array.size" '1'
 Modify 'SVLogin' 'Metadata' "$prefix.targets.Array.data[0]" ('@'+$f[2])
 Modify 'SVLogin' 'Metadata' "$prefix.components.Array.data[0]" ('@'+$f[2]+'#'+$f[3])
}

