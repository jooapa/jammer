@ECHO OFF
IF NOT pwd==%cd% (cd /d %~dp0)

@REM DEL /Q /S *.exe

SET "RELEASE_VERSION=win-x64"                                       @REM target runtime
SET "sourceFolder=bin\Release\net8.0\%RELEASE_VERSION%\publish"     @REM location of the published files
SET "targetFolder=..\nsis"                                          @REM nsin location
SET "start_name=Jammer-Setup_V2.8.0.0.exe"

dotnet publish -r %RELEASE_VERSION% -c Release /p:PublishSingleFile=true -p:DefineConstants="CLI_UI" --self-contained 

if %ERRORLEVEL% NEQ 0 (
    ECHO Error building CLI
    EXIT /B 1
)

ECHO =========================================
ECHO ^| Copying source files to target folder ^|
ECHO =========================================
ECHO.

ECHO Copying "%sourceFolder%\Jammer.CLI.exe" to "%targetFolder%\Jammer.exe"
COPY /B /Y "%sourceFolder%\Jammer.CLI.exe" "%targetFolder%\Jammer.exe" 1>NUL
CALL :COPY_ERRORCHECK
ECHO.

ECHO Copying "%sourceFolder%\Jammer.CLI.pdb" to "%targetFolder%\Jammer.pdb"
COPY /B /Y "%sourceFolder%\uiohook.dll" "%targetFolder%\uiohook.dll" 1>NUL
CALL :COPY_ERRORCHECK
ECHO.

ECHO Copying "%sourceFolder%\uiohook.dll" to "%targetFolder%\uiohook.dll"
COPY /B /Y "..\icons\trans_icon512x512.ico" "%targetFolder%\Jammer.ico" 1>NUL
CALL :COPY_ERRORCHECK
ECHO.

ECHO Copying "%sourceFolder%\uiohook.dll" to "%targetFolder%\uiohook.dll"
COPY /B /Y "libs\win\x64\bass" "%targetFolder%" 1>NUL
CALL :COPY_ERRORCHECK
ECHO.

ECHO Copying "%sourceFolder%\uiohook.dll" to "%targetFolder%\uiohook.dll"
COPY /B /Y "libs\win\x64\bass_aac.dll" "%targetFolder%" 1>NUL
CALL :COPY_ERRORCHECK
ECHO.

ECHO Copying "%sourceFolder%\uiohook.dll" to "%targetFolder%\uiohook.dll"
COPY /B /Y "libs\win\x64\bass.dll" %targetFolder% 1>NUL
CALL :COPY_ERRORCHECK
ECHO.

ECHO Copying "%sourceFolder%\uiohook.dll" to "%targetFolder%\uiohook.dll"
COPY /B /Y LICENSE %targetFolder% 1>NUL
CALL :COPY_ERRORCHECK
ECHO.

if not EXIST "%targetFolder%\docs" MKDIR "%targetFolder%\docs"
ECHO Copying "..\docs\console_styling.html" to "%targetFolder%\docs\console_styling.html"
COPY /B /Y "..\docs\console_styling.html" "%targetFolder%\docs\console_styling.html" 1>NUL
CALL :COPY_ERRORCHECK
ECHO.

IF NOT EXIST "%targetFolder%\locales" MKDIR "%targetFolder%\locales"

ECHO Copying "..\locales" to "%targetFolder%\locales"
XCOPY /S /Y ..\locales "%targetFolder%\locales" 1>NUL
CALL :COPY_ERRORCHECK
ECHO.

makensis %targetFolder%\setup.nsi  
IF %ERRORLEVEL% NEQ 0 (
    ECHO Error building installer
    EXIT /B 1
)

cd ..\nsis

call "%start_name%"
cd /d %~dp0

GOTO :EOF

:COPY_ERRORCHECK
IF %ERRORLEVEL% NEQ 0 (
    ECHO Error copying file^(s^)
) ELSE (
    ECHO File^(s^) copied successfully
)
GOTO :EOF