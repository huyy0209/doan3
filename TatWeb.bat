@echo off
rem ==========================================================================
rem  Quan ly gia su - TAT WEB (dich vu Windows + link cong khai Tailscale Funnel).
rem  Nhap dup la chay; tu xin quyen Administrator.
rem ==========================================================================
setlocal EnableExtensions
chcp 65001 >nul
net session >nul 2>&1
if errorlevel 1 (
  powershell -NoProfile -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
  exit /b
)
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0scripts\bat-tat-web.ps1" -Che Tat
echo.
pause
