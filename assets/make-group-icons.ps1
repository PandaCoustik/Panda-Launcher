$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$sources = @{ 'editor'='panda-editor-v2-interface-transparent.png'; 'run-turquoise'='panda-run-turquoise.png'; 'run-violet'='panda-run-violet.png'; 'run-fuchsia'='panda-run-fuchsia.png'; 'run-emeraude'='panda-run-emeraude.png'; 'run-dore'='panda-run-dore.png' }
foreach ($color in @("turquoise","violet","fuchsia","emeraude","dore")) { $sources["stop-$color"]="panda-stop-$color.png" }
New-Item -ItemType Directory -Force -Path (Join-Path $PSScriptRoot 'icons') | Out-Null
foreach ($assetName in $sources.Keys) {
    $sourceImage = [Drawing.Image]::FromFile((Join-Path (Join-Path $PSScriptRoot "panda-run-couleurs") $sources[$assetName]))
    $iconStream = [IO.MemoryStream]::new()
    $writer = [IO.BinaryWriter]::new($iconStream)
    $sizes = @(16, 24, 32, 48, 64, 128, 256)
    $payloads = @()
    foreach ($size in $sizes) {
        $bitmap = [Drawing.Bitmap]::new($size, $size)
        $graphics = [Drawing.Graphics]::FromImage($bitmap)
        $graphics.InterpolationMode = [Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.DrawImage($sourceImage, 0, 0, $size, $size)
        $png = [IO.MemoryStream]::new()
        $bitmap.Save($png, [Drawing.Imaging.ImageFormat]::Png)
        $payloads += ,$png.ToArray()
        if ($size -eq 256) { $bitmap.Save((Join-Path $PSScriptRoot "icons\$assetName-ui.png"), [Drawing.Imaging.ImageFormat]::Png) }
        $png.Dispose(); $graphics.Dispose(); $bitmap.Dispose()
    }
    $writer.Write([uint16]0); $writer.Write([uint16]1); $writer.Write([uint16]$sizes.Count)
    $offset = 6 + 16 * $sizes.Count
    for ($index = 0; $index -lt $sizes.Count; $index++) {
        $sizeByte = if ($sizes[$index] -eq 256) { 0 } else { $sizes[$index] }
        $writer.Write([byte]$sizeByte); $writer.Write([byte]$sizeByte)
        $writer.Write([byte]0); $writer.Write([byte]0)
        $writer.Write([uint16]1); $writer.Write([uint16]32)
        $writer.Write([uint32]$payloads[$index].Length); $writer.Write([uint32]$offset)
        $offset += $payloads[$index].Length
    }
    foreach ($payload in $payloads) { $writer.Write([byte[]]$payload) }
    [IO.File]::WriteAllBytes((Join-Path $PSScriptRoot "icons\$assetName.ico"), $iconStream.ToArray())
    $writer.Dispose(); $iconStream.Dispose(); $sourceImage.Dispose()
}

