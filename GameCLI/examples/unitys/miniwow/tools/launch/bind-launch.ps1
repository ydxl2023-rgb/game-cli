param([switch]$ResumeBindings)
$ErrorActionPreference = 'Stop'
$project = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../client'))
$cli = Join-Path $project 'Library/UnityCLI/UnityCLI.exe'
$prefab = 'Assets/Data/Widgets/Launch/Launch.prefab'
$evidence = [Collections.Generic.List[object]]::new()
function Invoke-Bridge([string[]]$Arguments) {
    $raw = & $cli --project $project --format json @Arguments
    $result = ($raw -join "`n") | ConvertFrom-Json
    if (!$result.ok) { throw ($raw -join "`n") }
    $evidence.Add(@{arguments=$Arguments;result=$result})
}
Invoke-Bridge @('console','--type','error','--count','20')
if ($evidence[0].result.data.totalCount -gt 0) { throw 'Resolve Console errors before binding.' }
if (!$ResumeBindings) {
    Invoke-Bridge @('components','add','--asset',$prefab,'--path','Launch','--component','Oathx.Core.XLauncher')
}
$bindings = @{
    loading='@Launch/Loading'; update='@Launch/Update';
    loadingText='@Launch/Loading/LoadingText#Text'; loadingPercent='@Launch/Loading/LoadingPercent#Text';
    tip='@Launch/Loading/Tip#Text'; downloadText='@Launch/Update/UpdateText#Text';
    downloadPercent='@Launch/Update/UpdatePercent#Text';
    fill='@Launch/Update/Track/FillArea/Fill#RectTransform'; handle='@Launch/Update/Track/FillArea/Handle#RectTransform';
    loadingFill='@Launch/Loading/Track/FillArea/Fill#RectTransform'; loadingHandle='@Launch/Loading/Track/FillArea/Handle#RectTransform'
}
foreach ($field in $bindings.Keys) {
    Invoke-Bridge @('modify','--asset',$prefab,'--path','Launch','--component','Oathx.Core.XLauncher','--property',$field,'--value',$bindings[$field])
}
$font = 'Assets/Data/Widgets/Launch/LaunchAssets/fangzhenglanting.ttf'
$sourceFont = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../../extracted-resources/assets/data/fonts/fangzhenglanting.ttf'))
Invoke-Bridge @('asset','import','--src',$sourceFont,'--dst',$font)
foreach ($label in @('Loading/LoadingText','Loading/LoadingPercent','Loading/Tip','Update/UpdateText','Update/UpdatePercent')) {
    Invoke-Bridge @('modify','--asset',$prefab,'--path',"Launch/$label",'--component','Text','--property','m_FontData.m_Font','--value',$font)
}
Invoke-Bridge @('save')
$evidence | ConvertTo-Json -Depth 30 | Set-Content (Join-Path $PSScriptRoot 'binding-evidence.json') -Encoding utf8
Set-Content (Join-Path $PSScriptRoot 'install.request') 'Install verified Miniwow Launch'
Write-Output 'Launch prefab fields and font bound; scene installation requested.'

