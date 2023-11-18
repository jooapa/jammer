!define APP_NAME "jammer"
!define EXE_NAME "jammer.exe"

; Request administrative privileges
RequestExecutionLevel admin

; Set the output folder for the installer
Outfile "jammer-Setup.exe"

; Default section
Section

; Remove existing "jammer" folder if it exists
RMDir /r "$PROGRAMFILES\${APP_NAME}"

; Set output path to install "jammer" folder
SetOutPath "$PROGRAMFILES\${APP_NAME}"

; Create "jammer" folder
CreateDirectory "$PROGRAMFILES\${APP_NAME}"

; Copy files to the installation directory
File /r "bin\Release\net6.0\win10-x64\publish\*.*"

; Copy the icon file to the installation directory
File "jammer_1024px.ico"

; Create shortcut to sendto folder in AppData\Roaming\Microsoft\Windows\SendTo
CreateShortCut "$SENDTO\${APP_NAME}.lnk" "$PROGRAMFILES\${APP_NAME}\${EXE_NAME}" "" "$PROGRAMFILES\${APP_NAME}\jammer_1024px.ico" 0

; CREATE SHORTCUT TO DESKTOP
CreateShortCut "$DESKTOP\${APP_NAME}.lnk" "$PROGRAMFILES\${APP_NAME}\${EXE_NAME}" "" "$PROGRAMFILES\${APP_NAME}\jammer_1024px.ico" 0

SectionEnd

Section "Uninstall"

    ; Remove files
    Delete "$PROGRAMFILES\${APP_NAME}\${EXE_NAME}"
    Delete "$PROGRAMFILES\${APP_NAME}\jammer_1024px.ico"

    ; Remove directory
    RMDir "$PROGRAMFILES\${APP_NAME}"

    ; Remove shortcut
    Delete "$SENDTO\${APP_NAME}.lnk"

SectionEnd
