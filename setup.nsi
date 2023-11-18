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

; Copy the icon file to the installation directory
File "jammer.ico"

; Create shortcut to sendto folder in AppData\Roaming\Microsoft\Windows\SendTo
CreateShortCut "$SENDTO\${APP_NAME}.lnk" "$PROGRAMFILES\${APP_NAME}\${EXE_NAME}" "" "$PROGRAMFILES\${APP_NAME}\jammer.ico" 0


SectionEnd

Section "Uninstall"

    ; Remove files
    Delete "$PROGRAMFILES\${APP_NAME}\${EXE_NAME}"
    Delete "$PROGRAMFILES\${APP_NAME}\jammer.ico"

    ; Remove directory
    RMDir "$PROGRAMFILES\${APP_NAME}"

    ; Remove shortcut
    Delete "$SENDTO\${APP_NAME}.lnk"

SectionEnd