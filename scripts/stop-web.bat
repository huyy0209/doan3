@echo off
rem ==========================================================================
rem  Quan ly gia su - TAT BAN WEB (tien trinh DoAnWeb do start-web.bat mo).
rem  Khong dung toi ban WinForms (DoAnWD.exe).
rem ==========================================================================
setlocal EnableExtensions
chcp 65001 >nul
set "ROOT=%~dp0.."
cd /d "%ROOT%"

powershell -NoProfile -Command ^
  "$ps = Get-CimInstance Win32_Process | Where-Object { $_.Name -eq 'DoAnWeb.exe' -or ($_.Name -eq 'dotnet.exe' -and $_.CommandLine -match 'DoAnWeb') };" ^
  "if (-not $ps) { Write-Host 'Ban web khong chay.'; exit 0 };" ^
  "$ps | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue; Write-Host ('Da tat tien trinh ' + $_.Name + ' (PID ' + $_.ProcessId + ')') }"
exit /b 0
