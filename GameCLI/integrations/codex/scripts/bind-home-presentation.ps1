param(
    [string]$Project = 'G:/Works/GameCLI/GameCLI/examples/unitys/development/client',
    [string]$Cli = 'G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe',
    [string]$Folder = "$PSScriptRoot/../.data/imports/sv-home/fidelity-candidate"
)
$ErrorActionPreference = 'Stop'
$asset = 'Assets/Data/Widgets/Home/Home.prefab'
$contract = Get-Content "$Folder/presentation.json" -Raw | ConvertFrom-Json
$design = (Get-Content "$Folder/template.json" -Raw | ConvertFrom-Json).design
$paths = @{}
foreach ($node in $design.nodes) {
    $paths[$node.id] = if ($node.parent) { $paths[$node.parent] + '/' + $node.name } else { $node.name }
}
function Invoke-Unity([string[]]$Arguments) {
    $result = & $Cli --project $Project --format json @Arguments | ConvertFrom-Json
    if (!$result.ok) { throw ($result | ConvertTo-Json -Depth 8 -Compress) }
    return $result.data
}
function Set-Field($path,$type,$field,$value) {
    Invoke-Unity @('modify','--asset',$asset,'--path',$path,'--component',$type,'--property',$field,'--value',([string]$value)) | Out-Null
}
$inspection = Invoke-Unity @('prefab','inspect','--prefab',$asset,'--recursive')
$present = @{}
foreach ($node in $inspection.nodes) { $present[$node.path] = $node }
$count = 0
foreach ($style in $contract.textStyles) {
    $path = $paths[$style.node]
    if (!$present.ContainsKey($path)) { continue }
    if (!$style.font.path) { throw "Source font missing: $path" }
    $fontName = [System.IO.Path]::GetFileName($style.font.path)
    $font = "Assets/Data/Widgets/Home/HomeAssets/$fontName"
    if (!(Test-Path "$Project/$font")) { throw "Import source font before binding: $font" }
    Set-Field $path Text 'm_FontData.m_Font' $font
    foreach ($property in @('m_FontSize','m_FontStyle','m_BestFit','m_MinSize','m_MaxSize','m_Alignment','m_AlignByGeometry','m_RichText','m_HorizontalOverflow','m_VerticalOverflow','m_LineSpacing')) {
        Set-Field $path Text "m_FontData.$property" $style.fontData.$property
    }
    if ($style.outline) {
        if (!($present[$path].components.type -contains 'HomeTextOutline')) {
            Invoke-Unity @('components','add','--asset',$asset,'--path',$path,'--component','HomeTextOutline') | Out-Null
        }
        $outline = ($style.outline.json | ConvertFrom-Json).MonoBehaviour
        Set-Field $path HomeTextOutline outlineShader 'Assets/Data/Widgets/Home/HomeAssets/OutlineEx.shader'
        Set-Field $path HomeTextOutline outlineWidth $outline.OutlineWidth
        $color = $outline.OutlineColor
        Set-Field $path HomeTextOutline outlineColor "$($color.r),$($color.g),$($color.b),$($color.a)"
    }
    $count++
}
Write-Output "Restored source typography on $count text nodes."
