@echo off
setlocal EnableExtensions
REM Release build, minimal output. Full discovery + detailed run: Test.gui.bat
echo Running WorldConfigMod unit tests (59 cases, Release)...
dotnet test "%~dp0WorldConfigMod.Tests\WorldConfigMod.Tests.csproj" -c Release -v minimal
exit /b %ERRORLEVEL%
