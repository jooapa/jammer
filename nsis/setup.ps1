# setup.ps1

# Check if the script is running with administrative privileges
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    # Relaunch the script with administrative privileges
    Start-Process powershell -Verb RunAs -ArgumentList ("-NoProfile -ExecutionPolicy Bypass -File `"$($MyInvocation.MyCommand.Path)`"")
    exit
}

# Define the path to the Jammer folder
$JammerFolderPath = "$PSScriptRoot"

# Check if Jammer folder path is already in the system's PATH environment variable
$currentPath = [System.Environment]::GetEnvironmentVariable("PATH", [System.EnvironmentVariableTarget]::Machine)
if ($currentPath -split ";" -notcontains $JammerFolderPath) {
    # Add the Jammer folder to the system's PATH environment variable
    $newPath = $currentPath + ";" + $JammerFolderPath
    [System.Environment]::SetEnvironmentVariable("PATH", $newPath, [System.EnvironmentVariableTarget]::Machine)
    Write-Host "Jammer folder added to the system PATH."
} else {
    Write-Host "Jammer folder is already present in the system PATH."
}

# # Add a "Jammer" as context menu item that will run Jammer.exe with the selected file(s) as argument. and it will not open multiple cmd windows
# $JammerCommand = "cmd /c start /b Jammer.exe `"%1`""
# $JammerCommandKey = "Software\Classes\*\shell\Jammer\command"
# $JammerCommandValue = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($JammerCommandKey, $true)
# if (-not $JammerCommandValue) {
#     $JammerCommandValue = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey($JammerCommandKey)
#     $JammerCommandValue.SetValue("", $JammerCommand)
#     Write-Host "Jammer context menu item added."
# } else {
#     Write-Host "Jammer context menu item is already present."
# }
