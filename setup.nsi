!define APP_NAME "jammer"
!define EXE_NAME "jammer.exe"


; Request administrative privileges
RequestExecutionLevel admin

; Set the output folder for the installer
Outfile "jammer-Setup.exe"

; default installation directory
InstallDir "$PROGRAMFILES\${APP_NAME}"

; Default section
Page Directory
Page InstFiles



Section

; Set output path to the selected installation directory
SetOutPath "$INSTDIR"

; Create the selected installation directory
CreateDirectory "$INSTDIR"

; Copy files to the installation directory
File /r "nsis-folder\*.*"

; Write uninstaller
WriteUninstaller "$INSTDIR\Uninstall.exe"

; Create shortcut to sendto folder in AppData\Roaming\Microsoft\Windows\SendTo
CreateShortCut "$SENDTO\${APP_NAME}.lnk" "$INSTDIR\${EXE_NAME}" "" "$INSTDIR\jammer_1024px.ico" 0

; shortcut to desktop
CreateShortCut "$DESKTOP\${APP_NAME}.lnk" "$INSTDIR\${EXE_NAME}" "" "$INSTDIR\jammer_1024px.ico" 0

; run setup.ps1 
ExecWait '"powershell.exe" -ExecutionPolicy Bypass -File "$INSTDIR\setup.ps1"'

SectionEnd

Section "Uninstall"

    ; run Uninstall.ps1
    ExecWait '"powershell.exe" -ExecutionPolicy Bypass -File "$INSTDIR\uninstall.ps1"'

    ; Remove files
    Delete "$INSTDIR\${EXE_NAME}"
    Delete "$INSTDIR\jammer_1024px.ico"
    Delete "$INSTDIR\jammer.ico"
    Delete "$INSTDIR\Uninstall.exe"
    Delete "$INSTDIR\selfdestruct.bat"
    Delete "$INSTDIR\setup.ps1"
    Delete "$INSTDIR\uninstall.ps1"
    Delete "$INSTDIR\raylib.dll"

    ; Remove shortcut
    Delete "$SENDTO\${APP_NAME}.lnk"
    Delete "$DESKTOP\${APP_NAME}.lnk"
    
    ; Remove directories
    RMDir "$INSTDIR"

SectionEnd
