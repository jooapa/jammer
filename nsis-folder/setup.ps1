# setup.ps1

# Check if the script is running with administrative privileges
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    # Relaunch the script with administrative privileges
    Start-Process powershell -Verb RunAs -ArgumentList ("-NoProfile -ExecutionPolicy Bypass -File `"$($MyInvocation.MyCommand.Path)`"")
    exit
}

# Define the path to the Jammer folder
$jammerFolderPath = "C:\Program Files (x86)\jammer"

# Add the Jammer folder to the system's PATH environment variable
$currentPath = [System.Environment]::GetEnvironmentVariable("PATH", [System.EnvironmentVariableTarget]::Machine)
$newPath = $currentPath + ";" + $jammerFolderPath
[System.Environment]::SetEnvironmentVariable("PATH", $newPath, [System.EnvironmentVariableTarget]::Machine)

Write-Host "Jammer folder added to the system PATH."