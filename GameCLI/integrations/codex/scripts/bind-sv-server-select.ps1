param([string]$Cli = 'G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe')
$ErrorActionPreference='Stop'
$asset='Assets/Data/Widgets/ServerSelect/ServerSelect.prefab'
$root='ServerSelect'
$panel="$root/TopZhuangShi/ServerChooseNode/ServerSelectBg"
function Invoke-Cli([string[]]$Arguments) { $r=& $Cli @Arguments --format json | ConvertFrom-Json; if(!$r.ok){throw ($r|ConvertTo-Json -Depth 5 -Compress)} }
function Has($p,$t){ $r=& $Cli prefab inspect --prefab $asset --recursive --format json | ConvertFrom-Json; return @($r.data.nodes | Where-Object path -eq $p | ForEach-Object components | Where-Object { $_.type -eq $t -or $_.type.EndsWith('.'+$t) }).Count -gt 0 }
function Add($p,$t){if(Has $p $t){return};Invoke-Cli @('components','add','--asset',$asset,'--path',$p,'--component',$t)}
function Remove($p,$t){if(!(Has $p $t)){return};Invoke-Cli @('components','remove','--asset',$asset,'--path',$p,'--component',$t)}
function Set-Field($p,$t,$f,$v){Invoke-Cli @('modify','--asset',$asset,'--path',$p,'--component',$t,'--property',$f,'--value',[string]$v)}
function Metadata($p,$vm,$fields){
 Add $p 'Metadata'; Set-Field $p 'Metadata' 'className' $vm
 Set-Field $p 'Metadata' 'metadatas.Array.size' $fields.Count
 for($i=0;$i -lt $fields.Count;$i++){
  $f=$fields[$i];$prefix="metadatas.Array.data[$i]"
  Set-Field $p 'Metadata' "$prefix.name" $f[0]; Set-Field $p 'Metadata' "$prefix.type" $f[1]
  $targets=@($f[2]);$components=@($f[3])
  Set-Field $p 'Metadata' "$prefix.targets.Array.size" $targets.Count; Set-Field $p 'Metadata' "$prefix.components.Array.size" $targets.Count
  for($j=0;$j -lt $targets.Count;$j++){Set-Field $p 'Metadata' "$prefix.targets.Array.data[$j]" "@$($targets[$j])";Set-Field $p 'Metadata' "$prefix.components.Array.data[$j]" "@$($targets[$j])#$($components[$j])"}
 }
}
$group="$panel/ServerItem";$server="$panel/GridCell"
foreach($p in @($group,$server)){
 Remove $p 'ServerSelectRow'
 Add $p 'UnityEngine.UI.Image'; Set-Field $p 'Image' 'm_Color' '0,0,0,0'
 Add $p 'UnityEngine.UI.Button'; Set-Field $p 'Button' 'm_TargetGraphic' "@$p#Image"
 Remove "$p/Button" 'UnityEngine.UI.Button'; Set-Field "$p/Button" 'Image' 'm_RaycastTarget' 'false'
 Set-Field $p 'RectTransform' 'm_Pivot' '0.5,0.5'
 Add $p 'Oathx.Widget.FancyGridScrollViewCell'
}
Add $group 'UIServerGroupItem'; Add $server 'UIServerItem'
Metadata $group 'VMServerGroupItem' @(
 ,@('title','SyncText',@("$group/Selected/SelectRecentText","$group/Unselected/UnselectRecentText"),@('Text','Text'))
 ,@('selected','SyncVisible',@("$group/Selected"),@('RectTransform'))
 ,@('unselected','SyncVisible',@("$group/Unselected"),@('RectTransform'))
)
$frame="$server/ServerFrameRoot/ServerFrame"
Metadata $server 'VMServerItem' @(
 ,@('serverName','SyncText',@("$frame/ServerName"),@('Text'))
 ,@('serverStatus','SyncImage',@("$frame/ServerStatus"),@('Image'))
 ,@('recommend','SyncVisible',@("$frame/Recommend"),@('RectTransform'))
 ,@('playerHead','SyncVisible',@("$frame/PlayerHead"),@('RectTransform'))
)
foreach($spec in @(@('ServerListGroup',$group,'412,100',1,8),@('ServerInfoGroup',$server,'514,120',2,16))){
 $view="$panel/$($spec[0])/Viewport";$content="$view/Content"
 Remove $view 'UnityEngine.UI.ScrollRect';Remove $content 'UnityEngine.UI.ContentSizeFitter';Remove $content 'UnityEngine.UI.GridLayoutGroup'
 Set-Field $content 'RectTransform' 'm_AnchorMin' '0,0';Set-Field $content 'RectTransform' 'm_AnchorMax' '1,1';Set-Field $content 'RectTransform' 'm_Pivot' '0.5,0.5';Set-Field $content 'RectTransform' 'm_SizeDelta' '0,0';Set-Field $content 'RectTransform' 'm_AnchoredPosition' '0,0'
 Add $view 'Oathx.Widget.FancyGridScrollView'
 Set-Field $view 'Scroller' 'viewport' "@$view#RectTransform"
 Set-Field $view 'FancyGridScrollView' 'cellContainer' "@$content#RectTransform"
 Set-Field $view 'FancyGridScrollView' 'cellPrefab' "@$($spec[1])#FancyGridScrollViewCell"
 Set-Field $view 'FancyGridScrollView' 'cellSize' $spec[2]
 Set-Field $view 'FancyGridScrollView' 'startAxisCellCount' $spec[3]
 Set-Field $view 'FancyGridScrollView' 'startAxisSpacing' '24'
 Set-Field $view 'FancyGridScrollView' 'spacing' $spec[4]
 Set-Field $view 'FancyGridScrollView' 'paddingHead' '22'
}
$gv="$panel/ServerListGroup/Viewport";$sv="$panel/ServerInfoGroup/Viewport"
Add $root 'UIServerSelect'
Set-Field $root 'UIServerSelect' 'serverGrid' "@$sv#FancyGridScrollView"
Metadata $root 'VMServerSelect' @(
 ,@('closeClick','SyncClick',@("$root/Close"),@('Button'))
 ,@('groups','SyncFancyGridScrollView',@($gv),@('FancyGridScrollView'))
 ,@('servers','SyncFancyGridScrollView',@($sv),@('FancyGridScrollView'))
 ,@('groupClick','SyncFancyGridClick',@($gv),@('FancyGridScrollView'))
 ,@('serverClick','SyncFancyGridClick',@($sv),@('FancyGridScrollView'))
 ,@('groupTemplate','SyncVisible',@($group),@('RectTransform'))
 ,@('serverTemplate','SyncVisible',@($server),@('RectTransform'))
)
'Fancy Widget bindings saved.'
