@ECHO off
dotnet publish -r win10-x64 -c Release /p:PublishSingleFile=true --self-contained
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