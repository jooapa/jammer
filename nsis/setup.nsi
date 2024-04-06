unicode True

!include LogicLib.nsh
!define WM_WININICHANGE 0x001A
!define HWND_BROADCAST 0xFFFF
!define WM_SETTINGCHANGE 0x001A

!define VERSION "2.0.5"

Outfile "jammer-Setup_V${VERSION}.exe" ; Use the version number here
BrandingText /TRIMCENTER "Jammer Setup V${VERSION}"
Name "Jammer Setup V${VERSION}"
RequestExecutionLevel admin

ManifestSupportedOS Win10
DetailsButtonText "Show progress"

######### LICENSE ############
PageEx license
    LicenseText "LICENSE"
    LicenseData LICENSE
    LicenseForceSelection checkbox
PageExEnd

############ DIRECTORY ######################
Var INSTALL_DIR

PageEx directory
    DirVar $INSTALL_DIR
    DirVerify leave
PageExEnd

Function .onVerifyInstDir
    Var /GLOBAL ext
    StrCpy $ext "jammer"
    StrCpy $INSTALL_DIR "$INSTALL_DIR$ext"
    ; Checks if folder already exists
    Call CheckFolder
FunctionEnd

; Checks if the folder exists, if it exists and user wants to delete
; it and it's contents the script will continue
Function CheckFolder
DetailPrint "Checking folder."
${If}  ${FileExists} $INSTALL_DIR
    ; Delete it
    MessageBox MB_YESNO|MB_ICONQUESTION `"$INSTALL_DIR" already exists, delete its contents and continue installing?` IDYES agree
    Abort "Setup aborted by user."
agree:
    DetailPrint 'Removing "$INSTALL_DIR" and its contents.'
    RMDir /r $INSTALL_DIR
${EndIf}
FunctionEnd

##########INSTFILE"######################
PageEx instfiles
PageExEnd

############## INIT ######################
; Set the default installation directory
Function .onInit
    InitPluginsDir
    StrCpy $INSTALL_DIR $PROGRAMFILES\jammer
FunctionEnd

############################## START ##############################
Section 

; After deletion begin with setup

; Check the folder
Call CheckFolder

; Set the installation directory
SetOutPath $INSTALL_DIR

; Create the installation directory if it doesn't exist
CreateDirectory $INSTALL_DIR

; Files to install
File "jammer.exe"
File "jammer_1024px.ico"
File "jammer.ico"
File "setup.ps1"
File "uninstall.ps1"
File "run_command.bat"
File "open_with_jammer.cmd"
File "LICENSE"
File "bass.dll"
File "bass_aac.dll"

CreateDirectory $PROFILE\jammer\locales
SetOutPath $PROFILE\jammer\locales
File /r "locales\*.*"
SetOutPath $INSTALL_DIR

SectionEnd

############# SETUP ################
Section 

runSetup:

File "setup.ps1"

; Execute setup.ps1 script
nsExec::ExecToLog 'Powershell.exe -ExecutionPolicy Bypass -File "$INSTALL_DIR\setup.ps1" "$INSTALL_DIR" "no"'

; Check if setup ran succesfully
Pop $0
${If} $0 == "0"
    DetailPrint "Setup sequence completed with success."
${Else}
    DetailPrint "Failed to run setup."
    MessageBox MB_ABORTRETRYIGNORE|MB_ICONEXCLAMATION "Error running setup. Retry by pressing 'Retry', \
    ignore the error and continue by pressing 'Ignore', or close the program by pressing 'Abort'." IDABORT abortM IDIGNORE ignoreM
        ; Run setup again
        DetailPrint "Running setup again."
        ; After executing, delete it
        Delete "setup.ps1"
        Goto runSetup
    ignoreM:
        ; Continue setup
        DetailPrint "Continuing setup."
        ; After executing, delete it
        Delete "setup.ps1"
        Goto Continue
    abortM:
        ; Abort
        ; After executing, delete it
        Delete "setup.ps1"
        Abort "Setup aborted by user."
${EndIf}
Continue:
; After executing, delete it
Delete "setup.ps1"

########## EXTRACTION ##########
; Extract files based on section selection

; Extract uninstallation script
File "uninstall.ps1"

# Create shortcut on DESKTOP
CreateShortcut "$DESKTOP\jammer.lnk" "$INSTALL_DIR\jammer.exe" "" "$INSTALL_DIR\jammer_1024px.ico"
CreateShortCut "$SENDTO\jammer.lnk" "$INSTALL_DIR\jammer.exe" "" "$INSTALL_DIR\jammer_1024px.ico" 0

; Create an uninstaller in the same directory as the installer
WriteUninstaller "$INSTALL_DIR\Uninstall.exe"
DetailPrint 'You can now close this windows by pressing "Close".' 

SectionEnd

########## UNINSTALL ##########

UninstPage uninstConfirm
UninstPage instfiles

Section "Uninstall"
; Execute the PowerShell script with elevated privileges and pass the parameters
nsExec::ExecToLog 'Powershell.exe -ExecutionPolicy Bypass -File "$INSTDIR\uninstall.ps1" "$INSTDIR"'

########################### DELETE FILES ###########################
; Remove installed files during uninstallation

Delete "$INSTDIR\jammer.exe"
Delete "$INSTDIR\jammer.ico"
Delete "$INSTDIR\jammer_1024px.ico"
Delete "$INSTDIR\LICENSE"
Delete "$INSTDIR\uninstall.exe"
Delete "$INSTDIR\setup.ps1"
Delete "$INSTDIR\run_command.bat"
Delete "$INSTDIR\open_with_jammer.cmd"
Delete "$DESKTOP\jammer.lnk"
Delete "$SENDTO\jammer.lnk"
Delete "$INSTDIR\bass.dll"
Delete "$INSTDIR\bass_aac.dll"


; Remove the installation directory if it still exists
RMDir /r $INSTDIR

SectionEnd