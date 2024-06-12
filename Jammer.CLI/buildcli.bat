@ECHO OFF
setlocal
IF NOT pwd==%cd% (cd /d %~dp0) 
cd Jammer.CLI
dotnet publish -r win10-x64 -c Release /p:PublishSingleFile=true -p:DefineConstants="CLI_UI" --self-contained
SET "sourceFolder=bin\Release\net8.0\win10-x64\publish"

@REM Build with debug executable
@REM dotnet build
@REM SET "sourceFolder=bin\Debug\net8.0\"

SET "targetFolder=..\nsis"

@REM COPY Jammer.exe
COPY /B /Y "%sourceFolder%\Jammer.CLI.exe" "%targetFolder%\Jammer.exe"
COPY /B /Y "%sourceFolder%\uiohook.dll" "%targetFolder%\uiohook.dll"

COPY /B /Y "..\icons\trans_icon512x512.ico" "%targetFolder%\Jammer.ico"
COPY /B /Y "libs\win\x64\bass" "%targetFolder%"
COPY /B /Y "libs\win\x64\bass_aac.dll" "%targetFolder%"
COPY /Y "libs\win\x64\bass.dll" %targetFolder%
COPY /Y LICENSE %targetFolder%

mkdir "%targetFolder%\locales"
XCOPY /S /Y ..\locales "%targetFolder%\locales"
SET "start_name=Jammer-Setup_V2.7.12.13.exe"

makensis %targetFolder%\setup.nsi  

IF %ERRORLEVEL% NEQ 0 (
    ECHO Error building installer
    EXIT /B 1
)

cd ..\nsis

START "" "%start_name%"
cd /d %~dp0
