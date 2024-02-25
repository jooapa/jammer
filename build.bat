@ECHO off
dotnet publish -r win10-x64 -c Release /p:PublishSingleFile=true --self-contained

SET "sourceFolder=bin\Release\net7.0\win10-x64\publish"
SET "targetFolder=nsis"

@REM COPY jammer.exe
COPY /B /Y "%sourceFolder%\jammer.exe" "%targetFolder%\jammer.exe"
COPY /Y LICENSE %targetFolder%

makensis %targetFolder%\setup.nsi  

IF ErrorLevel 1 (
    ECHO Error building installer
    EXIT /B 1
)

cd nsis
SET "start_name=jammer-Setup"  REM Set the start name of the files you want to run

REM Loop through files in the current directory with the specified start name
for %%F in ("%start_name%*.exe") do (
    ECHO Running %%F
    START "" "%%F"
)