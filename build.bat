@ECHO off
dotnet publish -r win10-x64 -c Release /p:PublishSingleFile=true --self-contained

SET "sourceFolder=bin\Release\net7.0\win10-x64\publish"
SET "targetFolder=nsis"

@REM COPY jammer.exe
COPY /B /Y "%sourceFolder%\jammer.exe" "%targetFolder%\jammer.exe"
COPY /Y "libs\win\x64\bass.dll" %targetFolder%
COPY /Y LICENSE %targetFolder%

mkdir "%targetFolder%\locales"
XCOPY /S /Y locales "%targetFolder%\locales"
SET "start_name=jammer-Setup"  REM Set the start name of the files you want to run

for %%F in ("%start_name%*.exe") do (
    DEL "" "%%F"
)

makensis %targetFolder%\setup.nsi  

IF ErrorLevel 1 (
    ECHO Error building installer
    EXIT /B 1
)

cd nsis
REM Loop through files in the current directory with the specified start name



REM Loop through files in the current directory with the specified start name
for %%F in ("%start_name%*.exe") do (
    ECHO Running %%F
    START "" "%%F"
)