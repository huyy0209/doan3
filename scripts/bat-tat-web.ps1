<#
.SYNOPSIS
    Bật / tắt bản web "Quản lý gia sư" (dịch vụ Windows + link công khai Tailscale Funnel).

.DESCRIPTION
    -Che Bat : bật SQL Server, bật dịch vụ QuanLyGiaSuWeb (cài nếu chưa có), chờ web lên,
               bật Tailscale + Funnel rồi in link công khai và mở trình duyệt.
    -Che Tat : tắt Funnel (link công khai chết ngay), dừng dịch vụ, đặt Manual để
               không tự chạy lại khi khởi động máy.
    Gọi qua BatWeb.bat / TatWeb.bat ở thư mục gốc (tự xin quyền Administrator).
#>
param(
    [Parameter(Mandatory = $true)][ValidateSet('Bat', 'Tat')][string]$Che,
    [string]$TenDichVu = 'QuanLyGiaSuWeb',
    [int]$Cong = 5080
)

$ErrorActionPreference = 'Stop'
$tailscale = Join-Path $env:ProgramFiles 'Tailscale\tailscale.exe'
$goc = Split-Path -Parent $PSScriptRoot

function Ghi([string]$Chu, [string]$Mau = 'Gray') { Write-Host $Chu -ForegroundColor $Mau }

function CoTailscale { Test-Path -LiteralPath $tailscale }

function LayLinkFunnel {
    if (-not (CoTailscale)) { return $null }
    $dong = & $tailscale funnel status 2>$null | Select-String -Pattern 'https://\S+' | Select-Object -First 1
    if ($dong) { return $dong.Matches[0].Value.TrimEnd('/') + '/' }
    return $null
}

if ($Che -eq 'Bat') {
    Ghi '=== BẬT WEB QUẢN LÝ GIA SƯ ===' 'White'

    # 1. SQL Server
    $sql = Get-Service -Name 'MSSQLSERVER' -ErrorAction SilentlyContinue
    if ($sql -and $sql.Status -ne 'Running') {
        Ghi '[1/4] Bật SQL Server...'
        Start-Service -Name 'MSSQLSERVER'
    }
    else { Ghi '[1/4] SQL Server đang chạy.' }

    # 2. Dịch vụ web (cài nếu chưa có)
    $dv = Get-Service -Name $TenDichVu -ErrorAction SilentlyContinue
    if (-not $dv) {
        Ghi '[2/4] Chưa cài dịch vụ web, đang cài (build Release, mất 1-2 phút)...' 'Yellow'
        & (Join-Path $PSScriptRoot 'cai-dich-vu-web.ps1')
        if ($LASTEXITCODE) { throw 'Cài dịch vụ thất bại.' }
    }
    else {
        Ghi '[2/4] Bật dịch vụ web...'
        Set-Service -Name $TenDichVu -StartupType Automatic
        if ($dv.Status -ne 'Running') { Start-Service -Name $TenDichVu }
    }

    # 3. Chờ web trả lời
    Ghi "[3/4] Chờ web lên ở http://localhost:$Cong ..."
    $len = $false
    for ($i = 0; $i -lt 45 -and -not $len; $i++) {
        try {
            Invoke-WebRequest -Uri "http://localhost:$Cong/" -UseBasicParsing -TimeoutSec 3 | Out-Null
            $len = $true
        }
        catch { Start-Sleep -Seconds 1 }
    }
    if (-not $len) { Ghi '    Web chưa trả lời sau 45 giây. Xem log trong C:\Program Files\QuanLyGiaSuWeb\logs' 'Red' }

    # 4. Tailscale Funnel
    $link = "http://localhost:$Cong/"
    if (CoTailscale) {
        Ghi '[4/4] Bật Tailscale Funnel (link công khai)...'
        $svcTs = Get-Service -Name 'Tailscale' -ErrorAction SilentlyContinue
        if ($svcTs -and $svcTs.Status -ne 'Running') { Start-Service -Name 'Tailscale'; Start-Sleep -Seconds 3 }
        & $tailscale up 2>$null | Out-Null
        & $tailscale funnel --bg $Cong | Out-Null
        $linkCongKhai = LayLinkFunnel
        if ($linkCongKhai) { $link = $linkCongKhai }
        else { Ghi '    Không bật được Funnel. Kiểm tra đã đăng nhập Tailscale chưa.' 'Red' }
    }
    else { Ghi '[4/4] Không thấy Tailscale, chỉ dùng được trên máy này / mạng LAN.' 'Yellow' }

    Ghi ''
    Ghi "WEB ĐÃ BẬT: $link" 'Green'
    Start-Process $link
}
else {
    Ghi '=== TẮT WEB QUẢN LÝ GIA SƯ ===' 'White'

    if (CoTailscale) {
        Ghi '[1/2] Tắt link công khai (Funnel)...'
        & $tailscale funnel --https=443 off 2>$null | Out-Null
    }

    $dv = Get-Service -Name $TenDichVu -ErrorAction SilentlyContinue
    if ($dv) {
        Ghi '[2/2] Dừng dịch vụ web...'
        if ($dv.Status -ne 'Stopped') { Stop-Service -Name $TenDichVu -Force }
        Set-Service -Name $TenDichVu -StartupType Manual
    }
    # Bản chạy tay (start-web.bat), nếu có
    & (Join-Path $PSScriptRoot 'stop-web.bat') | Out-Null

    Ghi ''
    Ghi 'WEB ĐÃ TẮT. Link công khai không còn truy cập được; khởi động lại máy web cũng không tự chạy.' 'Green'
    Ghi 'Bật lại: chạy BatWeb.bat' 'Gray'
}
