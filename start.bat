@echo off
setlocal EnableExtensions
chcp 65001 >nul
title Quan ly gia su - Khoi dong
cd /d "%~dp0"

set "PROJ=DoAnWD\DoAnWD.csproj"
set "EXE=DoAnWD\bin\Release\DoAnWD.exe"
set "MSBUILD="
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"

if not exist "%VSWHERE%" goto khong_msbuild
for /f "usebackq delims=" %%i in (`"%VSWHERE%" -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do set "MSBUILD=%%i"
if not defined MSBUILD goto khong_msbuild

echo [1/2] Dang build ung dung cau hinh Release...
"%MSBUILD%" "%PROJ%" -restore -p:RestorePackagesConfig=true "-p:SolutionDir=%~dp0\" -p:Configuration=Release -nologo -v:quiet -m
if errorlevel 1 goto build_loi

if not exist "%EXE%" (
    echo [X] Build xong nhung khong tim thay file "%EXE%".
    pause
    exit /b 1
)

echo [2/2] Dang mo ung dung...
start "" "%EXE%"
exit /b 0

:khong_msbuild
if exist "%EXE%" (
    echo [!] Khong tim thay Visual Studio/MSBuild - mo ban da build truoc do.
    start "" "%EXE%"
    exit /b 0
)
echo [X] Khong tim thay MSBuild cua Visual Studio.
echo     Cai Visual Studio 2022 tro len voi workload ".NET desktop development".
echo     Neu da cai, hay kiem tra vswhere.exe trong thu muc cai dat Visual Studio.
pause
exit /b 1

:build_loi
echo.
echo [X] Build Release that bai. Xem thong bao loi phia tren va thu lai sau khi sua loi.
pause
exit /b 1
