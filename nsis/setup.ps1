# setup.ps1

# Check if the script is running with administrative privileges
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    # Relaunch the script with administrative privileges
    Start-Process powershell -Verb RunAs -ArgumentList ("-NoProfile -ExecutionPolicy Bypass -File `"$($MyInvocation.MyCommand.Path)`"")
    exit
}

# Define the path to the Jammer folder
$jammerFolderPath = "$PSScriptRoot"

# Check if Jammer folder path is already in the system's PATH environment variable
$currentPath = [System.Environment]::GetEnvironmentVariable("PATH", [System.EnvironmentVariableTarget]::Machine)
if ($currentPath -split ";" -notcontains $jammerFolderPath) {
    # Add the Jammer folder to the system's PATH environment variable
    $newPath = $currentPath + ";" + $jammerFolderPath
    [System.Environment]::SetEnvironmentVariable("PATH", $newPath, [System.EnvironmentVariableTarget]::Machine)
    Write-Host "Jammer folder added to the system PATH."
} else {
    Write-Host "Jammer folder is already present in the system PATH."
}

# # Add a "jammer" as context menu item that will run jammer.exe with the selected file(s) as argument. and it will not open multiple cmd windows
# $jammerCommand = "cmd /c start /b jammer.exe `"%1`""
# $jammerCommandKey = "Software\Classes\*\shell\jammer\command"
# $jammerCommandValue = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($jammerCommandKey, $true)
# if (-not $jammerCommandValue) {
#     $jammerCommandValue = [Microsoft.Win32.Registry]::CurrentUser.CreateSubKey($jammerCommandKey)
#     $jammerCommandValue.SetValue("", $jammerCommand)
#     Write-Host "Jammer context menu item added."
# } else {
#     Write-Host "Jammer context menu item is already present."
# }
