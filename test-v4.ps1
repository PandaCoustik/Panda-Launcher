$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$testRoot = Join-Path $PSScriptRoot ('tests\v4-runs\' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
Copy-Item -LiteralPath "$PSScriptRoot\dist\Panda Launcher.exe" -Destination $testRoot
& $compiler /nologo /target:winexe /out:"$testRoot\Probe.exe" "$PSScriptRoot\tests\Probe.cs"
if ($LASTEXITCODE -ne 0) { throw 'Compilation du témoin échouée.' }
& $compiler /nologo /target:exe /out:"$testRoot\RegressionV4.exe" "/reference:$PSScriptRoot\dist\Panda Launcher.exe" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll "$PSScriptRoot\tests\RegressionV4.cs"
if ($LASTEXITCODE -ne 0) { throw 'Compilation des tests v4 échouée.' }
& "$testRoot\RegressionV4.exe" $testRoot "$PSScriptRoot\dist\Panda Launcher.exe" | Tee-Object -FilePath "$testRoot\results.txt"
if ($LASTEXITCODE -ne 0) { throw 'Tests v4 échoués.' }
