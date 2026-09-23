$ErrorActionPreference = 'Stop'
& "$PSScriptRoot\build.ps1"
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
$testRoot = Join-Path $PSScriptRoot ('tests\runs\Essai été & Panda ' + [Guid]::NewGuid().ToString('N'))
New-Item -ItemType Directory -Path $testRoot -Force | Out-Null
& $compiler /nologo /target:winexe /out:"$testRoot\Programme témoin.exe" "$PSScriptRoot\tests\Probe.cs"
if ($LASTEXITCODE -ne 0) { throw 'Compilation du témoin échouée.' }
& $compiler /nologo /target:exe /out:"$testRoot\Tests.exe" /reference:System.Web.Extensions.dll /reference:Microsoft.CSharp.dll "$PSScriptRoot\src\Core.cs" "$PSScriptRoot\src\ExecutionIdentity.cs" "$PSScriptRoot\tests\Tests.cs"
if ($LASTEXITCODE -ne 0) { throw 'Compilation des tests échouée.' }
& "$testRoot\Tests.exe" $testRoot "$PSScriptRoot\dist\Panda Launcher.exe" | Tee-Object -FilePath "$testRoot\results.txt"
if ($LASTEXITCODE -ne 0) { throw 'Tests échoués.' }
