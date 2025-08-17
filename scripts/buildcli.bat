@ECHO OFF
IF NOT pwd==%cd% (cd /d %~dp0)
cd ..\Jammer.CLI
SET "RELEASE_VERSION=win-x64"                                       
SET "sourceFolder=bin\Release\net8.0\%RELEASE_VERSION%\publish"     
SET "sourceFolder2=bin\Release\net8.0\%RELEASE_VERSION%"     
SET "targetFolder=..\nsis"                                          
SET "start_name=Jammer-Setup_V3.49.exe"

dotnet publish -r %RELEASE_VERSION% -c Release /p:PublishSingleFile=true -p:DefineConstants="CLI_UI" --self-contained 

if %ERRORLEVEL% NEQ 0 (
    ECHO Error building CLI
    EXIT /B 1
)


ECHO \Jammer.CLI.exe"
COPY /B /Y "%sourceFolder%\Jammer.CLI.exe" "%targetFolder%\Jammer.exe" 1>NUL
ECHO.

ECHO \Jammer.CLI.pdb"
COPY /B /Y "%sourceFolder2%\uiohook.dll" "%targetFolder%\uiohook.dll" 1>NUL
ECHO.

ECHO \trans_icon512x512.dll"
COPY /B /Y "..\icons\trans_icon512x512.ico" "%targetFolder%\Jammer.ico" 1>NUL
ECHO.

ECHO \bassmidi.dll"
COPY /B /Y "../libs\win\x64\bassmidi.dll" "%targetFolder%" 1>NUL
ECHO.

ECHO \bass_aac.dll"
COPY /B /Y "../libs\win\x64\bass_aac.dll" "%targetFolder%" 1>NUL
ECHO.

ECHO \bass.dll"
COPY /B /Y "../libs\win\x64\bass.dll" %targetFolder% 1>NUL
ECHO.

ECHO \bassopus.dll"
COPY /B /Y "../libs\win\x64\bassopus.dll" %targetFolder% 1>NUL
ECHO.

ECHO \ffmepg.exer"
COPY /B /Y "../libs\win\x64\ffmpeg.exe" %targetFolder% 1>NUL
ECHO.

cd ..
ECHO Copying LICENSE
COPY /B /Y "LICENSE" "nsis/" 1>NUL
ECHO.
cd Jammer.CLI

::--------------------------------------------------- 
if not EXIST "%targetFolder%\docs" MKDIR "%targetFolder%\docs"
ECHO Copying "..\docs\console_styling.html" to "%targetFolder%\docs\console_styling.html"
COPY /B /Y "..\docs\console_styling.html" "%targetFolder%\docs\console_styling.html" 1>NUL
ECHO.

IF NOT EXIST "%targetFolder%\locales" MKDIR "%targetFolder%\locales"

ECHO Copying "..\locales" to "%targetFolder%\locales"
XCOPY /S /Y ..\locales "%targetFolder%\locales" 1>NUL
ECHO.
:: ....--------------....--------------....--------------....--------------....--------------....--------------....--------------....--------------....--------------....--------------....--------------....--------------....--------------....--------------....--------------

cd ..
"C:\Program Files (x86)\NSIS\Bin\makensis.exe" "nsis\setup.nsi"
IF %ERRORLEVEL% NEQ 0 (
    ECHO Error building installer
    EXIT /B 1
)

@REM call %start_name%
@REM cd /d %~dp0

@REM GOTO :EOF

@REM :COPY_ERRORCHECK
@REM IF %ERRORLEVEL% NEQ 0 (
@REM     ECHO Error copying file^(s^)
@REM ) ELSE (
@REM     ECHO File^(s^) copied successfully
@REM )
@REM GOTO :EOF