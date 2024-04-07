@ECHO off
if "%1"=="" (
    GOTO: HELP
    EXIT /B 1
)
if "%1"=="CLI" (
    SET "boolean=false"
) else if "%1"=="FORMS" (
    SET "boolean=true"
) else (
    ECHO Invalid type: %1
    GOTO :HELP
    EXIT /B 1
)
@REM Add if else to check if %2 is ELECTRON or CLI
if "%2"=="ELECTRON" (
    cd JAMMER.ELECTRON
) else if "%2"=="CLI" (
    cd JAMMER.CLI
) else (
    ECHO Invalid type: %2
    GOTO :HELP
    EXIT /B 1
)

dotnet publish -r win10-x64 -c Release /p:PublishSingleFile=true --self-contained -p:UseForms=%boolean%
SET "sourceFolder=bin\Release\net7.0-windows\win10-x64\publish"

@REM Build with debug executable
@REM dotnet build
@REM SET "sourceFolder=bin\Debug\net7.0\"

SET "targetFolder=nsis"

@REM COPY jammer.exe
COPY /B /Y "%sourceFolder%\jammer.exe" "%targetFolder%\jammer.exe"

COPY /B /Y "libs\win\x64\bass" "%targetFolder%"
COPY /B /Y "libs\win\x64\bass_aac.dll" "%targetFolder%"
COPY /Y "libs\win\x64\bass.dll" %targetFolder%
COPY /Y LICENSE %targetFolder%

mkdir "%targetFolder%\locales"
XCOPY /S /Y locales "%targetFolder%\locales"
SET "start_name=jammer-Setup_V2.0.5.exe"

makensis %targetFolder%\setup.nsi  

IF ErrorLevel 1 (
    ECHO Error building installer
    EXIT /B 1
)

cd nsis

START "" "%start_name%"
EXIT /B 0

:HELP
ECHO Usage: build.bat { CLI ^| FORMS } { ELECTRON ^| CLI }

ECHO CLI - Only barebone CLI version
ECHO FORMS - Includes global key listeners for windows
ECHO ========================
ECHO CLI - CLI version
ECHO ELECTRON - UI-Electron version