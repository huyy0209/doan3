@echo off
rem ==========================================================================
rem  Quan ly gia su - BAN WEB: build + chay DoAnWeb, tu mo trinh duyet.
rem  Can: .NET SDK 8 tro len (may chi co .NET 10 van chay duoc nho RollForward),
rem       SQL Server "." co CSDL QuanLyGiaSu (chay CaiDatCSDL.bat neu chua co).
rem  Dung web: stop-web.bat   (ban WinForms van dung start.bat / stop.bat)
rem ==========================================================================
setlocal EnableExtensions
chcp 65001 >nul
title Quan ly gia su - Ban web
cd /d "%~dp0"

set "URL=http://localhost:5080"

where dotnet >nul 2>nul
if errorlevel 1 (
    echo [X] Khong tim thay lenh dotnet. Cai .NET SDK 8 tro len tai https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo [1/3] Dang build ban web...
dotnet build "DoAnWeb\DoAnWeb.csproj" -c Release -nologo -v q
if errorlevel 1 (
    echo [X] Build loi. Xem thong bao phia tren.
    pause
    exit /b 1
)

echo [2/3] Dang khoi dong web tai %URL% ...
rem Chay web trong cua so rieng (dong cua so do hoac chay stop-web.bat de tat)
start "DoAnWeb - Quan ly gia su" cmd /c "set ASPNETCORE_URLS=%URL%&& set ASPNETCORE_ENVIRONMENT=Production&& dotnet run --project DoAnWeb\DoAnWeb.csproj -c Release --no-build --no-launch-profile"

echo [3/3] Doi web san sang roi mo trinh duyet...
powershell -NoProfile -Command "$u='%URL%/TaiKhoan/DangNhap'; for($i=0;$i -lt 60;$i++){ try { Invoke-WebRequest $u -UseBasicParsing -TimeoutSec 2 | Out-Null; exit 0 } catch { Start-Sleep 1 } }; exit 1"
if errorlevel 1 (
    echo [!] Web chua phan hoi sau 60 giay - kiem tra cua so "DoAnWeb" va SQL Server.
    pause
    exit /b 1
)
start "" "%URL%"
echo Da mo %URL%  (dang nhap: admin / Admin@2026 hoac gs1 / 123456)
exit /b 0
