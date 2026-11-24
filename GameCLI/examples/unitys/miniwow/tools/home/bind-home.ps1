param([switch]$Resume)
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../client'))
$cli=Join-Path $project 'Library/UnityCLI/UnityCLI.exe'
$records=[Collections.Generic.List[object]]::new()
function Invoke-Bridge([string[]]$Arguments) {
 $raw=& $cli --project $project --format json @Arguments
 $result=($raw -join "`n") | ConvertFrom-Json
 $records.Add(@{arguments=$Arguments;result=$result})
 $records | ConvertTo-Json -Depth 40 | Set-Content (Join-Path $PSScriptRoot 'binding-evidence.json') -Encoding utf8
 if(!$result.ok){throw ($raw -join "`n")}
}
Invoke-Bridge @('console','--type','error','--count','20')
if($records[0].result.data.totalCount -gt 0){throw 'Resolve Console errors first.'}
Invoke-Bridge @('widget-metadata','UIHome','--vm','VMHome','--prefab','Assets/Data/Widgets/Home/Home.prefab')
if(!$Resume){
 Invoke-Bridge @('components','add','--asset','Assets/Data/Widgets/Home/Home.prefab','--path','Home/SafeArea','--component','HomeSafeArea')
 foreach($entry in @(@('Home','Home/SafeArea/MapVisible/MapBackground'),@('HomeEnvironment','HomeEnvironment/CampBackground'))){
  Invoke-Bridge @('components','add','--asset',"Assets/Data/Widgets/Home/$($entry[0]).prefab",'--path',$entry[1],'--component','UnityEngine.UI.AspectRatioFitter')
 }
}
foreach($entry in @(@('Home','Home/SafeArea/MapVisible/MapBackground'),@('HomeEnvironment','HomeEnvironment/CampBackground'))){
 Invoke-Bridge @('modify','--asset',"Assets/Data/Widgets/Home/$($entry[0]).prefab",'--path',$entry[1],'--component','AspectRatioFitter','--property','m_AspectMode','--value','4')
 Invoke-Bridge @('modify','--asset',"Assets/Data/Widgets/Home/$($entry[0]).prefab",'--path',$entry[1],'--component','AspectRatioFitter','--property','m_AspectRatio','--value','0.5')
}
$font='Assets/Data/Widgets/Common/Fonts/fangzhenglanting.ttf'
$fontSource=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../extracted-resources/assets/data/fonts/fangzhenglanting.ttf'))
Invoke-Bridge @('asset','import','--src',$fontSource,'--dst',$font)
$design=(Get-Content (Join-Path $PSScriptRoot 'home-payload.json') -Raw | ConvertFrom-Json).design
$lookup=@{}
foreach($node in $design.nodes){$lookup[$node.id]=$node}
foreach($node in $design.nodes | Where-Object type -eq 'Text'){
 $parts=[Collections.Generic.List[string]]::new()
 $current=$node
 while($null -ne $current){$parts.Insert(0,$current.name);if($current.parent){$current=$lookup[$current.parent]}else{$current=$null}}
 Invoke-Bridge @('modify','--asset','Assets/Data/Widgets/Home/Home.prefab','--path',($parts -join '/'),'--component','Text','--property','m_FontData.m_Font','--value',$font)
}
Invoke-Bridge @('save')
Set-Content (Join-Path $PSScriptRoot 'install.request') 'Install Home in verified miniwow'
Write-Output 'Home metadata and aspect fitting bound; scene installation requested.'
