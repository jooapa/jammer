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

if "%2"=="AVALONIA" (
    cd Jammer.Avalonia
) else if "%2"=="CLI" (
    cd Jammer.CLI
) else (
    ECHO Invalid type: %2
    GOTO :HELP
    EXIT /B 1
)


dotnet run -p:UseForms=%boolean%
cd ..
EXIT /B 0

:HELP
ECHO Usage: build.bat { CLI ^| FORMS } { ELECTRON ^| CLI }

ECHO CLI - Only barebone CLI version
ECHO FORMS - Includes global key listeners for windows
ECHO ========================
ECHO CLI - CLI version
ECHO Avalonia - Avalonia version