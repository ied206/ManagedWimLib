@echo off
SETLOCAL ENABLEEXTENSIONS

REM %~dp0 => absolute path of directory where batch file exists
cd %~dp0
SET NUGET="%cd%\..\res\nuget.exe"

dotnet clean -c Release
dotnet build -c Release
%NUGET% pack ManagedWimLib.nuspec

ENDLOCAL