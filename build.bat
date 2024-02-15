@ECHO off
dotnet publish -r win10-x64 -c Release /p:PublishSingleFile=true --self-contained

SET "sourceFolder=bin\Release\net7.0\win10-x64\publish"
SET "targetFolder=nsis"

@REM COPY jammer.exe
COPY /B /Y "%sourceFolder%\jammer.exe" "%targetFolder%\jammer.exe"
COPY /Y LICENSE %targetFolder%

makensis %targetFolder%\setup.nsi  
