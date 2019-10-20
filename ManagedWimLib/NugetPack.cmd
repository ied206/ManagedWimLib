@echo off
SETLOCAL ENABLEEXTENSIONS

REM %~dp0 => absolute path of directory where batch file exists
cd %~dp0
SET BASE=%~dp0

dotnet clean -c Release
dotnet build -c Release
dotnet pack -c Release -o %BASE%

ENDLOCAL