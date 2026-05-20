@echo off
setlocal EnableExtensions
echo Running WorldConfigMod unit tests...
dotnet test "%~dp0WorldConfigMod.Tests\WorldConfigMod.Tests.csproj" -c Release -v minimal
exit /b %ERRORLEVEL%
