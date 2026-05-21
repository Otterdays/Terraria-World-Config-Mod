@echo off
setlocal EnableExtensions EnableDelayedExpansion

REM ============================================================
REM  WorldConfigMod - tModLoader build script
REM ------------------------------------------------------------
REM  Flow:
REM    1. Mirror this folder into ModSources (so ..\tModLoader.targets
REM       import in the .csproj resolves correctly).
REM    2. Prefer "dotnet build" against the .csproj (headless, fast).
REM    3. Fall back to bundled/system dotnet + tModLoader.dll -build.
REM    4. Verify the resulting .tmod exists in tModLoader\Mods.
REM
REM  Env overrides (set before running):
REM    TML_DIR        install dir containing tModLoader.dll
REM    TML_DATA_DIR   "%USERPROFILE%\Documents\My Games\Terraria\tModLoader"
REM    MOD_NAME       override the mod folder / assembly name
REM    BUILD_CONFIG   Release (default) | Debug
REM ============================================================

if "%MOD_NAME%"==""     set "MOD_NAME=WorldConfigMod"
if "%BUILD_CONFIG%"=="" set "BUILD_CONFIG=Release"
set "MOD_SRC_DIR=%~dp0"
if "%MOD_SRC_DIR:~-1%"=="\" set "MOD_SRC_DIR=%MOD_SRC_DIR:~0,-1%"

call :find_tml
if errorlevel 1 exit /b 1

REM ---- Locate ModSources / Mods data dir ----------------------
if not defined TML_DATA_DIR set "TML_DATA_DIR=%USERPROFILE%\Documents\My Games\Terraria\tModLoader"
set "MOD_SOURCES_DIR=!TML_DATA_DIR!\ModSources"
set "TARGET_SRC=!MOD_SOURCES_DIR!\!MOD_NAME!"
set "OUTPUT_TMOD=!TML_DATA_DIR!\Mods\!MOD_NAME!.tmod"
set "TARGETS_FILE=!MOD_SOURCES_DIR!\tModLoader.targets"
set "TARGET_CSPROJ=!TARGET_SRC!\!MOD_NAME!.csproj"

if not exist "!MOD_SOURCES_DIR!" (
    echo [INFO] Creating ModSources dir: !MOD_SOURCES_DIR!
    mkdir "!MOD_SOURCES_DIR!" 2>nul
)

REM ---- Refuse to build if tModLoader is running ---------------
call :check_tml_running
if errorlevel 1 exit /b 2

REM ---- Mirror source into ModSources --------------------------
echo.
echo [STEP 1/3] Mirroring source...
echo            From: !MOD_SRC_DIR!
echo            To:   !TARGET_SRC!
robocopy "!MOD_SRC_DIR!" "!TARGET_SRC!" ^
    /MIR ^
    /XD bin obj .git .vs .claude .idea node_modules DOCS WorldConfigMod.Tests assets .github ^
    /XF build.bat test.bat Test.gui.bat *.user *.suo .gitignore .gitattributes index.html styles.css app.js .nojekyll ^
    /NFL /NDL /NJH /NJS /NC /NS /NP >nul
set "RC=!ERRORLEVEL!"
if !RC! GEQ 8 (
    echo [ERROR] robocopy failed with code !RC!.
    exit /b 3
)

REM ---- Pick build path (SDK dotnet only; bundled runtime cannot "dotnet build") ----
set "USE_DOTNET=0"
if exist "!TARGETS_FILE!" (
    where dotnet >nul 2>nul
    if not errorlevel 1 (
        dotnet --list-sdks 2>nul | findstr /R "[0-9]" >nul
        if not errorlevel 1 set "USE_DOTNET=1"
    )
)

if "!USE_DOTNET!"=="1" goto :build_dotnet
goto :build_tml

:build_dotnet
echo.
echo [STEP 2/3] Building via dotnet (!BUILD_CONFIG!)...
dotnet build "!TARGET_CSPROJ!" -c !BUILD_CONFIG! -v minimal
set "BUILD_RC=!ERRORLEVEL!"
if not "!BUILD_RC!"=="0" (
    echo [WARN] dotnet build failed, falling back to tModLoader -build...
    goto :build_tml
)
goto :verify

:build_tml
echo.
if not exist "!TARGETS_FILE!" (
    echo [INFO] tModLoader.targets not found in ModSources.
    echo        Launch tModLoader once to generate it, or rely on -build fallback.
)
echo [STEP 2/3] Building via tModLoader.dll -build (this can take 30-90s)...
pushd "!TML_DIR!"
"!TML_DOTNET!" tModLoader.dll -build "!TARGET_SRC!"
set "BUILD_RC=!ERRORLEVEL!"
popd
if not "!BUILD_RC!"=="0" (
    echo [ERROR] tModLoader build failed with exit code !BUILD_RC!.
    echo         Check log: !TML_DATA_DIR!\Logs\Logs.log
    exit /b 4
)

:verify
echo.
echo [STEP 3/3] Verifying .tmod output...
if not exist "!OUTPUT_TMOD!" (
    echo [WARN] Build returned success but .tmod not found at:
    echo        !OUTPUT_TMOD!
    echo        Check log: !TML_DATA_DIR!\Logs\Logs.log
    exit /b 5
)

for %%I in ("!OUTPUT_TMOD!") do set "TMOD_SIZE=%%~zI"
echo.
echo  ============================================================
echo   BUILD OK
echo   File: !OUTPUT_TMOD!
echo   Size: !TMOD_SIZE! bytes
echo  ============================================================
echo.
echo  Enable via tModLoader main menu -^> Workshop / Mods -^> !MOD_NAME!.
exit /b 0

REM ============================================================
:find_tml
REM Modern tML ships tModLoader.dll (+ bundled dotnet), not tModLoader.exe.
set "TML_DIR="
set "TML_DLL="
set "TML_DOTNET="

if defined TML_DIR call :set_tml_paths "!TML_DIR!"
if defined TML_DLL goto :find_tml_done

REM Registry Steam path (handles custom library on C:)
for /f "tokens=2,*" %%A in ('reg query "HKCU\Software\Valve\Steam" /v SteamPath 2^>nul') do (
    set "STEAM_PATH=%%B"
)
if defined STEAM_PATH (
    set "STEAM_PATH=!STEAM_PATH:/=\!"
    call :set_tml_paths "!STEAM_PATH!\steamapps\common\tModLoader"
)

if not defined TML_DLL (
    for %%D in (
        "C:\Program Files (x86)\Steam\steamapps\common\tModLoader"
        "C:\Program Files\Steam\steamapps\common\tModLoader"
        "D:\SteamLibrary\steamapps\common\tModLoader"
        "E:\SteamLibrary\steamapps\common\tModLoader"
        "F:\SteamLibrary\steamapps\common\tModLoader"
    ) do (
        if not defined TML_DLL call :set_tml_paths "%%~D"
    )
)

:find_tml_done
if not defined TML_DLL (
    echo [ERROR] tModLoader install not found.
    echo         Expected tModLoader.dll under your Steam tModLoader folder.
    echo         Set TML_DIR to that folder and retry.
    exit /b 1
)
echo [INFO] tModLoader: !TML_DIR!
echo [INFO] dotnet:       !TML_DOTNET!
exit /b 0

:set_tml_paths
set "TRY_DIR=%~1"
if not exist "!TRY_DIR!\tModLoader.dll" exit /b 0
set "TML_DIR=!TRY_DIR!"
set "TML_DLL=!TRY_DIR!\tModLoader.dll"
if exist "!TRY_DIR!\dotnet\dotnet.exe" (
    set "TML_DOTNET=!TRY_DIR!\dotnet\dotnet.exe"
) else (
    set "TML_DOTNET=dotnet"
)
exit /b 0

:check_tml_running
tasklist /FI "IMAGENAME eq tModLoader.exe" 2>nul | find /I "tModLoader.exe" >nul
if not errorlevel 1 (
    echo [ERROR] tModLoader is running. Close it and retry.
    exit /b 1
)
powershell -NoProfile -Command ^
  "$p = Get-CimInstance Win32_Process -Filter \"name='dotnet.exe'\" -ErrorAction SilentlyContinue | Where-Object { $_.CommandLine -like '*tModLoader.dll*' }; if ($p) { exit 1 } else { exit 0 }"
if not errorlevel 1 exit /b 0
echo [ERROR] tModLoader (dotnet + tModLoader.dll) is running. Close it and retry.
exit /b 1
