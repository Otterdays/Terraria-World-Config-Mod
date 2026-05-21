@echo off
setlocal EnableExtensions

title WorldConfigMod - Full Test Suite
color 0B

set "ROOT=%~dp0"
set "PROJECT=%ROOT%WorldConfigMod.Tests\WorldConfigMod.Tests.csproj"

echo.
echo  ============================================================
echo   WorldConfigMod - Full Test Suite
echo  ============================================================
echo.

if not exist "%PROJECT%" (
    color 0C
    echo [ERROR] Test project not found:
    echo         %PROJECT%
    echo.
    pause
    exit /b 1
)

where dotnet >nul 2>nul
if errorlevel 1 (
    color 0C
    echo [ERROR] dotnet SDK not found on PATH.
    echo         Install .NET 8 SDK, then rerun this script.
    echo.
    pause
    exit /b 1
)

for /f "usebackq delims=" %%V in (`dotnet --version`) do set "DOTNET_VERSION=%%V"
echo [INFO] dotnet SDK: %DOTNET_VERSION%
echo [INFO] project:    %PROJECT%
echo.

echo [STEP 1/2] Discovering tests...
echo ------------------------------------------------------------
dotnet test "%PROJECT%" -c Release --list-tests -v minimal
if errorlevel 1 goto :fail

echo.
echo [STEP 2/2] Running full suite with detailed console output...
echo ------------------------------------------------------------
dotnet test "%PROJECT%" -c Release -v normal --logger "console;verbosity=detailed"
if errorlevel 1 goto :fail

color 0A
echo.
echo  ============================================================
echo   TEST SUITE PASSED
echo  ============================================================
echo.
pause
exit /b 0

:fail
color 0C
echo.
echo  ============================================================
echo   TEST SUITE FAILED
echo  ============================================================
echo.
pause
exit /b 1
