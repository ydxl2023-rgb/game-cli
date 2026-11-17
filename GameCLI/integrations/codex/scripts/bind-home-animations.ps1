$ErrorActionPreference = 'Stop'
$project = 'G:/Works/GameCLI/GameCLI/examples/unitys/development/client'
$cli = 'G:/gameai/ugame-ai-cli/app/client/Library/UnityCLI/UnityCLI.exe'
$animator = "$project/Library/UnityCLI/unity-cli.exe"
$folder = "$PSScriptRoot/../.data/imports/sv-home/fidelity-candidate"
$asset = 'Assets/Data/Widgets/Home/Home.prefab'
$design = (Get-Content "$folder/template.json" -Raw | ConvertFrom-Json).design
$paths = @{}
foreach ($node in $design.nodes) { $paths[$node.id] = if ($node.parent) { $paths[$node.parent]+'/'+$node.name } else { $node.name } }
function Invoke-Unity([string[]]$Arguments) {
    $result = & $cli --project $project --format json @Arguments | ConvertFrom-Json
    if (!$result.ok) { throw ($result | ConvertTo-Json -Depth 8 -Compress) }
    return $result.data
}
function Invoke-Animator([string[]]$Arguments) {
    $result = & $animator animator @Arguments --project $project --json | ConvertFrom-Json
    if (!$result.ok) { throw ($result | ConvertTo-Json -Depth 6 -Compress) }
}
$specs = @(
    @{name='HomeEntrance';animation=($design.animations | Where-Object id -eq HomeEntrance);input="$folder/home-open.clip.json"},
    @{name='HomeNewTag';animation=($design.animations | Where-Object { $_.clip.name -eq 'HomeNewTag' } | Select-Object -First 1)},
    @{name='HomeDrop';animation=($design.animations | Where-Object id -eq HomeDropLoop)}
)
foreach ($spec in $specs) {
    $clip="Assets/Data/Widgets/Home/$($spec.name).anim"
    $controller="Assets/Data/Widgets/Home/$($spec.name).controller"
    if (!$spec.input) {
        $curves = @($spec.animation.clip.curves | ForEach-Object {
            @{path='';component=$_.component;property=$_.property;preWrapMode=8;postWrapMode=8;keys=@($_.keys | ForEach-Object {
                @{time=$_.time;value=$_.value;inSlope=$_.inTangent;outSlope=$_.outTangent;inWeight=$_.inWeight;outWeight=$_.outWeight;weightedMode=0;tangentMode=0}
            })}
        })
        $spec.input="$folder/$($spec.name).clip.json"
        @{schemaVersion='animation-clip/1';frameRate=30;loop=$true;curves=$curves} | ConvertTo-Json -Depth 20 | Set-Content $spec.input
    }
    Invoke-Animator @('clip','import-json','--clip',$clip,'--input',$spec.input,'--dry-run')
    Invoke-Animator @('clip','import-json','--clip',$clip,'--input',$spec.input)
    if (!(Test-Path "$project/$controller")) {
        Invoke-Animator @('controller','create','--controller',$controller,'--dry-run')
        Invoke-Animator @('controller','create','--controller',$controller)
    }
    Invoke-Animator @('state','add','--controller',$controller,'--state',$spec.animation.state.name,'--clip',$clip,'--dry-run')
    Invoke-Animator @('state','add','--controller',$controller,'--state',$spec.animation.state.name,'--clip',$clip)
}
$inspection = Invoke-Unity @('prefab','inspect','--prefab',$asset,'--recursive')
foreach ($animation in $design.animations) {
    $path = $paths[$animation.hostNodeId]
    if (!(($inspection.nodes | Where-Object path -eq $path).components.type -contains 'UnityEngine.Animator')) {
        Invoke-Unity @('components','add','--asset',$asset,'--path',$path,'--component','Animator') | Out-Null
    }
    $name = if ($animation.id -eq 'HomeEntrance') {'HomeEntrance'} elseif ($animation.id -eq 'HomeDropLoop') {'HomeDrop'} else {'HomeNewTag'}
    Invoke-Unity @('modify','--asset',$asset,'--path',$path,'--component','Animator','--property','m_Controller','--value',"Assets/Data/Widgets/Home/$name.controller") | Out-Null
}
$left=$paths['Home0_6_2']
if (!(($inspection.nodes | Where-Object path -eq $left).components.type -contains 'UnityEngine.CanvasGroup')) {
    Invoke-Unity @('components','add','--asset',$asset,'--path',$left,'--component','CanvasGroup') | Out-Null
}
Write-Output 'Restored Home entrance, 49 NEW loops, and drop loop.'
