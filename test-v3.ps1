$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$testRoot = Join-Path $PSScriptRoot ('tests\v3-runs\' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
& $compiler /nologo /target:exe /main:RegressionV3 /out:"$testRoot\RegressionV3.exe" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll /reference:Microsoft.CSharp.dll "$PSScriptRoot\src\Core.cs" "$PSScriptRoot\src\ExecutionIdentity.cs" "$PSScriptRoot\src\Groups.cs" "$PSScriptRoot\src\GroupDialog.cs" "$PSScriptRoot\src\ListActions.cs" "$PSScriptRoot\src\UI.cs" "$PSScriptRoot\tests\RegressionV3.cs"
if ($LASTEXITCODE -ne 0) { throw 'Compilation des tests v3 échouée.' }
& "$testRoot\RegressionV3.exe" $testRoot | Tee-Object -FilePath "$testRoot\results.txt"
if ($LASTEXITCODE -ne 0) { throw 'Tests v3 échoués.' }
