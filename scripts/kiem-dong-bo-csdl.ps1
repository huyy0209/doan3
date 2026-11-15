<#
.SYNOPSIS
Kiểm tra đồng bộ CSDL giữa hai nhánh git (app và web)

.DESCRIPTION
Script này so sánh nội dung thư mục Database/ giữa hai nhánh bất kỳ
Báo cáo file chỉ có ở nhánh này, file chỉ có ở nhánh kia, file khác nội dung
Trả về mã thoát: 0 nếu giống nhau, 1 nếu khác nhau

.PARAMETER NhanhA
Tên nhánh A để so sánh (mặc định: app)

.PARAMETER NhanhB
Tên nhánh B để so sánh (mặc định: web)

.EXAMPLE
.\kiem-dong-bo-csdl.ps1 -NhanhA app -NhanhB web

.EXAMPLE
.\kiem-dong-bo-csdl.ps1 -NhanhA master -NhanhB huyy0209/web-c3
#>
param(
    [string]$NhanhA = "app",
    [string]$NhanhB = "web"
)

# Màu chữ cho output
$Color_Reset = [ConsoleColor]::White
$Color_Header = [ConsoleColor]::Cyan
$Color_OnlyA = [ConsoleColor]::Yellow
$Color_OnlyB = [ConsoleColor]::Magenta
$Color_Different = [ConsoleColor]::Red
$Color_Same = [ConsoleColor]::Green
$Color_Error = [ConsoleColor]::Red

# Hàm để hiển thị text với màu sắc
function Write-Colored {
    param([ConsoleColor]$Color, [string]$Message)
    $prevColor = $Host.UI.RawUI.ForegroundColor
    $Host.UI.RawUI.ForegroundColor = $Color
    Write-Output $Message
    $Host.UI.RawUI.ForegroundColor = $prevColor
}

# Hàm để lấy danh sách file từ một nhánh
function Get-BranchFiles {
    param([string]$Branch, [string]$Path = "Database")

    try {
        # Lấy danh sách file từ nhánh bằng git ls-tree
        $treeOutput = git ls-tree -r --name-only $Branch -- $Path 2>$null
        if ($LASTEXITCODE -ne 0) {
            Write-Colored -Color $Color_Error "✗ Không thể đọc nhánh $Branch"
            return @()
        }

        # Lọc chỉ các file trong Database/
        $files = $treeOutput | Where-Object { $_ -like "Database/*" } | ForEach-Object { $_.Substring(9) }
        return $files
    }
    catch {
        Write-Colored -Color $Color_Error "✗ Lỗi khi lấy file từ $Branch"
        return @()
    }
}

# Hàm để kiểm tra hash của file
function Get-GitFileHash {
    param([string]$Branch, [string]$File)

    try {
        # Dùng git blob của file để lấy hash
        $blobHash = (git hash-object "${Branch}:${File}" 2>$null).Trim()
        if ($LASTEXITCODE -ne 0) {
            return $null
        }
        return $blobHash
    }
    catch {
        return $null
    }
}

# Hiển thị header
Write-Colored -Color $Color_Header @"
╔════════════════════════════════════════════════════════════════════════════════════════╗
║ KIỂM TRA ĐỒNG BỘ CSDL - SO SÁNH NHÁNH '$NhanhA' vs '$NhanhB'                             ║
╚════════════════════════════════════════════════════════════════════════════════════════╝
"@

# Fetch nhánh từ remote nếu có
Write-Colored -Color $Color_Reset "`n[1] Fetch nhánh từ remote..."
try {
    git fetch origin $NhanhA $NhanhB -q
    Write-Colored -Color $Color_Same "✓ Đã fetch thành công từ remote"
}
catch {
    Write-Colored -Color $Color_Yellow "⚠ Không fetch được từ remote (có thể offline), sẽ dùng nhánh local"
}

# Lấy danh sách file từ cả hai nhánh
Write-Colored -Color $Color_Reset "`n[2] Lấy danh sách file từ Database/..."
try {
    $filesA = Get-BranchFiles -Branch $NhanhA
    $filesB = Get-BranchFiles -Branch $NhanhB

    Write-Colored -Color $Color_Same "✓ Nhánh '$NhanhA': $($filesA.Count) file"
    Write-Colored -Color $Color_Same "✓ Nhánh '$NhanhB': $($filesB.Count) file"
}
catch {
    Write-Colored -Color $Color_Error "✗ Lỗi khi lấy file: $_"
    exit 1
}

# Tìm các loại khác nhau
$filesAOnly = $filesA | Where-Object { $_ -notin $filesB }
$filesBOnly = $filesB | Where-Object { $_ -notin $filesA }
$commonFiles = $filesA | Where-Object { $_ -in $filesB }

# Kiểm tra file có nội dung khác nhau
$differentFiles = @()
foreach ($file in $commonFiles) {
    $hashA = Get-GitFileHash -Branch $NhanhA -File $file
    $hashB = Get-GitFileHash -Branch $NhanhB -File $file

    if ($hashA -ne $hashB) {
        $differentFiles += $file
    }
}

# Hiển thị kết quả
Write-Colored -Color $Color_Reset "`n[3] KẾT QUẢ SO SÁNH:"
Write-Colored -Color $Color_Reset "┌──────────────────────────────────────────────────────────────────────────────────────┐"

# File chỉ có ở nhánh A
if ($filesAOnly.Count -gt 0) {
    Write-Colored -Color $Color_OnlyA "║ CHỈ CÓ Ở NHÁNH '$NhanhA' ($($filesAOnly.Count) file):"
    foreach ($file in $filesAOnly) {
        Write-Colored -Color $Color_OnlyA "║  • $file"
    }
} else {
    Write-Colored -Color $Color_Same "║ CHỈ CÓ Ở NHÁNH '$NhanhA': (Không có file)"
}

# File chỉ có ở nhánh B
if ($filesBOnly.Count -gt 0) {
    Write-Colored -Color $Color_OnlyB "║ CHỈ CÓ Ở NHÁNH '$NhanhB' ($($filesBOnly.Count) file):"
    foreach ($file in $filesBOnly) {
        Write-Colored -Color $Color_OnlyB "║  • $file"
    }
} else {
    Write-Colored -Color $Color_Same "║ CHỈ CÓ Ở NHÁNH '$NhanhB': (Không có file)"
}

# File khác nội dung
if ($differentFiles.Count -gt 0) {
    Write-Colored -Color $Color_Different "║ KHÁC NỘI DUNG ($($differentFiles.Count) file):"
    foreach ($file in $differentFiles) {
        Write-Colored -Color $Color_Different "║  • $file"
    }
} else {
    Write-Colored -Color $Color_Same "║ KHÁC NỘI DUNG: (Không có file)"
}

Write-Colored -Color $Color_Reset "└──────────────────────────────────────────────────────────────────────────────────────┘"

# Tổng kết
$totalDiff = $filesAOnly.Count + $filesBOnly.Count + $differentFiles.Count
Write-Colored -Color $Color_Reset "`n[TỔNG KẾT]"
if ($totalDiff -eq 0) {
    Write-Colored -Color $Color_Same "✓ Database/ hoàn toàn giống nhau giữa hai nhánh"
    exit 0
} else {
    Write-Colored -Color $Color_Different "✗ Database/ có sự khác biệt giữa hai nhánh"
    Write-Colored -Color $Color_Reset "  • Tổng cộng: $totalDiff file khác biệt"
    Write-Colored -Color $Color_Reset "  • Chỉ có ở ${NhanhA}: $($filesAOnly.Count) file"
    Write-Colored -Color $Color_Reset "  • Chỉ có ở ${NhanhB}: $($filesBOnly.Count) file"
    Write-Colored -Color $Color_Reset "  • Khác nội dung: $($differentFiles.Count) file"
    exit 1
}