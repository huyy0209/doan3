@echo off
rem ==========================================================================
rem  Quan ly gia su - KHOI DONG NHANH
rem  Bam dup file nay: tu build ban moi nhat (neu may co Visual Studio) roi mo ung dung.
rem  Lan dau chay can CSDL: chay CaiDatCSDL.bat truoc.
rem ==========================================================================
setlocal EnableExtensions
title Quan ly gia su - Khoi dong nhanh
cd /d "%~dp0"

set "PROJ=DoAnWD\DoAnWD.csproj"
set "EXE=DoAnWD\bin\Release\DoAnWD.exe"
set "MSBUILD="
set "VSWHERE=%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe"

rem --- 1. Tim MSBuild di kem Visual Studio -----------------------------------
if not exist "%VSWHERE%" goto khong_msbuild
for /f "usebackq delims=" %%i in (`"%VSWHERE%" -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe`) do set "MSBUILD=%%i"
if not defined MSBUILD goto khong_msbuild

rem --- 2. Build ban Release (chi build lai phan da thay doi nen lan sau rat nhanh)
echo [1/2] Dang build ung dung (lan dau co the mat 1-2 phut)...
"%MSBUILD%" "%PROJ%" -restore -p:RestorePackagesConfig=true "-p:SolutionDir=%~dp0\" -p:Configuration=Release -nologo -v:quiet -m
if errorlevel 1 goto build_loi
goto chay

:khong_msbuild
if exist "%EXE%" (
    echo [!] Khong tim thay Visual Studio/MSBuild - mo ban da build truoc do.
    goto chay
)
echo [X] Khong tim thay Visual Studio de build ung dung.
echo     Cai Visual Studio 2022 tro len (workload ".NET desktop development")
echo     hoac mo DoAnWD.slnx bang Visual Studio va bam Build mot lan.
pause
exit /b 1

:build_loi
echo.
echo [X] Build loi. Xem thong bao phia tren, hoac mo DoAnWD.slnx bang Visual Studio de xem chi tiet.
pause
exit /b 1

rem --- 3. Mo ung dung --------------------------------------------------------
:chay
if not exist "%EXE%" (
    echo [X] Khong thay file %EXE%
    pause
    exit /b 1
)
echo [2/2] Dang mo ung dung...
start "" "%EXE%"
exit /b 0
