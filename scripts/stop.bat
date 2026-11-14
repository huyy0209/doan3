@echo off
setlocal EnableExtensions
chcp 65001 >nul
title Quan ly gia su - Dung ung dung
set "ROOT=%~dp0.."
cd /d "%ROOT%"

tasklist /FI "IMAGENAME eq DoAnWD.exe" /NH | find /I "DoAnWD.exe" >nul
if errorlevel 1 (
    echo [!] Ung dung DoAnWD.exe hien khong chay.
    exit /b 0
)

taskkill /F /IM DoAnWD.exe /T >nul 2>nul
if errorlevel 1 (
    echo [X] Tim thay DoAnWD.exe nhung khong the dung tien trinh. Hay thu lai voi quyen phu hop.
    exit /b 1
)

echo [OK] Da dung DoAnWD.exe.
exit /b 0
