@echo off
SETLOCAL ENABLEEXTENSIONS

REM Adjust these statements according to your envrionment
REM SET MSBUILD_PATH="%windir%\Microsoft.NET\Framework\v4.0.30319\"
SET MSBUILD_PATH="%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community\MSBuild\15.0\bin"

REM %~dp0 => absolute path of directory where batch file exists
cd %~dp0
SET SOLUTION="%cd%\..\ManagedWimLib.sln"
SET NUGET="%cd%\..\res\nuget.exe"

%MSBUILD_PATH%\MSBuild.exe %SOLUTION% /p:Configuration=Release /target:Rebuild

%NUGET% pack ManagedWimLib.nuspec

ENDLOCAL