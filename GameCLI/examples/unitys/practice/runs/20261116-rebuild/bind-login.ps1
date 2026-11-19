$ErrorActionPreference='Stop'
$cli='G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
$project='G:/Works/GameCLI/GameCLI/examples/unitys/practice/client'
$asset='Assets/Data/Widgets/Login/Login.prefab'
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

Add 'Login' 'Metadata'
Modify 'Login' 'Metadata' 'className' 'VMLogin'
Modify 'Login' 'Metadata' 'classGuid' '68a650d213c8d7c42ae59d51dc4f7fcb'
Modify 'Login' 'Metadata' 'metadatas.Array.size' '9'
$fields=@(
 @('announcementClick','SyncClick','Login/Animator/ButtonGroup/LayoutGroup/Announcement','Button'),
 @('enterClick','SyncClick','Login/Animator/Content/Enter','Button'),
 @('canEnter','SyncInteractable','Login/Animator/Content/Enter','Button'),
 @('serverName','SyncText','Login/Animator/Content/Server/Name','Text'),
 @('serverStatus','SyncText','Login/Animator/Content/Server/Status/Status','Text'),
 @('versionText','SyncText','Login/Animator/ButtonGroup/Version','Text'),
 @('username','SyncText','Login/Animator/Account/Username','Text'),
 @('friends','SyncVisible','Login/Animator/Content/Friends','RectTransform'),
 @('changeClick','SyncClick','Login/Animator/Content/Server/Change','Button')
)

for($index=0;$index -lt $fields.Count;$index++){
 $f=$fields[$index];$prefix="metadatas.Array.data[$index]"
 Modify 'Login' 'Metadata' "$prefix.name" $f[0]
 Modify 'Login' 'Metadata' "$prefix.type" $f[1]
 Modify 'Login' 'Metadata' "$prefix.targets.Array.size" '1'
 Modify 'Login' 'Metadata' "$prefix.components.Array.size" '1'
 Modify 'Login' 'Metadata' "$prefix.targets.Array.data[0]" ('@'+$f[2])
 Modify 'Login' 'Metadata' "$prefix.components.Array.data[0]" ('@'+$f[2]+'#'+$f[3])
}

