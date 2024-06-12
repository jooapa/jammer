@echo OFF
IF NOT pwd==%cd% (cd /d %~dp0) 
RMDIR /S /Q bin
RMDIR /S /Q obj
cd ..
cd Jammer.Core
RMDIR /S /Q bin
RMDIR /S /Q obj
cd ..\Jammer.CLI