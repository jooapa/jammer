@ECHO off
@REM dotnet publish -r win10-x64 -c Release /p:PublishSingleFile=true --self-contained

SET "sourceFolder=bin\Release\net7.0\win10-x64\publish"
SET "targetFolder=nsis"

@REM COPY jammer.exe
@REM COPY /B /Y "%sourceFolder%\jammer.exe" "%targetFolder%\jammer.exe"
@REM COPY /Y LICENSE %targetFolder%

makensis %targetFolder%\setup.nsi

IF not EXIST ".\build\" MKDIR .\build\
COPY /B /Y "%targetFolder%\jammer-Setup_V1.X.X.exe" ".\build\"

.\build\jammer-Setup_V1.X.X.exe
