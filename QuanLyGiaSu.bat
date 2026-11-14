@echo off
rem ==========================================================================
rem  Quản lý gia sư - BẢN WEB: menu khởi động duy nhất.
rem  Chạy thẳng: QuanLyGiaSu.bat web|stop|cai-csdl|sao-luu|shortcut
rem  Các script con nằm trong thư mục scripts\
rem ==========================================================================
setlocal EnableExtensions
chcp 65001 >nul
title Quản lý gia sư - Bản web

if /I "%~1"=="web" goto mo_web
if /I "%~1"=="stop" goto tat_web
if /I "%~1"=="cai-csdl" goto cai_csdl
if /I "%~1"=="sao-luu" goto sao_luu
if /I "%~1"=="shortcut" goto tao_shortcut
if /I not "%~1"=="" goto lenh_khong_hop_le

:menu
cls
echo.
echo  ╔══════════════════════════════════════════════════════════╗
echo  ║             QUẢN LÝ GIA SƯ - BẢN WEB                     ║
echo  ╠══════════════════════════════════════════════════════════╣
echo  ║  1. Mở bản web (trình duyệt)                             ║
echo  ║  2. Tắt bản web                                          ║
echo  ║  3. Cài / nâng cấp CSDL                                  ║
echo  ║  4. Sao lưu CSDL                                         ║
echo  ║  5. Tạo shortcut "Quản lý gia sư" ra Desktop             ║
echo  ║  0. Thoát                                                ║
echo  ╚══════════════════════════════════════════════════════════╝
echo.
choice /c 123450 /n /m "Chọn chức năng: "
if errorlevel 6 goto thoat
if errorlevel 5 goto menu_shortcut
if errorlevel 4 goto menu_sao_luu
if errorlevel 3 goto menu_cai_csdl
if errorlevel 2 goto menu_tat_web
if errorlevel 1 goto menu_mo_web
goto menu

:menu_mo_web
call "%~dp0scripts\start-web.bat"
goto menu_cho_tiep

:menu_tat_web
call "%~dp0scripts\stop-web.bat"
goto menu_cho_tiep

:menu_cai_csdl
call "%~dp0scripts\CaiDatCSDL.bat"
goto menu_cho_tiep

:menu_sao_luu
call "%~dp0scripts\SaoLuuCSDL.bat"
goto menu_cho_tiep

:menu_shortcut
call :shortcut
goto menu_cho_tiep

:menu_cho_tiep
echo.
pause
goto menu

:mo_web
call "%~dp0scripts\start-web.bat"
exit /b %ERRORLEVEL%

:tat_web
call "%~dp0scripts\stop-web.bat"
exit /b %ERRORLEVEL%

:cai_csdl
if "%~2"=="" (
    call "%~dp0scripts\CaiDatCSDL.bat"
) else (
    call "%~dp0scripts\CaiDatCSDL.bat" "%~2"
)
exit /b %ERRORLEVEL%

:sao_luu
if "%~2"=="" (
    call "%~dp0scripts\SaoLuuCSDL.bat"
) else (
    call "%~dp0scripts\SaoLuuCSDL.bat" "%~2"
)
exit /b %ERRORLEVEL%

:tao_shortcut
call :shortcut
exit /b %ERRORLEVEL%

:shortcut
powershell -NoProfile -Command "$desktop = [Environment]::GetFolderPath('Desktop'); $shell = New-Object -ComObject WScript.Shell; $shortcut = $shell.CreateShortcut((Join-Path $desktop 'Quản lý gia sư.lnk')); $shortcut.TargetPath = '%~f0'; $shortcut.WorkingDirectory = '%~dp0'; $icon = Join-Path '%~dp0' 'assets\QuanLyGiaSu.ico'; if (Test-Path $icon) { $shortcut.IconLocation = $icon }; $shortcut.Save()"
if errorlevel 1 (
    echo [X] Không tạo được shortcut trên Desktop.
    exit /b 1
)
echo [OK] Đã tạo shortcut "Quản lý gia sư" trên Desktop.
exit /b 0

:lenh_khong_hop_le
echo [X] Lệnh không hợp lệ: %~1
echo     Dùng: QuanLyGiaSu.bat web^|stop^|cai-csdl^|sao-luu^|shortcut
exit /b 2

:thoat
exit /b 0
