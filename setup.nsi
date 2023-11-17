!define APP_NAME "jammer"
!define EXE_NAME "jammer.exe"

; Request administrative privileges
RequestExecutionLevel admin

; Set the output folder for the installer
Outfile "jammer-Setup.exe"

; Default section
Section
SetOutPath "$PROGRAMFILES\${APP_NAME}"

; Create "jammer" folder
CreateDirectory "$PROGRAMFILES\${APP_NAME}"

; Copy files to the installation directory
File /r "bin\Release\net6.0\win10-x64\publish\*.*"

; Execute PowerShell script to update PATH
nsExec::ExecToStack 'powershell.exe -ExecutionPolicy Bypass -File "$INSTDIR\setup.ps1"'
Pop $0 ; return value


SectionEnd
