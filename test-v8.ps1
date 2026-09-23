$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$testRoot = Join-Path $PSScriptRoot ('tests\v8-runs\' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
Copy-Item -LiteralPath "$PSScriptRoot\dist\Panda Launcher.exe" -Destination $testRoot
& $compiler /nologo /target:winexe /out:"$testRoot\SlowWindow.exe" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll "$PSScriptRoot\tests\SlowWindow.cs"
if ($LASTEXITCODE -ne 0) { throw 'Compilation du témoin échouée.' }
& $compiler /nologo /target:exe /out:"$testRoot\RegressionV8.exe" "/reference:$PSScriptRoot\dist\Panda Launcher.exe" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll "$PSScriptRoot\tests\RegressionV8.cs"
if ($LASTEXITCODE -ne 0) { throw 'Compilation des tests v8 échouée.' }
& "$testRoot\RegressionV8.exe" $testRoot | Tee-Object -FilePath "$testRoot\results.txt"
if ($LASTEXITCODE -ne 0) { throw 'Tests v8 échoués.' }


