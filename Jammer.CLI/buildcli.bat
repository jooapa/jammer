@ECHO OFF
cd Jammer.CLI
dotnet publish -r win10-x64 -c Release /p:PublishSingleFile=true -p:DefineConstants="CLI_UI" --self-contained
SET "sourceFolder=bin\Release\net7.0\win10-x64\publish"

@REM Build with debug executable
@REM dotnet build
@REM SET "sourceFolder=bin\Debug\net7.0\"

SET "targetFolder=..\nsis"

@REM COPY Jammer.exe
COPY /B /Y "%sourceFolder%\Jammer.CLI.exe" "%targetFolder%\Jammer.exe"

COPY /B /Y "libs\win\x64\bass" "%targetFolder%"
COPY /B /Y "libs\win\x64\bass_aac.dll" "%targetFolder%"
COPY /Y "libs\win\x64\bass.dll" %targetFolder%
COPY /Y LICENSE %targetFolder%

mkdir "%targetFolder%\locales"
XCOPY /S /Y ..\locales "%targetFolder%\locales"
SET "start_name=Jammer-Setup_V2.0.7.exe"

makensis %targetFolder%\setup.nsi  

IF ErrorLevel 1 (
    ECHO Error building installer
    EXIT /B 1
)

cd ..\nsis

START "" "%start_name%"
EXIT /B 0