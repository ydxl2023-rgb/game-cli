param([switch]$Resume)
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../client'))
$cli=Join-Path $PSScriptRoot 'cli/UnityCLI.exe'
$prefab='Assets/Data/Widgets/Home/UIHome.prefab'
$records=[Collections.Generic.List[object]]::new()
function Bridge([string[]]$Arguments) {
 $raw=& $cli --project $project --format json @Arguments
 $r=($raw -join "`n")|ConvertFrom-Json
 $records.Add(@{arguments=$Arguments;result=$r})
 if(!$r.ok){$records|ConvertTo-Json -Depth 35|Set-Content "$PSScriptRoot/binding-evidence.json";throw ($raw -join "`n")}
}
$design=(Get-Content "$PSScriptRoot/uihome-payload.json" -Raw|ConvertFrom-Json).design
$manifest=Get-Content "$PSScriptRoot/production-resources.json" -Raw|ConvertFrom-Json
$lookup=@{};foreach($n in $design.nodes){$lookup[$n.id]=$n}
function NodePath($node) {
 $parts=[Collections.Generic.List[string]]::new();$current=$node
 while($null -ne $current){$parts.Insert(0,$current.name);if($current.parent){$current=$lookup[$current.parent]}else{$current=$null}}
 return $parts -join '/'
}
if(!$Resume){
 Bridge @('components','add','--asset',$prefab,'--path','UIHome','--component','UIHome')
 Bridge @('components','add','--asset',$prefab,'--path','UIHome/SafeArea','--component','HomeSafeArea')
 foreach($node in $design.nodes|Where-Object {$_.parent -eq 'Entries'}){
  $path=NodePath $node
  Bridge @('components','add','--asset',$prefab,'--path',$path,'--component','UIHomeEntry')
  Bridge @('widget-metadata','UIHomeEntry','--vm','VMHomeEntry','--prefab',$prefab,'--path',$path,'--dry-run')
  Bridge @('widget-metadata','UIHomeEntry','--vm','VMHomeEntry','--prefab',$prefab,'--path',$path)
 }
}
Bridge @('widget-metadata','UIHome','--vm','VMHome','--prefab',$prefab,'--dry-run')
Bridge @('widget-metadata','UIHome','--vm','VMHome','--prefab',$prefab)
$entryIds=($design.nodes|Where-Object {$_.parent -eq 'Entries'}|ForEach-Object {$_.id -replace '^Entry',''}) -join ','
Bridge @('modify','--asset',$prefab,'--path','UIHome','--component','UIHome','--property','entryOrder','--value',$entryIds)
Write-Output 'All entry Metadata and root Metadata bound.' 
foreach($font in @('fangzhenglanting','hanyijingkai')){
 if(!(Test-Path "$project/Assets/Data/Widgets/Common/Fonts/$font.ttf")){
  Bridge @('asset','import','--src',([IO.Path]::GetFullPath("$PSScriptRoot/../../extracted-resources/assets/data/fonts/$font.ttf")),'--dst',"Assets/Data/Widgets/Common/Fonts/$font.ttf")
 }
}
foreach($node in $design.nodes|Where-Object type -eq 'Text'){
 $font=if($node.id -match 'Entry40\dLabel|GuildTitle|Power$'){'hanyijingkai'}else{'fangzhenglanting'}
 Bridge @('modify','--asset',$prefab,'--path',(NodePath $node),'--component','Text','--property','m_FontData.m_Font','--value',"Assets/Data/Widgets/Common/Fonts/$font.ttf")
}
Write-Output 'Original fonts bound.'
Bridge @('asset','create-folder','--dst','Assets/Data/Widgets/Common/Icons')
foreach($asset in $manifest|Where-Object {$_.destination -like '*Common/Icons/*'}){
 $filename=[IO.Path]::GetFileName($asset.destination).ToLowerInvariant()
 $source="Assets/Data/Widgets/Home/UIHomeAssets/$filename"
 if(Test-Path "$project/$source"){
  if(!(Test-Path "$project/$($asset.destination)")){Bridge @('asset','move','--src',$source,'--dst',$asset.destination)}
  else {
   if((Get-FileHash -LiteralPath "$project/$source" -Algorithm SHA256).Hash -ne (Get-FileHash -LiteralPath "$project/$($asset.destination)" -Algorithm SHA256).Hash){throw 'Shared icon content collision'}
   foreach($node in $design.nodes|Where-Object image -eq $asset.assetId){
    Bridge @('modify','--asset',$prefab,'--path',(NodePath $node),'--component','Image','--property','m_Sprite','--value',$asset.destination)
   }
   Bridge @('asset','delete','--src',$source)
  }
 }
}
# Feedback blocks scene drags; decorative graphics remain non-raycast targets.
Bridge @('modify','--asset',$prefab,'--path','UIHome/SafeArea/FeedbackVisible','--component','Image','--property','m_RaycastTarget','--value','true')
Bridge @('components','add','--asset',$prefab,'--path','UIHome/SafeArea/MapVisible/MapBackground','--component','UnityEngine.UI.AspectRatioFitter')
Bridge @('modify','--asset',$prefab,'--path','UIHome/SafeArea/MapVisible/MapBackground','--component','AspectRatioFitter','--property','m_AspectMode','--value','4')
Bridge @('modify','--asset',$prefab,'--path','UIHome/SafeArea/MapVisible/MapBackground','--component','AspectRatioFitter','--property','m_AspectRatio','--value','0.5')
Bridge @('modify','--asset',$prefab,'--path','UIHome/SafeArea/FeedbackVisible','--component','Image','--property','m_Color','--value','0,0,0,0.56')
foreach($node in $design.nodes|Where-Object name -eq 'Locked'){
 Bridge @('modify','--asset',$prefab,'--path',(NodePath $node),'--component','Image','--property','m_Color','--value','0.086,0.075,0.051,0.44')
}
Bridge @('save')
$records|ConvertTo-Json -Depth 35|Set-Content "$PSScriptRoot/binding-evidence.json" -Encoding utf8
Write-Output 'UIHome production binding complete.'
