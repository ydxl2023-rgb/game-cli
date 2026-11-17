param(
    [string]$Project = 'G:/Works/GameCLI/GameCLI/examples/unitys/development/client',
    [string]$Cli = 'G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe',
    [switch]$KeysOnly
)
$ErrorActionPreference = 'Stop'
$folder = "$PSScriptRoot/../.data/imports/sv-home/fidelity-candidate"
$asset = 'Assets/Data/Widgets/Home/Home.prefab'
$contract = Get-Content "$folder/presentation.json" -Raw | ConvertFrom-Json
$report = Get-Content "$folder/report.json" -Raw | ConvertFrom-Json
$design = (Get-Content "$folder/template.json" -Raw | ConvertFrom-Json).design
$paths = @{}
$enumKeys = @($contract.states | Sort-Object value | ForEach-Object key)
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
$root = $paths['Home0']
if (!(($inspection.nodes | Where-Object path -eq $root).components.type -contains 'UIHome')) {
    Invoke-Unity @('components','add','--asset',$asset,'--path',$root,'--component','UIHome') | Out-Null
}
Set-Field $root UIHome 'indicators.Array.size' $contract.states.Count
for ($index=0; $index -lt $contract.states.Count; $index++) {
    $state = $contract.states[$index]
    Set-Field $root UIHome "indicators.Array.data[$index].indicator" ([array]::IndexOf($enumKeys, $state.key))
    if (!$KeysOnly) { Set-Field $root UIHome "indicators.Array.data[$index].target" "@$($paths[$state.node])" }
}
Set-Field $root UIHome dropImage "@$($paths['Home0_6_5_0_4'])#Image"
Set-Field $root UIHome doubleDrop 'Assets/Data/Widgets/Home/HomeAssets/UI_System_DoubleDrop_02.png'
Set-Field $root UIHome tripleDrop 'Assets/Data/Widgets/Home/HomeAssets/hlhd_3bei.png'
$placeholders = @($report.unimplementedTextPlaceholders) + @($report.bindings | Where-Object dynamic -eq $true)
foreach ($binding in ($placeholders | Where-Object { $null -ne $_ } | Sort-Object node -Unique)) {
    Set-Field $paths[$binding.node] Text m_Text ' '
}
foreach ($grid in $contract.layouts) {
    $path = $paths[$grid.node]
    if (!(($inspection.nodes | Where-Object path -eq $path).components.type -contains 'UnityEngine.UI.GridLayoutGroup')) {
        Invoke-Unity @('components','add','--asset',$asset,'--path',$path,'--component','GridLayoutGroup') | Out-Null
    }
    foreach ($property in @('m_ChildAlignment','m_StartCorner','m_StartAxis','m_Constraint','m_ConstraintCount')) {
        Set-Field $path GridLayoutGroup $property $grid.data.$property
    }
    foreach ($property in @('m_CellSize','m_Spacing')) {
        $value = $grid.data.$property
        Set-Field $path GridLayoutGroup $property "$($value.x),$($value.y)"
    }
    foreach ($side in @('m_Left','m_Right','m_Top','m_Bottom')) {
        Set-Field $path GridLayoutGroup "m_Padding.$side" $grid.data.m_Padding.$side
    }
}
Write-Output "Bound $($contract.states.Count) independent Home indicators and source activity grid."
