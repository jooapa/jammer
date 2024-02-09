dotnet publish -r win10-x64 -c Release /p:PublishSingleFile=true --self-contained

@echo off
set "sourceFolder=bin\Release\net7.0\win10-x64\publish"
set "targetFolder=nsis-folder"

rem Copy jammer.exe
copy /y "%sourceFolder%\jammer.exe" "%targetFolder%\jammer.exe"

echo Files copied successfully.

makensis setup.nsi

rem if program files (x86)/jammer exists, run C:\Program Files (x86)\jammer\Uninstall.exe
if exist "C:\Program Files (x86)\jammer" (
    echo Uninstalling previous version...
    start "" "C:\Program Files (x86)\jammer\Uninstall.exe"
)

jammer-Setup_V1.X.X.exe
