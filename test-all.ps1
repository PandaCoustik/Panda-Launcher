$ErrorActionPreference = 'Stop'
foreach ($suite in @('test.ps1','test-v3.ps1','test-v4.ps1','test-v5.ps1','test-v6.ps1','test-v7.ps1','test-v8.ps1','test-v9.ps1','test-v10.ps1')) {
    & (Join-Path $PSScriptRoot $suite)
}
Write-Output 'Toutes les suites de régression sont terminées.'
