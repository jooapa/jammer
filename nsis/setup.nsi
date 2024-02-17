unicode True

!include LogicLib.nsh
!define WM_WININICHANGE 0x001A
!define HWND_BROADCAST 0xFFFF
!define WM_SETTINGCHANGE 0x001A

!define VERSION "1.2.1"

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

########### COMPONENTS #####################

InstType "Full" IT_FULL
InstType "Minimal" IT_MIN

PageEx components
    ComponentText "Choose files you want to install." \
    #"About" \
    #"Main program contains all necessary components for basic functioning. \
    #Additional components contain all extra files, mainly used by the context menu."
PageExEnd

SectionGroup "!Main Program" RO
    Section !jammer.exe sec1_id
        DetailPrint "These files go to your TEMP folder. These will be deleted after setup."
        SectionInstType ${IT_FULL} ${IT_MIN} RO
        SectionIn 1 ${sec1_id}
        SetOutPath "$TEMP\jammer"
        File "jammer.exe"
    SectionEnd
    Section !jammer_1024px.ico sec2_id
        SectionInstType ${IT_FULL} ${IT_MIN} RO
        SectionIn 1 ${sec1_id}
        SetOutPath "$TEMP\jammer"
        File "jammer_1024px.ico"
    SectionEnd
    Section !jammer.ico sec3_id
        SectionInstType ${IT_FULL} ${IT_MIN} RO
        SectionIn 1 ${sec1_id}
        SetOutPath "$TEMP\jammer"
        File "jammer.ico"
    SectionEnd
    Section !setup.ps1 sec4_id
        SectionInstType ${IT_FULL} ${IT_MIN} RO
        SectionIn 1 ${sec1_id}
        SetOutPath "$TEMP\jammer"
        File "setup.ps1"
    SectionEnd
    Section !uninstall.ps1 sec5_id
        SectionInstType ${IT_FULL} ${IT_MIN} RO
        SectionIn 1 ${sec1_id}
        SetOutPath "$TEMP\jammer"
        File "uninstall.ps1"
    SectionEnd
    Section !run_command.bat sec6_id
        SectionInstType ${IT_FULL} ${IT_MIN} RO
        SectionIn 1 ${sec1_id}
        SetOutPath "$TEMP\jammer"
        File "run_command.bat"
    SectionEnd
    Section !open_with_jammer.cmd sec7_id
        SectionInstType ${IT_FULL} ${IT_MIN} RO
        SectionIn 1 ${sec1_id}
        SetOutPath "$TEMP\jammer"
        File "open_with_jammer.cmd"
    SectionEnd
SectionGroupEnd


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
    ; run uninstaller
    nsExec::ExecToLog 'Powershell.exe -ExecutionPolicy Bypass -File "$INSTALL_DIR\Uninstall.exe" "$INSTALL_DIR"'
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
; Delete the $TEMP folder stuff before extracting more files
; and taking up more space from the disk
Delete "$TEMP\jammer\jammer.exe"
Delete "$TEMP\jammer\jammer.ico"
Delete "$TEMP\jammer\jammer_1024px.ico"
Delete "$TEMP\jammer\LICENSE"
Delete "$TEMP\jammer\uninstall.exe"
Delete "$TEMP\jammer\setup.ps1"
Delete "$TEMP\jammer\run_command.bat"
Delete "$TEMP\jammer\open_with_jammer.cmd"

; The folder will be deleted in the Uninstall section
RMDir /r $TEMP\jammer

DetailPrint 'Files from "$TEMP\jammer" deleted. beginning setup.' 

; After deletion begin with setup

; Check the folder
Call CheckFolder

; Set the installation directory
SetOutPath $INSTALL_DIR

; Create the installation directory if it doesn't exist
CreateDirectory $INSTALL_DIR

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

########## MAIN PROGRAM ##########
${If} ${SectionIsSelected} ${sec1_id}
    File "jammer.exe"
${EndIf}
${If} ${SectionIsSelected} ${sec2_id}
    File "jammer_1024px.ico"
${EndIf}
${If} ${SectionIsSelected} ${sec3_id}
    File "jammer.ico"
${EndIf}
${If} ${SectionIsSelected} ${sec4_id}
    File "setup.ps1"
${EndIf}
${If} ${SectionIsSelected} ${sec5_id}
    File "uninstall.ps1"
${EndIf}
${If} ${SectionIsSelected} ${sec6_id}
    File "run_command.bat"
${EndIf}
${If} ${SectionIsSelected} ${sec7_id}
    File "open_with_jammer.cmd"
${EndIf}



# Create shortcut on DESKTOP
CreateShortcut "$DESKTOP\jammer.lnk" "$INSTALL_DIR\jammer.exe" "" "$INSTALL_DIR\jammer_1024px.ico"
CreateShortCut "$SENDTO\jammer.lnk" "$INSTDIR\jammer.exe" "" "$INSTDIR\jammer_1024px.ico" 0

; Create an uninstaller in the same directory as the installer
WriteUninstaller "$INSTALL_DIR\Uninstall.exe"
DetailPrint 'You can now close this windows by pressing "Close".' 

SectionEnd

########## UNINSTALL ##########


UninstPage uninstConfirm
UninstPage instfiles

# Call must be used with function names starting with "un." in the uninstall section.
;Function unRefresh
;    ; Refreshes icons
;    System::Call 'shell32.dll::SHChangeNotify(i, i, i, i) i(0x8000000, 0, 0, 0)'
;
;    ; Refreshes Environmental variables
;    System::Call 'user32::SendMessage(i ${HWND_BROADCAST}, i ${WM_WININICHANGE}, i 0, t "Environment")'
;FunctionEnd

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

; Remove the installation directory and TEMP directory if it still exists
RMDir /r $INSTDIR
RMDir /r $TEMP\jammer

; Lastly refresh icons and env
;Call unRefresh

SectionEnd