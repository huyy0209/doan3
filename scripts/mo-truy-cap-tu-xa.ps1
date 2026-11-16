<#
.SYNOPSIS
    Mở bản web cho các thiết bị trong tailnet Tailscale bằng HTTPS.

.DESCRIPTION
    Chuyển tiếp HTTPS của Tailscale tới bản web chỉ trên loopback.
    Không dùng tailscale funnel: Funnel sẽ công khai web ra Internet.
    -WhatIf chỉ in lệnh, không chạy lệnh Tailscale hay thay đổi cấu hình.

.PARAMETER Cong
    Cổng HTTP của bản web trên máy này. Mặc định 5080.

.PARAMETER Tat
    Tắt cấu hình Tailscale Serve bằng `tailscale serve reset`.

.PARAMETER WhatIf
    Chỉ in lệnh sẽ chạy, không thay đổi gì trên máy.

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts\mo-truy-cap-tu-xa.ps1 -WhatIf

.EXAMPLE
    powershell -ExecutionPolicy Bypass -File scripts\mo-truy-cap-tu-xa.ps1 -Tat
#>
param(
    [ValidateRange(1, 65535)]
    [int]$Cong = 5080,
    [switch]$Tat,
    [switch]$WhatIf
)

$ErrorActionPreference = 'Stop'

function Ghi {
    param([string]$Chu, [string]$Mau = 'Gray')
    Write-Host $Chu -ForegroundColor $Mau
}

if ($WhatIf) {
    if ($Tat) {
        Ghi '[WhatIf] tailscale serve reset' 'DarkYellow'
    } else {
        Ghi "[WhatIf] tailscale serve --bg --https=443 http://127.0.0.1:$Cong" 'DarkYellow'
    }
    Ghi 'CẢNH BÁO: Không dùng tailscale funnel; Funnel sẽ công khai web ra Internet.' 'Yellow'
    exit 0
}

try {
    $tailscale = Get-Command tailscale -ErrorAction SilentlyContinue
    if (-not $tailscale) {
        throw 'Không tìm thấy Tailscale. Hãy cài ứng dụng Tailscale trên máy chạy web.'
    }

    if ($Tat) {
        Ghi 'Đang tắt Tailscale Serve...' 'White'
        & $tailscale.Source serve reset
        if ($LASTEXITCODE -ne 0) {
            throw "Tắt Tailscale Serve thất bại (mã lỗi $LASTEXITCODE)."
        }
        Ghi '[OK] Đã tắt truy cập từ xa qua Tailscale.' 'Green'
        exit 0
    }

    $json = & $tailscale.Source status --json 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Không đọc được trạng thái Tailscale (mã lỗi $LASTEXITCODE). Hãy mở Tailscale và kiểm tra đăng nhập."
    }

    try {
        $status = ($json -join [Environment]::NewLine) | ConvertFrom-Json -ErrorAction Stop
    } catch {
        throw 'Tailscale trả về trạng thái không đọc được. Hãy cập nhật ứng dụng Tailscale rồi thử lại.'
    }

    if ($status.BackendState -ne 'Running' -or -not $status.Self) {
        throw 'Tailscale chưa đăng nhập hoặc chưa kết nối. Hãy mở ứng dụng Tailscale, đăng nhập và đợi kết nối.'
    }

    $tenMien = [string]$status.Self.DNSName
    if ([string]::IsNullOrWhiteSpace($tenMien) -and
        -not [string]::IsNullOrWhiteSpace([string]$status.MagicDNSSuffix) -and
        -not [string]::IsNullOrWhiteSpace([string]$status.Self.HostName)) {
        $tenMien = "$($status.Self.HostName).$($status.MagicDNSSuffix)"
    }
    $tenMien = $tenMien.TrimEnd('.')
    if ([string]::IsNullOrWhiteSpace($tenMien)) {
        throw 'Không lấy được tên miền thiết bị từ Tailscale. Hãy bật MagicDNS trong tailnet.'
    }

    $tcp = New-Object System.Net.Sockets.TcpClient
    try {
        $ketNoi = $tcp.BeginConnect('127.0.0.1', $Cong, $null, $null)
        if (-not $ketNoi.AsyncWaitHandle.WaitOne(1500)) {
            throw "Không kết nối được web tại cổng $Cong. Hãy khởi động web hoặc kiểm tra tham số -Cong."
        }
        $tcp.EndConnect($ketNoi)
    } catch {
        if ($_.Exception.Message -like 'Không kết nối được web tại cổng*') { throw }
        throw "Không kết nối được web tại cổng $Cong. Hãy khởi động web hoặc kiểm tra tham số -Cong."
    } finally {
        $tcp.Close()
    }

    Ghi 'Đang bật HTTPS riêng trong tailnet Tailscale...' 'White'
    & $tailscale.Source serve --bg --https=443 "http://127.0.0.1:$Cong"
    if ($LASTEXITCODE -ne 0) {
        throw "Không bật được Tailscale Serve (mã lỗi $LASTEXITCODE). Hãy kiểm tra phiên bản Tailscale và quyền truy cập tailnet."
    }

    Ghi '[OK] Đã mở web qua HTTPS trong tailnet.' 'Green'
    Ghi "Địa chỉ: https://$tenMien" 'Cyan'
    Ghi 'CẢNH BÁO: Không dùng tailscale funnel; Funnel sẽ công khai web ra Internet.' 'Yellow'
} catch {
    Ghi "[X] $($_.Exception.Message)" 'Red'
    exit 1
}
