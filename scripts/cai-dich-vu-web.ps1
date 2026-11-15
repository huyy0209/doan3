<#
.SYNOPSIS
    Cài bản web "Quản lý gia sư" làm dịch vụ Windows và mở cổng cho máy khác trong mạng LAN (đề xuất #7).

.DESCRIPTION
    Script làm các việc sau (hướng dẫn đầy đủ: docs/trien-khai.md):
      1. dotnet publish -c Release -o <ThuMuc>
      2. sc.exe create "QuanLyGiaSuWeb" binPath= "dotnet DoAnWeb.dll" start= auto (tự chạy khi khởi động máy)
      3. Đặt biến môi trường của dịch vụ: ASPNETCORE_URLS (mặc định http://0.0.0.0:5080) + ASPNETCORE_ENVIRONMENT=Production
      4. Tùy chọn -MoTuongLua: mở cổng TCP bằng New-NetFirewallRule, CHỈ profile Private (mạng nội bộ)
      5. -GoBo: dừng + gỡ dịch vụ và rule tường lửa (giữ nguyên thư mục đã publish để xem log)

    Phải chạy bằng quyền Administrator (chuột phải QuanLyGiaSu.bat / PowerShell → Run as administrator).
    -WhatIf: chỉ IN ra các lệnh sẽ chạy, KHÔNG thay đổi gì trên máy (dùng để xem trước).

.PARAMETER ThuMuc
    Thư mục publish và chạy dịch vụ. Mặc định: %ProgramFiles%\QuanLyGiaSuWeb

.PARAMETER Cong
    Cổng HTTP cho mạng LAN. Mặc định 5080.

.PARAMETER TenDichVu
    Tên dịch vụ Windows. Mặc định QuanLyGiaSuWeb.

.PARAMETER Url
    Ghi đè ASPNETCORE_URLS. Mặc định http://0.0.0.0:<Cong>.
    Ví dụ HTTPS: -Url "https://0.0.0.0:5443" (kèm chứng chỉ trong appsettings.Production.json).

.PARAMETER MoTuongLua
    Mở cổng trên tường lửa Windows cho máy khác trong LAN (chỉ mạng Private).

.PARAMETER GoBo
    Gỡ dịch vụ + rule tường lửa.

.PARAMETER WhatIf
    Chế độ xem trước: in các lệnh sẽ chạy mà không làm gì.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts\cai-dich-vu-web.ps1 -WhatIf

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts\cai-dich-vu-web.ps1 -MoTuongLua

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts\cai-dich-vu-web.ps1 -GoBo -WhatIf
#>
param(
    [string]$ThuMuc = (Join-Path $env:ProgramFiles 'QuanLyGiaSuWeb'),
    [int]$Cong = 5080,
    [string]$TenDichVu = 'QuanLyGiaSuWeb',
    [string]$Url = '',
    [switch]$MoTuongLua,
    [switch]$GoBo,
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'

$goc = Split-Path -Parent $PSScriptRoot          # thư mục gốc repo
$csproj = Join-Path $goc 'DoAnWeb\DoAnWeb.csproj'
$dll = Join-Path $ThuMuc 'DoAnWeb.dll'
if ([string]::IsNullOrWhiteSpace($Url)) { $Url = "http://0.0.0.0:$Cong" }
$tenRule = "$TenDichVu (TCP $Cong)"
$tenHienThi = 'Quan ly gia su - Ban web'
$khoaRegistry = "HKLM\SYSTEM\CurrentControlSet\Services\$TenDichVu"

function Ghi {
    param([string]$Chu = '', [string]$Mau = 'Gray')
    Write-Host $Chu -ForegroundColor $Mau
}

# Chạy một lệnh cmd/exe (in ra trước để người dùng thấy rõ, và để -WhatIf copy được)
function ChayLenh {
    param(
        [Parameter(Mandatory = $true)][string]$File,
        [string]$ThamSo = '',
        [switch]$ChoPhepLoi
    )
    $hienThi = if ($ThamSo) { "$File $ThamSo" } else { $File }
    if ($WhatIf) {
        Ghi "  [WhatIf] $hienThi" 'DarkYellow'
        return
    }
    Ghi "  > $hienThi" 'Cyan'
    # ArgumentList dạng một chuỗi: PowerShell chuyển nguyên văn cho CreateProcess,
    # cần vậy vì sc.exe đòi dấu \" bên trong binPath=.
    $p = Start-Process -FilePath $File -ArgumentList $ThamSo -Wait -NoNewWindow -PassThru
    if ($p.ExitCode -ne 0 -and -not $ChoPhepLoi) {
        throw "Lệnh thất bại (mã $($p.ExitCode)): $hienThi"
    }
    return
}

# Chạy một lệnh PowerShell (vd New-NetFirewallRule) theo cùng quy ước in/xem trước
function ChayPs {
    param(
        [Parameter(Mandatory = $true)][string]$MoTa,
        [Parameter(Mandatory = $true)][scriptblock]$ThucThi
    )
    if ($WhatIf) {
        Ghi "  [WhatIf] $MoTa" 'DarkYellow'
        return
    }
    Ghi "  > $MoTa" 'Cyan'
    & $ThucThi | Out-Null
}

function KiemTraQuyenAdmin {
    $laAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
        [Security.Principal.WindowsBuiltInRole]::Administrator)
    if ($laAdmin) { return $true }
    if ($WhatIf) {
        Ghi '[!] Chưa chạy bằng quyền Administrator, nhưng -WhatIf chỉ in lệnh xem trước nên vẫn tiếp tục được.' 'Yellow'
        Ghi '    Khi cài thật: chuột phải QuanLyGiaSu.bat → "Run as administrator" (hoặc mở PowerShell bằng quyền Administrator).' 'Yellow'
        return $false
    }
    Ghi '[X] Cần quyền Administrator để cài/gỡ dịch vụ Windows và mở tường lửa.' 'Red'
    Ghi '    Cách chạy: chuột phải QuanLyGiaSu.bat (hoặc PowerShell) → "Run as administrator",' 'Yellow'
    Ghi '    rồi chọn 9 trong menu hoặc chạy lại script này.' 'Yellow'
    return $false
}

function TimRuleTuongLua {
    try { return Get-NetFirewallRule -DisplayName $tenRule -ErrorAction SilentlyContinue }
    catch { return $null }
}

function LayDotnet {
    $lenh = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($lenh) { return $lenh.Source }
    $macDinh = Join-Path $env:ProgramFiles 'dotnet\dotnet.exe'
    if (Test-Path -LiteralPath $macDinh) { return $macDinh }
    return $null
}

# ---------------------------------------------------------------------------
# Bắt đầu
# ---------------------------------------------------------------------------
Ghi ''
Ghi '=== Cài bản web Quản lý gia sư làm dịch vụ Windows (đề xuất #7) ===' 'White'
Ghi "  Dịch vụ     : $TenDichVu"
Ghi "  Thư mục     : $ThuMuc"
Ghi "  Địa chỉ     : $Url"
Ghi "  Tường lửa   : $(if ($MoTuongLua) { "mở cổng TCP $Cong (Private)" } else { 'không đổi' })"
Ghi "  Chế độ      : $(if ($GoBo) { 'GỠ dịch vụ' } else { 'CÀI / nâng cấp' })"
if ($WhatIf) { Ghi '  Xem trước   : -WhatIf (không thay đổi gì trên máy)' 'Yellow' }
Ghi ''

$coQuyen = KiemTraQuyenAdmin
if (-not $coQuyen -and -not $WhatIf) { exit 1 }

$dichVuSanCo = Get-Service -Name $TenDichVu -ErrorAction SilentlyContinue

# ---------------------------------------------------------------------------
# Gỡ dịch vụ
# ---------------------------------------------------------------------------
if ($GoBo) {
    Ghi '--- Gỡ dịch vụ ---' 'White'
    if ($dichVuSanCo) {
        if ($dichVuSanCo.Status -ne 'Stopped') {
            ChayLenh -File 'sc.exe' -ThamSo "stop `"$TenDichVu`"" -ChoPhepLoi | Out-Null
        }
        ChayLenh -File 'sc.exe' -ThamSo "delete `"$TenDichVu`"" -ChoPhepLoi | Out-Null
    } else {
        Ghi "  (dịch vụ $TenDichVu chưa được cài)" 'DarkGray'
    }

    $rule = TimRuleTuongLua
    if ($rule) {
        ChayPs -MoTa "Remove-NetFirewallRule -DisplayName `"$tenRule`"" -ThucThi {
            Remove-NetFirewallRule -DisplayName $tenRule -Confirm:$false
        }
    } else {
        Ghi "  (chưa có rule tường lửa `"$tenRule`")" 'DarkGray'
    }

    Ghi ''
    if ($WhatIf) {
        Ghi '[WhatIf] Đã in xong các lệnh gỡ — CHƯA thay đổi gì trên máy.' 'Yellow'
    } else {
        Ghi '[OK] Đã gỡ dịch vụ + rule tường lửa (nếu có).' 'Green'
    }
    if (Test-Path -LiteralPath $ThuMuc) {
        Ghi "     Thư mục cài vẫn giữ để xem log: $ThuMuc" 'Gray'
        Ghi "     Muốn xóa hẳn: Remove-Item -LiteralPath `"$ThuMuc`" -Recurse -Force" 'Gray'
    }
    exit 0
}

# ---------------------------------------------------------------------------
# Cài / nâng cấp: dừng dịch vụ cũ (nếu có) → publish → tạo lại dịch vụ
# ---------------------------------------------------------------------------
$dotnetExe = LayDotnet
if (-not $dotnetExe) {
    Ghi '[X] Không tìm thấy lệnh dotnet. Cài .NET SDK 8 trở lên: https://dotnet.microsoft.com/download' 'Red'
    exit 1
}
if (-not (Test-Path -LiteralPath $csproj)) {
    Ghi "[X] Không thấy project: $csproj" 'Red'
    exit 1
}

Ghi '--- Bước 1/4: publish bản Release ---' 'White'
if ($dichVuSanCo -and $dichVuSanCo.Status -ne 'Stopped') {
    ChayLenh -File 'sc.exe' -ThamSo "stop `"$TenDichVu`""
    Ghi '  Đợi dịch vụ cũ dừng hẳn...' 'DarkGray'
    if (-not $WhatIf) {
        try { (Get-Service -Name $TenDichVu).WaitForStatus('Stopped', [TimeSpan]::FromSeconds(30)) } catch { }
    }
}

if ($WhatIf) {
    Ghi "  [WhatIf] dotnet publish `"$csproj`" -c Release -o `"$ThuMuc`" --nologo" 'DarkYellow'
} else {
    Ghi "  > dotnet publish `"$csproj`" -c Release -o `"$ThuMuc`" --nologo" 'Cyan'
    & $dotnetExe publish $csproj -c Release -o $ThuMuc --nologo
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish thất bại (mã $LASTEXITCODE)." }
    if (-not (Test-Path -LiteralPath $dll)) { throw "Publish xong nhưng không thấy $dll." }
}

Ghi ''
Ghi '--- Bước 2/4: tạo dịch vụ Windows ---' 'White'
if ($dichVuSanCo) {
    # Cài lại sạch để binPath/DisplayName khớp bản mới
    ChayLenh -File 'sc.exe' -ThamSo "delete `"$TenDichVu`"" -ChoPhepLoi | Out-Null
}

# sc.exe cần binPath= là MỘT tham số chứa: "dotnet.exe" "DoAnWeb.dll"
# => trong dòng lệnh phải viết \"...\" \"...\" (dấu \ ngăn CommandLineToArgvW cắt token)
$binPath = '"\"' + $dotnetExe + '\" \"' + $dll + '\""'
try {
    ChayLenh -File 'sc.exe' -ThamSo "create `"$TenDichVu`" binPath= $binPath start= auto DisplayName= `"$tenHienThi`""
} catch {
    Ghi ''
    Ghi '[X] Không tạo được dịch vụ. Nếu dịch vụ vừa bị gỡ, Windows có thể cần vài giây:' 'Red'
    Ghi '    đóng cửa sổ services.msc, chờ 5–10 giây rồi chạy lại lệnh cài.' 'Yellow'
    throw
}

# Biến môi trường riêng của dịch vụ: URL lắng nghe + môi trường Production
# (reg.exe dùng \0 để ngăn cách các chuỗi trong REG_MULTI_SZ)
$moiTruong = "ASPNETCORE_URLS=$Url\0ASPNETCORE_ENVIRONMENT=Production"
ChayLenh -File 'reg.exe' -ThamSo "add `"$khoaRegistry`" /v Environment /t REG_MULTI_SZ /d `"$moiTruong`" /f"

ChayLenh -File 'sc.exe' -ThamSo "description `"$TenDichVu`" `"Quan ly gia su - ban web (ASP.NET Core), nghe $Url`"" -ChoPhepLoi | Out-Null
# Tự khởi động lại nếu tiến trình web thoát bất thường (3 lần, cách nhau 60 giây)
ChayLenh -File 'sc.exe' -ThamSo "failure `"$TenDichVu`" reset= 86400 actions= restart/60000/restart/60000/restart/60000" -ChoPhepLoi | Out-Null

Ghi ''
Ghi '--- Bước 3/4: tường lửa (máy khác trong LAN) ---' 'White'
if ($MoTuongLua) {
    $rule = TimRuleTuongLua
    if ($rule) {
        Ghi "  (đã có rule `"$tenRule`", bỏ qua)" 'DarkGray'
    } else {
        ChayPs -MoTa "New-NetFirewallRule -DisplayName `"$tenRule`" -Direction Inbound -Protocol TCP -LocalPort $Cong -Action Allow -Profile Private" -ThucThi {
            New-NetFirewallRule -DisplayName $tenRule -Description 'Cho may khac trong mang LAN mo ban web Quan ly gia su' `
                -Direction Inbound -Protocol TCP -LocalPort $Cong -Action Allow -Profile Private
        }
    }
} else {
    Ghi '  Không mở cổng. Máy khác trong LAN chỉ vào được nếu bạn tự mở cổng hoặc dùng -MoTuongLua.' 'DarkGray'
}

Ghi ''
Ghi '--- Bước 4/4: khởi động dịch vụ ---' 'White'
ChayLenh -File 'sc.exe' -ThamSo "start `"$TenDichVu`""

Ghi ''
if ($WhatIf) {
    Ghi '[WhatIf] Đã in xong các lệnh sẽ chạy — CHƯA thay đổi gì trên máy.' 'Yellow'
    Ghi '        Bỏ -WhatIf (và chạy bằng quyền Administrator) để cài thật.' 'Yellow'
} else {
    Ghi '[OK] Cài xong dịch vụ web.' 'Green'
}
Ghi "     Địa chỉ trong máy: http://localhost:$Cong"
$ips = @()
try {
    $ips = Get-NetIPAddress -AddressFamily IPv4 -ErrorAction SilentlyContinue |
        Where-Object { $_.IPAddress -notlike '127.*' -and $_.PrefixOrigin -ne 'WellKnown' } |
        Select-Object -ExpandProperty IPAddress
} catch { }
foreach ($ip in $ips) { Ghi "     Từ máy khác trong LAN: http://${ip}:$Cong  (nếu đã mở tường lửa)" 'Gray' }
Ghi "     Log (Serilog): $ThuMuc\logs\doanweb-YYYYMMDD.log" 'Gray'
Ghi "     Gỡ dịch vụ: QuanLyGiaSu.bat dich-vu -GoBo    (hoặc chạy script này với -GoBo)" 'Gray'
Ghi '     HTTPS / chứng chỉ: xem docs/trien-khai.md' 'Gray'
exit 0
