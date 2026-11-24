param(
    [Parameter(Mandatory)][string]$Project,
    [Parameter(Mandatory)][string]$Contract,
    [Parameter(Mandatory)][string]$BaselineResources,
    [Parameter(Mandatory)][string]$Output
)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$document = Get-Content -LiteralPath $Contract -Raw | ConvertFrom-Json
$mapping = Get-Content -LiteralPath $BaselineResources -Raw | ConvertFrom-Json
$Project = (Resolve-Path -LiteralPath $Project).Path
if ([IO.Path]::GetFullPath($Contract) -eq [IO.Path]::GetFullPath($Output)) { throw 'Baseline output must not overwrite input contract' }
function Measure-Skin([string]$Path) {
    $bitmap = [Drawing.Bitmap]::FromFile($Path)
    try {
        $left=$bitmap.Width; $top=$bitmap.Height; $right=-1; $bottom=-1
        $visible=0; $centerVisible=0; $centerTotal=0
        for($y=0;$y -lt $bitmap.Height;$y++) {
            for($x=0;$x -lt $bitmap.Width;$x++) {
                $center=$x -ge [Math]::Floor($bitmap.Width/4) -and $x -lt [Math]::Floor($bitmap.Width*3/4) -and $y -ge [Math]::Floor($bitmap.Height/4) -and $y -lt [Math]::Floor($bitmap.Height*3/4)
                if($center){$centerTotal++}
                if($bitmap.GetPixel($x,$y).A -lt 128){continue}
                $visible++
                if($center){$centerVisible++}
                $left=[Math]::Min($left,$x);$top=[Math]::Min($top,$y)
                $right=[Math]::Max($right,$x);$bottom=[Math]::Max($bottom,$y)
            }
        }
        if($right -lt 0){throw "Empty skin: $Path"}
        return @{x=$left;y=$top;width=($right-$left+1);height=($bottom-$top+1);imageWidth=$bitmap.Width;imageHeight=$bitmap.Height;coverage=($visible/($bitmap.Width*$bitmap.Height));centerCoverage=$(if($centerTotal){$centerVisible/$centerTotal}else{0})}
    } finally { $bitmap.Dispose() }
}
$audit=@()
$windows=$(if($document.windows){$document.windows}else{@($document.window)})
foreach($window in $windows) {
    foreach($slot in $window.resourceSlots) {
        if($slot.kind -ne 'sprite'){continue}
        $matches=@($mapping.resources|Where-Object slotId -eq $slot.slotId)
        if($matches.Count -ne 1){throw "Missing/ambiguous baseline slot: $($slot.slotId)"}
        $record=$matches[0]
        if(!$record.path.StartsWith('Assets/') -or $record.path.Split('/') -contains '..'){throw 'Invalid baseline path'}
        $path=Join-Path $Project $record.path
        if((Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash.ToLowerInvariant() -ne $record.sha256){throw "Baseline hash mismatch: $($slot.slotId)"}
        $m=Measure-Skin $path
        $sx=$slot.deliverySize.width/$m.imageWidth;$sy=$slot.deliverySize.height/$m.imageHeight
        if(!$slot.purpose){throw "Explicit purpose required for baseline: $($slot.slotId)"}
        $filledSurface=$slot.purpose -in @('surface','background')
        $coverageFloor=$(if($filledSurface){$m.coverage*0.65}else{0})
        $centerFloor=$(if($filledSurface){$m.centerCoverage*0.7}else{0})
        $bounds=@{x=[int][Math]::Round($m.x*$sx);y=[int][Math]::Round($m.y*$sy);width=[int][Math]::Round($m.width*$sx);height=[int][Math]::Round($m.height*$sy);alphaThreshold=128;tolerance=2;minCoverage=$coverageFloor;minCenterCoverage=$centerFloor}
        $slot|Add-Member -Force visibleBounds $bounds
        $audit+=@{slotId=$slot.slotId;baselinePath=$record.path;sha256=$record.sha256;measured=$m;guard=$bounds}
    }
}
$document|ConvertTo-Json -Depth 100|Set-Content -LiteralPath $Output -Encoding utf8
$audit|ConvertTo-Json -Depth 10|Set-Content -LiteralPath "$Output.baseline.json" -Encoding utf8
Write-Output "Captured $($audit.Count) sprite baselines; layout and binding fields preserved."
