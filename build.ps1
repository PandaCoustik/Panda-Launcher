$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:WINDIR 'Microsoft.NET\Framework64\v4.0.30319\csc.exe'
if (!(Test-Path -LiteralPath $compiler)) { throw 'Compilateur .NET Framework 4.x introuvable.' }
$output = Join-Path $PSScriptRoot 'dist'
New-Item -ItemType Directory -Force -Path $output | Out-Null
$resources = @("/resource:$PSScriptRoot\assets\icons\editor-ui.png,editor.png")
foreach ($color in @('turquoise','violet','fuchsia','emeraude','dore')) {
    $resources += "/resource:$PSScriptRoot\assets\icons\run-$color.ico,run-$color.ico"
    $resources += "/resource:$PSScriptRoot\assets\icons\stop-$color.ico,stop-$color.ico"
    $resources += "/resource:$PSScriptRoot\assets\icons\run-$color-ui.png,run-$color.png"
}
& $compiler /nologo /target:winexe /optimize+ /utf8output /win32manifest:"$PSScriptRoot\src\app.manifest" /win32icon:"$PSScriptRoot\assets\icons\editor.ico" @resources /out:"$output\Panda Launcher.exe" /reference:System.Windows.Forms.dll /reference:System.Drawing.dll /reference:System.Web.Extensions.dll /reference:Microsoft.CSharp.dll "$PSScriptRoot\src\Core.cs" "$PSScriptRoot\src\ExecutionIdentity.cs" "$PSScriptRoot\src\Groups.cs" "$PSScriptRoot\src\GroupDialog.cs" "$PSScriptRoot\src\ListActions.cs" "$PSScriptRoot\src\UI.cs"
if ($LASTEXITCODE -ne 0) { throw 'Échec de compilation.' }
Write-Output "Application créée : $output\Panda Launcher.exe"

