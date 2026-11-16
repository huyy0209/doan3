@echo off
rem ==========================================================================
rem  Quan ly gia su - CAI DAT / NANG CAP CSDL
rem  Chay cac script trong thu muc Database theo dung thu tu bang sqlcmd.
rem  Moi script deu chay lai nhieu lan duoc (khong xoa du lieu dang co).
rem
rem  Cach dung:  CaiDatCSDL.bat                (SQL Server tren may nay: ".")
rem              CaiDatCSDL.bat .\SQLEXPRESS   (ten instance khac)
rem  Neu doi ten server, nho sua ConnectionStrings:QuanLyGiaSu trong DoAnWeb\appsettings.json.
rem ==========================================================================
setlocal EnableExtensions
title Quan ly gia su - Cai dat CSDL
set "ROOT=%~dp0.."
cd /d "%ROOT%"

set "SERVER=%~1"
if "%SERVER%"=="" set "SERVER=."
set "CSDL=%~2"
if "%CSDL%"=="" set "CSDL=QuanLyGiaSu_Web"
set "TMPSQL=%TEMP%\caidat_csdl_web.sql"

where sqlcmd >nul 2>nul
if errorlevel 1 (
    echo [X] Khong tim thay lenh sqlcmd.
    echo     Cai "SQL Server Command Line Utilities" hoac mo SSMS va chay lan luot
    echo     cac file trong thu muc Database theo thu tu so.
    pause
    exit /b 1
)

echo Server: %SERVER%   CSDL: %CSDL%
echo.
echo [1/2] Tao CSDL va nang cap cau truc...
for %%f in (01_QuanLyGiaSu 02_NangCap_v2 03_NangCap_v3 04_NangCap_v4 05_NangCap_v5 06_NangCap_v6 07_NangCap_v7 08_NangCap_v8 09_NhatKy 10_ChiMuc_Web 11_ThongBao) do (
    echo     - %%f.sql
    call :chay "%%f"
    if errorlevel 1 goto loi
)

echo.
choice /c CK /n /m "[2/2] Them du lieu mau de chay thu? (C = co, K = khong): "
if errorlevel 2 goto xong
for %%f in (11_DuLieuMau_ThemNhieu 12_DuLieuMau_DiemDanh_HocPhi 13_DuLieuMau_v3 14_DuLieuMau_HocBu 15_DuLieuMau_CapDo 16_DuLieuMau_v6) do (
    echo     - %%f.sql
    call :chay "%%f"
    if errorlevel 1 goto loi
)

:xong
echo.
echo [OK] CSDL da san sang. Chay QuanLyGiaSu.bat web de mo ban web.
echo      Tai khoan quan ly mac dinh: admin / admin123 (doi mat khau sau khi dang nhap).
pause
exit /b 0

:loi
echo.
echo [X] Loi khi chay script tren. Kiem tra SQL Server da bat chua va ten server "%SERVER%" co dung khong.
echo     Chay lai lenh sqlcmd o tren (bo "^>nul") de xem chi tiet loi.
pause
exit /b 1

:chay
rem  Doi ten CSDL trong script (CREATE DATABASE / DB_ID / USE) sang %CSDL% roi chay bang sqlcmd.
powershell -NoProfile -Command "$t = [IO.File]::ReadAllText('Database\%~1.sql'); $t = $t -replace \"DB_ID\(N'QuanLyGiaSu'\)\", \"DB_ID(N'$env:CSDL')\" -replace 'CREATE DATABASE QuanLyGiaSu;', \"CREATE DATABASE $env:CSDL;\" -replace '(?m)^USE QuanLyGiaSu;', \"USE $env:CSDL;\"; [IO.File]::WriteAllText($env:TMPSQL, $t, (New-Object Text.UTF8Encoding($true)))"
if errorlevel 1 exit /b 1
sqlcmd -S "%SERVER%" -E -C -I -b -f 65001 -i "%TMPSQL%" >nul
exit /b %errorlevel%
