param(
    [string]$Project = 'G:/Works/GameCLI/GameCLI/examples/unitys/development/client',
    [string]$Cli = 'G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe',
    [string[]]$Pages = @('main','recharge','gift','giftbuy')
)
$ErrorActionPreference = 'Stop'
$contract = Get-Content "$PSScriptRoot/../.data/imports/sv-shop/bindings.json" -Raw | ConvertFrom-Json
$names = @{ main='Shop'; recharge='ShopRecharge'; gift='ShopGift'; giftbuy='ShopPurchase' }
function Invoke-Unity([string[]]$Arguments) {
    $result = & $Cli --project $Project --format json @Arguments | ConvertFrom-Json
    if (!$result.ok) { throw ($result | ConvertTo-Json -Depth 8 -Compress) }
    return $result.data
}
foreach ($page in $Pages) {
    $name = $names[$page]
    $asset = if ($page -eq "main") { "Assets/Data/Widgets/Shop/Shop.prefab" } else { "Assets/Data/Widgets/Shop/$name/$name.prefab" }
    $inspection = Invoke-Unity @('prefab','inspect','--prefab',$asset,'--recursive')
    $components = @{}
    foreach ($node in $inspection.nodes) { $components[$node.path] = [System.Collections.Generic.HashSet[string]]::new(); foreach ($component in $node.components) { [void]$components[$node.path].Add($component.type.Split('.')[-1]) } }
    function Add-Component($path,$type) {
        $short = $type.Split('.')[-1]
        if (!$components[$path].Contains($short)) {
            Invoke-Unity @('components','add','--asset',$asset,'--path',$path,'--component',$type) | Out-Null
            [void]$components[$path].Add($short)
            if ($short -eq 'FancyGridScrollView') { [void]$components[$path].Add('Scroller') }
            if ($short -in @('UIShopGroup','UIShopTab','UIShopRechargeItem','UIShopGiftItem')) { [void]$components[$path].Add('Metadata') }
        }
    }
    function Remove-Component($path,$type) {
        $short = $type.Split('.')[-1]
        if ($components[$path].Contains($short)) {
            Invoke-Unity @('components','remove','--asset',$asset,'--path',$path,'--component',$type) | Out-Null
            [void]$components[$path].Remove($short)
        }
    }
    function Set-Field($path,$type,$field,$value) { Invoke-Unity @('modify','--asset',$asset,'--path',$path,'--component',$type,'--property',$field,'--value',([string]$value)) | Out-Null }
    foreach ($grid in @($contract.grids | Where-Object page -eq $page)) {
        foreach ($node in $inspection.nodes | Where-Object { $_.path.StartsWith($grid.cell+'/') }) { Remove-Component $node.path 'Button'; if ($components[$node.path].Contains('Image')) { Set-Field $node.path 'Image' 'm_RaycastTarget' 'false' } }
        Add-Component $grid.cell 'UnityEngine.UI.Image'
        Set-Field $grid.cell 'Image' 'm_Color' '0,0,0,0'
        Add-Component $grid.cell 'UnityEngine.UI.Button'
        Set-Field $grid.cell 'Button' 'm_TargetGraphic' "@$($grid.cell)#Image"
        Set-Field $grid.cell 'RectTransform' 'm_Pivot' '0.5,0.5'
        Add-Component $grid.cell 'Oathx.Widget.FancyGridScrollViewCell'
        Add-Component $grid.cell ('UI'+$grid.widget)
        Remove-Component $grid.view 'ScrollRect'
        Remove-Component $grid.content 'ContentSizeFitter'
        Remove-Component $grid.content 'GridLayoutGroup'
        Set-Field $grid.content 'RectTransform' 'm_AnchorMin' '0,0'
        Set-Field $grid.content 'RectTransform' 'm_AnchorMax' '1,1'
        Set-Field $grid.content 'RectTransform' 'm_Pivot' '0.5,0.5'
        Set-Field $grid.content 'RectTransform' 'm_SizeDelta' '0,0'
        Set-Field $grid.content 'RectTransform' 'm_AnchoredPosition' '0,0'
        Add-Component $grid.view 'Oathx.Widget.FancyGridScrollView'
        Add-Component $grid.view 'UnityEngine.UI.RectMask2D'
        Set-Field $grid.view 'Scroller' 'viewport' "@$($grid.view)#RectTransform"
        Set-Field $grid.view 'FancyGridScrollView' 'cellContainer' "@$($grid.content)#RectTransform"
        Set-Field $grid.view 'FancyGridScrollView' 'cellPrefab' "@$($grid.cell)#FancyGridScrollViewCell"
        Set-Field $grid.view 'FancyGridScrollView' 'cellSize' ($grid.size -join ',')
        Set-Field $grid.view 'FancyGridScrollView' 'startAxisCellCount' $grid.columns
        Set-Field $grid.view 'FancyGridScrollView' 'startAxisSpacing' '20'
        Set-Field $grid.view 'FancyGridScrollView' 'spacing' $grid.spacing
        Set-Field $grid.view 'FancyGridScrollView' 'paddingHead' '20'
    }
    foreach ($spec in @($contract.specs | Where-Object page -eq $page)) {
        $root=$spec.nativeRoot
        Add-Component $root ('UI'+$spec.name)
        Add-Component $root 'Metadata'
        Set-Field $root 'Metadata' 'className' ('VM'+$spec.name)
        $guid = (Get-Content "$Project/Assets/Scripts/Systems/Shop/VM/VM$($spec.name).cs.meta" | Select-String '^guid: ').Line.Substring(6)
        Set-Field $root 'Metadata' 'classGuid' $guid
        Set-Field $root 'Metadata' 'metadatas.Array.size' $spec.fields.Count
        for ($i=0; $i -lt $spec.fields.Count; $i++) {
            $field=$spec.fields[$i]; $prefix="metadatas.Array.data[$i]"
            Set-Field $root 'Metadata' "$prefix.name" $field.name
            Set-Field $root 'Metadata' "$prefix.type" $field.sync
            Set-Field $root 'Metadata' "$prefix.targets.Array.size" $field.targets.Count
            Set-Field $root 'Metadata' "$prefix.components.Array.size" $field.targets.Count
            for ($j=0; $j -lt $field.targets.Count; $j++) {
                $target=$field.targets[$j]
                Set-Field $root 'Metadata' "$prefix.targets.Array.data[$j]" "@$target"
                Set-Field $root 'Metadata' "$prefix.components.Array.data[$j]" "@$target#$($field.component)"
                if ($field.sync -eq 'SyncText') {
                    Set-Field $target 'Text' 'm_FontData.m_HorizontalOverflow' '1'
                    Set-Field $target 'Text' 'm_FontData.m_VerticalOverflow' '1'
                }
            }
        }
    }
    if ($page -eq 'main') {
        Add-Component 'Shop' 'UnityEngine.CanvasGroup'
        Set-Field 'Shop' 'UIShop' 'interaction' '@Shop#CanvasGroup'
    }
    if ($page -eq 'recharge' -or $page -eq 'gift') {
        $grid = @($contract.grids | Where-Object page -eq $page)[0]
        $animationTarget = $grid.cell
        if ($page -eq 'gift') { $animationTarget += '/RechargeField' }
        Set-Field $grid.cell ('UI'+$grid.widget) 'entrance' "@$animationTarget#Animator"
    }
    Write-Output "$name Metadata and Fancy bindings saved."
}

