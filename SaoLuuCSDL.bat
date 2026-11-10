@echo off
rem ==========================================================================
rem  Quan ly gia su - SAO LUU CSDL
rem  Tao file .bak trong thu muc sao luu mac dinh cua SQL Server, ten co ngay gio.
rem  Cach dung:  SaoLuuCSDL.bat              (SQL Server tren may nay)
rem              SaoLuuCSDL.bat .\SQLEXPRESS
rem  Khoi phuc: mo SSMS > chuot phai Databases > Restore Database > chon file .bak
rem ==========================================================================
setlocal EnableExtensions
title Quan ly gia su - Sao luu CSDL
set "SERVER=%~1"
if "%SERVER%"=="" set "SERVER=."

where sqlcmd >nul 2>nul
if errorlevel 1 (
    echo [X] Khong tim thay lenh sqlcmd. Cai "SQL Server Command Line Utilities" hoac sao luu bang SSMS.
    pause
    exit /b 1
)

for /f %%i in ('powershell -NoProfile -Command "Get-Date -Format yyyyMMdd_HHmm"') do set "MOC=%%i"
set "TEN=QuanLyGiaSu_%MOC%.bak"
echo Dang sao luu CSDL QuanLyGiaSu tren server %SERVER% ...
sqlcmd -S "%SERVER%" -E -C -b -Q "BACKUP DATABASE QuanLyGiaSu TO DISK = N'%TEN%' WITH INIT, NAME = N'QuanLyGiaSu %MOC%'; SELECT TOP 1 N'File: ' + physical_device_name FROM msdb.dbo.backupmediafamily ORDER BY media_set_id DESC;"
if errorlevel 1 (
    echo.
    echo [X] Sao luu that bai. Kiem tra SQL Server da bat chua va tai khoan Windows co quyen sao luu.
    pause
    exit /b 1
)
echo.
echo [OK] Da sao luu. Hay chep file .bak o duong dan tren sang USB hoac o dia khac.
pause
exit /b 0
