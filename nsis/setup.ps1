# Check if the script is running with administrator privileges
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")
if (-not $isAdmin) {
    Write-Host "You need to run this script as an administrator."
    Write-Host "Please right-click on the script and select 'Run as administrator.'"
    pause
    exit 1
}
# Argument got from .bat
# Contains the directory it was called from
$callDir = $args[0]
# $mode = $args[1]



# Windows PATH Environment Variable Setup
#
# ---------------------------------------------------------------------
# Add the current directory to the PATH environment variable
# Check if folder $callDir is already in the path
$pathDir = "$callDir"

Write-Host Adding current path to System Environmental Variables.
if($Env:PATH -notlike "*$pathDir*"){
    # TRUE, ADD TO PATH


    # Make oldPath and newPath variables
    $oldpath = (Get-ItemProperty -Path 'Registry::HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Session Manager\Environment' -Name PATH).path
    $newpath = "$oldpath;$pathDir"
    
    Set-ItemProperty -Path 'Registry::HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Session Manager\Environment' -Name PATH -Value $newpath

    Write-Host Added current path to System Environmental Variables.
} else {
	# FALSE DON'T ADD
    Write-Host Current path is already found in System Environmental Variables.
}

# Windows PATHEXT Environmental Variable Setup
#
# ---------------------------------------------------------------------
# Setting the PATHEXT environment variable
$oldpathext = (Get-ItemProperty -Path 'Registry::HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Session Manager\Environment' -Name PATHEXT).pathext

$newpathext = "$oldpathext;.jammer"

# Check if .jammer is already in the pathext
Write-Host Adding .jammer to PATHEXT.
if($Env:PATHEXT -notlike "*.jammer*"){
    # TRUE, ADD TO PATHEXT
    Set-ItemProperty -Path 'Registry::HKEY_LOCAL_MACHINE\System\CurrentControlSet\Control\Session Manager\Environment' -Name PATHEXT -Value $newpathext
    Write-Host Added .jammer to PATHEXT.
} else {
    # FALSE, DON'T ADD
    Write-Host .jammer already found in PATHEXT.
}
# ---------------------------------------------------------------------

# Windows HKCR Setup
#
# Define file extensions
# ! EXECUTABLE ICON
$iconPathExe = "$PSScriptRoot\jammer.ico"

# ! .JAMMER ICON
$iconPath = "$PSScriptRoot\jammer.ico"

# $cmdScriptPath = "$PSScriptRoot\run_command.bat"
$cmdScriptPath = "$PSScriptRoot\open_with_jammer.cmd"
$registryKeyPath = "HKCR\.jammer"
$registryKeyValuePerceivedType = "PerceivedType"
$registryKeyValuePerceivedTypeValue = "text"
$registryShellKeyPath = "HKCR\.jammer\shell\Jam with jammer"
$registryCommandKeyPath = "HKCR\.jammer\shell\Jam with jammer\command"
# Create registry entries
reg.exe add "$registryKeyPath" /v "$registryKeyValuePerceivedType" /d "$registryKeyValuePerceivedTypeValue" /f
reg.exe add "$registryShellKeyPath" /ve /d "Jam with jammer" /f
reg.exe add "$registryCommandKeyPath" /ve /d "$cmdScriptPath `"%1`"" /f
# Context menu icon
reg.exe add "$registryShellKeyPath" /v "Icon" /d "$iconPathExe" /f
# Add position
# reg.exe add "$registryKeyPath" /v "Position" /d "Top" /f
Write-Host "Context menu entry added for .jammer files."
# Add the DefaultIcon registry key with the path to your custom icon
reg.exe add "$registryKeyPath\DefaultIcon" /ve /d "$iconPath" /f
Write-Host "Custom icon added for .jammer files."

# foreach ($extension in $fileExtensions) {
    $registryKeyPath = "HKCR\SystemFileAssociations\audio"
    $registryShellKeyPath = "$registryKeyPath\shell\Jam with jammer"
    $registryCommandKeyPath = "$registryShellKeyPath\command"
    #HKEY_CLASSES_ROOT\SystemFileAssociations\audio\Shell\
    # Create registry entries
    reg.exe add "$registryKeyPath" /v "PerceivedType" /d "text" /f
    reg.exe add "$registryShellKeyPath" /ve /d "Jam with jammer" /f
    reg.exe add "$registryCommandKeyPath" /ve /d "$cmdScriptPath `"%1`"" /f
    reg.exe add "$registryShellKeyPath" /v "Icon" /d "$iconPathExe" /f
    reg.exe add "$registryShellKeyPath\DefaultIcon" /ve /d "$iconPath" /f

    Write-Host "Context menu entry added for audio files."
# }

# Add global reged encrypt dir with atk

$cryptCmd2 = "$PSScriptRoot\open_with_jammer.cmd"
reg.exe add "HKCR\Directory\shell\Jam with jammer" /ve /d "Jam with jammer" /f
reg.exe add "HKCR\Directory\shell\Jam with jammer\command" /ve /d "$cryptCmd2 `"%1`"" /f
Write-Host "Context menu entry added for all files."

# Context menu icon
reg.exe add "HKCR\Directory\shell\Jam with jammer" /v "Icon" /d "$iconPathExe" /f
Write-Host "Context menu icon added for all directories."
#####################################################
$registryKeyPath = "HKCU\SOFTWARE\Classes\Directory\Background"
$registryShellKeyPath = "$registryKeyPath\shell\Jam with jammer"
$registryCommandKeyPath = "$registryShellKeyPath\command"

reg.exe add "$registryKeyPath" /v "PerceivedType" /d "text" /f
reg.exe add "$registryShellKeyPath" /ve /d "Jam with jammer" /f
reg.exe add "$registryCommandKeyPath" /ve /d "$cmdScriptPath `"%1`"" /f
reg.exe add "$registryShellKeyPath" /v "Icon" /d "$iconPathExe" /f
reg.exe add "$registryShellKeyPath\DefaultIcon" /ve /d "$iconPath" /f
# Context menu icon
Write-Host "Context menu icon added for all directories."



$Env:Path = [System.Environment]::GetEnvironmentVariable("Path","Machine")
Write-Host "Setup succesfull."