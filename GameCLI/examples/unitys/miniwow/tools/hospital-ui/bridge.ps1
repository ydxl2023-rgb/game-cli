$hospitalProject = [IO.Path]::GetFullPath("$PSScriptRoot/../../client")
$hospitalCLI = "$hospitalProject/Library/UnityCLI/UnityCLI.exe"
$hospitalEvidence = "$PSScriptRoot/runtime-evidence.jsonl"
function Invoke-HospitalBridge([string[]]$arguments) {
    for ($attempt = 0; $attempt -lt 5; $attempt++) {
        $raw = & $hospitalCLI --project $hospitalProject --format json @arguments
        $result = ($raw -join "`n") | ConvertFrom-Json
        if ($result.error.code -eq 'unauthorized') { Start-Sleep -Milliseconds 500; continue }
        break
    }
    @{ arguments=$arguments; result=$result } | ConvertTo-Json -Compress -Depth 30 | Add-Content $hospitalEvidence
    if (!$result.ok) { throw ($raw -join "`n") }
    return $result
}
function Invoke-Hospital([string]$method, [string]$arguments='[]', [string]$type='MiniwowHospitalProbe') {
    return Invoke-HospitalBridge @('call','--invoke-type',$type,'--invoke-member',$method,'--args-json',$arguments)
}
function Enter-HospitalTest {
    Invoke-HospitalBridge @('run','play') | Out-Null
    Start-Sleep -Seconds 2
    Invoke-Hospital 'SetResolution' '[1080,1920]' 'MiniwowHomeProbe' | Out-Null
    Invoke-Hospital 'LoadScene' '["App"]' 'UnityEngine.SceneManagement.SceneManager' | Out-Null
    Start-Sleep -Seconds 2
    Invoke-Hospital 'EnterGame' '[]' 'MiniwowTaskProbe' | Out-Null
    Start-Sleep -Seconds 2
}
