param (
    [string]$WebRoot = (Join-Path $PSScriptRoot '..\DoAnWeb\wwwroot')
)

Add-Type -AssemblyName System.Drawing

$webRootPath = (Resolve-Path $WebRoot).Path
$imageDirectory = Join-Path $webRootPath 'img'

# Vẽ cùng một biểu tượng hình học cho PNG và mọi cỡ trong hai file ICO.
function New-LogoBitmap([int]$Size) {
    $bitmap = New-Object System.Drawing.Bitmap($Size, $Size)
    $graphics = [System.Drawing.Graphics]::FromImage($bitmap)
    $background = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $book = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $green = [System.Drawing.Color]::FromArgb(29, 74, 64)
    $leafGreen = [System.Drawing.Color]::FromArgb(27, 122, 94)
    $paper = [System.Drawing.Color]::FromArgb(242, 245, 243)
    $chalk = [System.Drawing.Color]::FromArgb(243, 196, 79)
    $backgroundBrush = [System.Drawing.SolidBrush]::new($green)
    $capBrush = [System.Drawing.SolidBrush]::new($chalk)
    $bookBrush = [System.Drawing.SolidBrush]::new($paper)
    $bookPen = [System.Drawing.Pen]::new($leafGreen, 3)
    $tasselPen = [System.Drawing.Pen]::new($chalk, 4)

    try {
        $graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $graphics.Clear([System.Drawing.Color]::Transparent)
        $graphics.ScaleTransform($Size / 100.0, $Size / 100.0)

        $background.AddArc(3, 3, 40, 40, 180, 90)
        $background.AddArc(57, 3, 40, 40, 270, 90)
        $background.AddArc(57, 57, 40, 40, 0, 90)
        $background.AddArc(3, 57, 40, 40, 90, 90)
        $background.CloseFigure()
        $graphics.FillPath($backgroundBrush, $background)

        $cap = [System.Drawing.PointF[]]@(
            [System.Drawing.PointF]::new(16, 36),
            [System.Drawing.PointF]::new(50, 18),
            [System.Drawing.PointF]::new(84, 36),
            [System.Drawing.PointF]::new(50, 53)
        )
        $graphics.FillPolygon($capBrush, $cap)
        $graphics.DrawLine($tasselPen, 84, 36, 84, 54)
        $graphics.FillEllipse($capBrush, 81, 54, 6, 6)

        $book.AddBezier(21, 53, 32, 51, 42, 56, 50, 61)
        $book.AddBezier(50, 61, 58, 56, 68, 51, 79, 53)
        $book.AddLine(79, 53, 79, 78)
        $book.AddBezier(79, 78, 67, 76, 58, 80, 50, 84)
        $book.AddBezier(50, 84, 42, 80, 33, 76, 21, 78)
        $book.CloseFigure()
        $graphics.FillPath($bookBrush, $book)
        $graphics.DrawPath($bookPen, $book)
        $graphics.DrawLine($bookPen, 50, 61, 50, 84)

        return $bitmap
    }
    catch {
        $bitmap.Dispose()
        throw
    }
    finally {
        $graphics.Dispose()
        $background.Dispose()
        $book.Dispose()
        $backgroundBrush.Dispose()
        $capBrush.Dispose()
        $bookBrush.Dispose()
        $bookPen.Dispose()
        $tasselPen.Dispose()
    }
}

function Get-IcoFrameBytes([int]$Size) {
    $bitmap = New-LogoBitmap $Size
    $stream = [System.IO.MemoryStream]::new()
    $writer = [System.IO.BinaryWriter]::new($stream)
    try {
        $stride = $Size * 4
        $writer.Write([UInt32]40)
        $writer.Write([Int32]$Size)
        $writer.Write([Int32]($Size * 2))
        $writer.Write([UInt16]1)
        $writer.Write([UInt16]32)
        $writer.Write([UInt32]0)
        $writer.Write([UInt32]($stride * $Size))
        $writer.Write([Int32]0)
        $writer.Write([Int32]0)
        $writer.Write([UInt32]0)
        $writer.Write([UInt32]0)

        for ($y = $Size - 1; $y -ge 0; $y--) {
            for ($x = 0; $x -lt $Size; $x++) {
                $pixel = $bitmap.GetPixel($x, $y)
                $writer.Write($pixel.B)
                $writer.Write($pixel.G)
                $writer.Write($pixel.R)
                $writer.Write($pixel.A)
            }
        }

        $maskStride = [int]([Math]::Ceiling($Size / 32.0) * 4)
        $writer.Write([byte[]]::new($maskStride * $Size))
        return ,$stream.ToArray()
    }
    finally {
        $writer.Dispose()
        $bitmap.Dispose()
        $stream.Dispose()
    }
}

function Save-Ico([string]$Path, [int[]]$Sizes) {
    $images = @()
    foreach ($size in $Sizes) {
        $images += ,@{ Size = $size; Bytes = (Get-IcoFrameBytes $size) }
    }

    $stream = [System.IO.File]::Create($Path)
    $writer = [System.IO.BinaryWriter]::new($stream)
    try {
        $writer.Write([UInt16]0)
        $writer.Write([UInt16]1)
        $writer.Write([UInt16]$images.Count)

        # Mỗi khung DIB được ghi theo cấu trúc ICO để Icon của Windows đọc trực tiếp.
        $imageOffset = 6 + (16 * $images.Count)
        foreach ($image in $images) {
            $dimension = if ($image.Size -eq 256) { [byte]0 } else { [byte]$image.Size }
            $writer.Write($dimension)
            $writer.Write($dimension)
            $writer.Write([byte]0)
            $writer.Write([byte]0)
            $writer.Write([UInt16]1)
            $writer.Write([UInt16]32)
            $writer.Write([UInt32]$image.Bytes.Length)
            $writer.Write([UInt32]$imageOffset)
            $imageOffset += $image.Bytes.Length
        }

        foreach ($image in $images) {
            $writer.Write($image.Bytes)
        }
    }
    finally {
        $writer.Dispose()
    }
}

$logoPath = Join-Path $imageDirectory 'logo-180.png'
$logoBitmap = New-LogoBitmap 180
try {
    $logoBitmap.Save($logoPath, [System.Drawing.Imaging.ImageFormat]::Png)
}
finally {
    $logoBitmap.Dispose()
}

Save-Ico (Join-Path $webRootPath 'favicon.ico') @(16, 32, 48)
Save-Ico (Join-Path $PSScriptRoot 'QuanLyGiaSu.ico') @(16, 32, 48, 256)

Write-Host "Đã tạo logo-180.png, favicon.ico và assets\QuanLyGiaSu.ico."
