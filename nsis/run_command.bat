@ECHO OFF
set "ACTION=%1"
set "COMMAND=%2"
set "CALLCMD="
ECHO.

IF "%ACTION%"=="update" (
    ECHO Updating Jammer...
    cd /D C:\Users\%USERNAME%\jammer
    if exist ".\temp" rd /s /q "temp"
    md ".\temp"
    move /Y "%COMMAND%" ".\temp"
    DEL /Q "Jammer-Setup*.exe"
    move /Y ".\temp\Jammer-Setup*.exe" "%COMMAND%"
    rd /s /q "temp"

    set "CALLCMD=%COMMAND%"
    GOTO CALLCOMMAND
)
exit /B 0

:CALLCOMMAND
ECHO Calling "%CALLCMD%"...
call "%CALLCMD%"
ECHO "Done..."
ECHO "Press enter to exit..."