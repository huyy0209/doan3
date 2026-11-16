<#
.SYNOPSIS
    Publish DoAnWeb and create or update its IIS site.

.DESCRIPTION
    Requires IIS, the ASP.NET Core Hosting Bundle (ANCM and a compatible
    ASP.NET Core runtime), and Administrator rights for a real installation.
    -WhatIf reports the planned actions without publishing or changing IIS.
#>
param(
    [ValidatePattern('^[A-Za-z0-9][A-Za-z0-9 ._-]*$')]
    [string]$TenSite = 'QuanLyGiaSuWeb',
    [ValidateRange(1, 65535)]
    [int]$Cong = 5080,
    [string]$ThuMuc = (Join-Path $env:ProgramFiles 'QuanLyGiaSuIIS'),
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'

function Ghi {
    param([string]$Chu, [string]$Mau = 'Gray')
    Write-Host $Chu -ForegroundColor $Mau
}

function KiemTraQuyenAdmin {
    $laAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole(
        [Security.Principal.WindowsBuiltInRole]::Administrator)
    if ($laAdmin) { return }
    if ($WhatIf) {
        Ghi '[!] Chưa chạy bằng quyền Administrator; -WhatIf chỉ xem trước nên vẫn tiếp tục.' 'Yellow'
        return
    }
    throw 'Cần quyền Administrator để cấu hình IIS và cấp quyền thư mục. Mở PowerShell hoặc QuanLyGiaSu.bat bằng "Run as administrator".'
}

function LayDotnet {
    $lenh = Get-Command dotnet -ErrorAction SilentlyContinue
    if ($lenh) { return $lenh.Source }
    $macDinh = Join-Path $env:ProgramFiles 'dotnet\dotnet.exe'
    if (Test-Path -LiteralPath $macDinh) { return $macDinh }
    return $null
}

function KiemTraMoiTruong {
    param([string]$DotnetExe)

    $iis = $false
    try {
        $dichVuIis = Get-Service -Name W3SVC -ErrorAction Stop
        Import-Module WebAdministration -ErrorAction Stop
        $iis = $true
        Ghi "[OK] IIS và module WebAdministration đã có (W3SVC: $($dichVuIis.Status))." 'Green'
    } catch {
        Ghi '[X] Chưa cài đủ IIS (Web Server) và module WebAdministration.' 'Red'
    }

    $moduleAncm = Join-Path $env:ProgramFiles 'IIS\Asp.Net Core Module\V2\aspnetcorev2.dll'
    $coAncm = Test-Path -LiteralPath $moduleAncm
    if ($coAncm) {
        Ghi "[OK] Đã tìm thấy ASP.NET Core Module (ANCM): $moduleAncm" 'Green'
    } else {
        Ghi "[X] Không tìm thấy ANCM. Cài ASP.NET Core Hosting Bundle phù hợp (dự án net8.0)." 'Red'
    }

    $runtimeTuongThich = $false
    if ($DotnetExe) {
        $danhSachRuntime = & $DotnetExe --list-runtimes 2>&1
        if ($LASTEXITCODE -eq 0) {
            foreach ($dong in $danhSachRuntime) {
                if ([string]$dong -match '^Microsoft\.AspNetCore\.App\s+([0-9]+)\.') {
                    if ([int]$matches[1] -ge 8) { $runtimeTuongThich = $true }
                }
            }
        }
    }
    if ($runtimeTuongThich) {
        Ghi '[OK] Có ASP.NET Core Runtime 8 trở lên; RollForward=Major cho phép runtime major mới hơn.' 'Green'
    } else {
        Ghi '[X] Thiếu Microsoft.AspNetCore.App runtime 8 trở lên. Cài ASP.NET Core Hosting Bundle.' 'Red'
    }

    if (-not $iis -or -not $coAncm -or -not $runtimeTuongThich) {
        throw 'IIS/Hosting Bundle chưa sẵn sàng; chưa publish hoặc thay đổi cấu hình. Hãy cài thành phần được báo thiếu rồi chạy lại.'
    }
}

function CapQuyenThuMuc {
    param(
        [Parameter(Mandatory = $true)][string]$DuongDan,
        [Parameter(Mandatory = $true)][string]$Quyen
    )
    $identity = "IIS AppPool\$TenSite"
    $grant = "${identity}:(OI)(CI)($Quyen)"
    & icacls.exe $DuongDan '/grant:r' $grant '/T' '/C' | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw "Không cấp được quyền $Quyen cho $identity trên $DuongDan (icacls mã $LASTEXITCODE)."
    }
}

try {
    $goc = Split-Path -Parent $PSScriptRoot
    $csproj = Join-Path $goc 'DoAnWeb\DoAnWeb.csproj'
    if ([string]::IsNullOrWhiteSpace($ThuMuc)) { throw 'Tham số -ThuMuc không được để trống.' }
    $ThuMuc = [IO.Path]::GetFullPath($ThuMuc)
    $dotnetExe = LayDotnet

    Ghi ''
    Ghi '=== Triển khai DoAnWeb lên IIS ===' 'White'
    Ghi "  Site / App Pool : $TenSite"
    Ghi "  Binding         : http *:$Cong"
    Ghi "  Thư mục         : $ThuMuc"
    if ($WhatIf) { Ghi '  Chế độ          : -WhatIf (không publish hoặc thay đổi máy)' 'Yellow' }
    Ghi ''

    if (-not $dotnetExe) { throw 'Không tìm thấy lệnh dotnet. Cài .NET SDK 8 trở lên để publish.' }
    if (-not (Test-Path -LiteralPath $csproj)) { throw "Không thấy project: $csproj" }
    KiemTraMoiTruong -DotnetExe $dotnetExe
    KiemTraQuyenAdmin

    $logs = Join-Path $ThuMuc 'logs'
    $saoLuu = Join-Path $ThuMuc 'sao-luu'
    if ($WhatIf) {
        Ghi '--- Các thao tác sẽ thực hiện ---' 'White'
        Ghi "  [WhatIf] dotnet publish `"$csproj`" -c Release -o `"$ThuMuc`" --nologo" 'DarkYellow'
        Ghi "  [WhatIf] Tạo/cập nhật App Pool `"$TenSite`" (No Managed Code)" 'DarkYellow'
        Ghi "  [WhatIf] Tạo/cập nhật site `"$TenSite`", binding http *:$Cong, thư mục `"$ThuMuc`"" 'DarkYellow'
        Ghi "  [WhatIf] Cấp quyền IIS AppPool\${TenSite}: RX thư mục publish; Modify cho logs và sao-luu" 'DarkYellow'
        Ghi '[WhatIf] Hoàn tất xem trước; chưa publish và chưa thay đổi gì trên máy.' 'Yellow'
        exit 0
    }

    Ghi '--- Publish Release ---' 'White'
    & $dotnetExe publish $csproj -c Release -o $ThuMuc --nologo
    if ($LASTEXITCODE -ne 0) { throw "dotnet publish thất bại (mã $LASTEXITCODE)." }
    $dll = Join-Path $ThuMuc 'DoAnWeb.dll'
    if (-not (Test-Path -LiteralPath $dll)) { throw "Publish xong nhưng không thấy $dll." }
    if (-not (Test-Path -LiteralPath (Join-Path $ThuMuc 'web.config'))) {
        throw "Publish không sinh web.config tại $ThuMuc. Không thể cấu hình ASP.NET Core Module cho IIS."
    }

    Ghi '--- Tạo/cập nhật IIS App Pool và site ---' 'White'
    $poolPath = "IIS:\AppPools\$TenSite"
    if (-not (Test-Path -LiteralPath $poolPath)) {
        New-WebAppPool -Name $TenSite | Out-Null
    }
    Set-ItemProperty -Path $poolPath -Name managedRuntimeVersion -Value ''
    Set-ItemProperty -Path $poolPath -Name managedPipelineMode -Value 'Integrated'
    Set-ItemProperty -Path $poolPath -Name processModel.identityType -Value 'ApplicationPoolIdentity'
    Set-ItemProperty -Path $poolPath -Name enable32BitAppOnWin64 -Value $false

    $site = Get-Website -Name $TenSite -ErrorAction SilentlyContinue
    if (-not $site) {
        New-Website -Name $TenSite -PhysicalPath $ThuMuc -Port $Cong -IPAddress '*' `
            -ApplicationPool $TenSite | Out-Null
    } else {
        Set-ItemProperty -Path "IIS:\Sites\$TenSite" -Name physicalPath -Value $ThuMuc
        Set-ItemProperty -Path "IIS:\Sites\$TenSite" -Name applicationPool -Value $TenSite
        $binding = Get-WebBinding -Name $TenSite | Where-Object {
            $_.protocol -eq 'http' -and $_.bindingInformation -eq "*:${Cong}:"
        }
        if (-not $binding) {
            New-WebBinding -Name $TenSite -Protocol http -IPAddress '*' -Port $Cong
        }
    }

    $logs = Join-Path $ThuMuc 'logs'
    $saoLuu = Join-Path $ThuMuc 'sao-luu'
    New-Item -ItemType Directory -Path $logs -Force | Out-Null
    New-Item -ItemType Directory -Path $saoLuu -Force | Out-Null
    Ghi '--- Cấp quyền cho App Pool ---' 'White'
    CapQuyenThuMuc -DuongDan $ThuMuc -Quyen 'RX'
    CapQuyenThuMuc -DuongDan $logs -Quyen 'M'
    CapQuyenThuMuc -DuongDan $saoLuu -Quyen 'M'

    $pool = Get-WebAppPoolState -Name $TenSite
    if ($pool.Value -ne 'Started') { Start-WebAppPool -Name $TenSite }
    $site = Get-Website -Name $TenSite
    if ($site.State -ne 'Started') { Start-Website -Name $TenSite }

    Ghi ''
    Ghi '[OK] Đã publish và cấu hình IIS.' 'Green'
    Ghi "     URL: http://localhost:$Cong"
    Ghi "     Log: $logs"
    Ghi "     Sao lưu (nếu cấu hình SaoLuu:ThuMuc): $saoLuu"
    Ghi '     Cấu hình HTTPS binding, Production và SQL Server theo docs/trien-khai.md.' 'Gray'
    exit 0
} catch {
    Ghi "[X] $($_.Exception.Message)" 'Red'
    exit 1
}
