#! /usr/bin/env pwsh

[CmdletBinding()]
param(
    [ValidateSet('linux-x64', 'win-x64', 'osx-x64', 'osx-arm64', 'all')]
    [string]$Target = 'all',
    [ValidateSet('Release', 'Debug')]
    [string]$Configuration = 'Release',
    [switch]$Clean,
    [string]$OutputDirectory = 'artifacts',
    [string]$Version,
    [switch]$SkipPackage
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if ($PSVersionTable.PSVersion.Major -lt 7) {
    throw 'scripts/build.ps1 requires PowerShell 7 or newer. Run it with pwsh.'
}

$RepositoryRoot = Split-Path -Parent $PSScriptRoot
$Project = Join-Path $RepositoryRoot 'Jammer.CLI/Jammer.CLI.csproj'
$VersionFile = Join-Path $RepositoryRoot 'VERSION'
if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = (Get-Content -Raw $VersionFile).Trim()
}
if ($Version -notmatch '^\d+\.\d+(?:\.\d+)?(?:-[0-9A-Za-z.-]+)?$') {
    throw "Invalid version '$Version'. Expected a semantic version such as 1.2 or 1.2.3."
}

function Require-Command([string]$Name, [string]$Guidance) {
    $command = Get-Command $Name -ErrorAction SilentlyContinue
    if ($null -eq $command) { throw "Required tool '$Name' was not found. $Guidance" }
    return $command.Source
}

function Invoke-External([string]$Command, [string[]]$Arguments) {
    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "Command failed with exit code $LASTEXITCODE`: $Command $($Arguments -join ' ')"
    }
}

$Dotnet = Require-Command 'dotnet' 'Install the .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0'
$OutputRoot = if ([IO.Path]::IsPathRooted($OutputDirectory)) {
    [IO.Path]::GetFullPath($OutputDirectory)
} else {
    [IO.Path]::GetFullPath((Join-Path $RepositoryRoot $OutputDirectory))
}
if ($Clean -and (Test-Path $OutputRoot)) {
    if ($OutputRoot -eq [IO.Path]::GetPathRoot($OutputRoot) -or $OutputRoot -eq $RepositoryRoot) {
        throw "Refusing to clean unsafe output directory: $OutputRoot"
    }
    Remove-Item -Recurse -Force $OutputRoot
}
New-Item -ItemType Directory -Force $OutputRoot | Out-Null

$Targets = if ($Target -eq 'all') { @('linux-x64', 'win-x64', 'osx-x64', 'osx-arm64') } else { @($Target) }
$HostPlatform = if ($IsWindows) { 'windows' } elseif ($IsLinux) { 'linux' } elseif ($IsMacOS) { 'macos' } else { 'unknown' }

function Publish-Target([string]$Rid) {
    $publishDirectory = Join-Path $OutputRoot "publish/$Rid"
    New-Item -ItemType Directory -Force $publishDirectory | Out-Null
    $arguments = @(
        'publish', $Project,
        '--runtime', $Rid,
        '--configuration', $Configuration,
        '--self-contained', 'true',
        '--output', $publishDirectory,
        '-p:PublishSingleFile=true',
        '-p:IncludeNativeLibrariesForSelfExtract=false',
        "-p:Version=$Version",
        "-p:InformationalVersion=$Version"
    )
    Invoke-External $Dotnet $arguments
    return $publishDirectory
}

function Copy-Required([string]$Source, [string]$Destination) {
    if (-not (Test-Path $Source)) { throw "Required packaging file is missing: $Source" }
    $parent = Split-Path -Parent $Destination
    if ($parent) { New-Item -ItemType Directory -Force $parent | Out-Null }
    Copy-Item -Force $Source $Destination
}

function Package-Linux([string]$PublishDirectory) {
    if ($HostPlatform -ne 'linux') { throw 'AppImage packaging must run on a Linux x64 host. Use -SkipPackage elsewhere to validate publishing.' }
    $AppImageTool = Require-Command 'appimagetool' 'Install the official appimagetool and place it on PATH.'
    $appDir = Join-Path $OutputRoot 'staging/jammer.AppDir'
    if (Test-Path $appDir) { Remove-Item -Recurse -Force $appDir }
    New-Item -ItemType Directory -Force (Join-Path $appDir 'usr/bin'), (Join-Path $appDir 'usr/lib'), (Join-Path $appDir 'usr/locales') | Out-Null
    Copy-Required (Join-Path $RepositoryRoot 'jammer.AppDir/AppRun') (Join-Path $appDir 'AppRun')
    Copy-Required (Join-Path $RepositoryRoot 'jammer.AppDir/jammer.desktop') (Join-Path $appDir 'jammer.desktop')
    Copy-Required (Join-Path $RepositoryRoot 'jammer.AppDir/Jammer-icon.png') (Join-Path $appDir 'Jammer-icon.png')
    Copy-Required (Join-Path $PublishDirectory 'Jammer.CLI') (Join-Path $appDir 'usr/bin/Jammer')
    Get-ChildItem (Join-Path $RepositoryRoot 'libs/linux/x86_64/libbass*.so') | Copy-Item -Destination (Join-Path $appDir 'usr/lib')
    Copy-Required (Join-Path $PublishDirectory 'libuiohook.so') (Join-Path $appDir 'usr/lib/libuiohook.so')
    Copy-Item (Join-Path $RepositoryRoot 'locales/*') -Destination (Join-Path $appDir 'usr/locales')
    Invoke-External (Require-Command 'chmod' 'Install coreutils.') @('+x', (Join-Path $appDir 'AppRun'), (Join-Path $appDir 'usr/bin/Jammer'))
    $artifact = Join-Path $OutputRoot "jammer-$Version-x86_64.AppImage"
    $env:ARCH = 'x86_64'
    try { Invoke-External $AppImageTool @($appDir, $artifact) } finally { Remove-Item Env:ARCH -ErrorAction SilentlyContinue }
}

function Package-Windows([string]$PublishDirectory) {
    if ($HostPlatform -ne 'windows') { throw 'NSIS installer packaging must run on Windows. Use -SkipPackage elsewhere to validate publishing.' }
    $MakeNsis = Require-Command 'makensis' 'Install NSIS and add makensis.exe to PATH.'
    $stage = Join-Path $OutputRoot 'staging/nsis'
    if (Test-Path $stage) { Remove-Item -Recurse -Force $stage }
    New-Item -ItemType Directory -Force (Join-Path $stage 'locales'), (Join-Path $stage 'docs') | Out-Null
    Copy-Required (Join-Path $PublishDirectory 'Jammer.CLI.exe') (Join-Path $stage 'Jammer.exe')
    Copy-Required (Join-Path $PublishDirectory 'uiohook.dll') (Join-Path $stage 'uiohook.dll')
    Copy-Required (Join-Path $RepositoryRoot 'icons/trans_icon512x512.ico') (Join-Path $stage 'Jammer.ico')
    foreach ($name in @('bass.dll', 'bass_aac.dll', 'bassmidi.dll', 'bassopus.dll', 'ffmpeg.exe')) {
        Copy-Required (Join-Path $RepositoryRoot "libs/win/x64/$name") (Join-Path $stage $name)
    }
    foreach ($name in @('setup.nsi', 'setup.ps1', 'uninstall.ps1', 'run_command.bat', 'open_with_jammer.cmd')) {
        Copy-Required (Join-Path $RepositoryRoot "nsis/$name") (Join-Path $stage $name)
    }
    Copy-Required (Join-Path $RepositoryRoot 'LICENSE') (Join-Path $stage 'LICENSE')
    Copy-Item (Join-Path $RepositoryRoot 'locales/*') -Destination (Join-Path $stage 'locales')
    Copy-Item (Join-Path $RepositoryRoot 'docs/*') -Destination (Join-Path $stage 'docs')
    Push-Location $stage
    try { Invoke-External $MakeNsis @("/DVERSION=$Version", 'setup.nsi') } finally { Pop-Location }
    Copy-Required (Join-Path $stage "Jammer-Setup_V$Version.exe") (Join-Path $OutputRoot "Jammer-Setup_V$Version.exe")
}

function Package-MacOS([string]$Rid, [string]$PublishDirectory) {
    if ($HostPlatform -ne 'macos') { throw 'macOS distribution packaging must run on macOS. Use -SkipPackage elsewhere to validate publishing.' }
    $nativeRoot = Join-Path $RepositoryRoot 'libs/macos/universal'
    $required = @('libbass.dylib', 'libbassmidi.dylib', 'libbassopus.dylib')
    $missing = @($required | Where-Object { -not (Test-Path (Join-Path $nativeRoot $_)) })
    if ($missing.Count -gt 0) {
        throw "Cannot package ${Rid}: missing native files in ${nativeRoot}: $($missing -join ', '). macOS native files must be universal binaries; no runnable archive was created."
    }
    $Lipo = Require-Command 'lipo' 'Xcode command-line tools are required to validate macOS native architectures.'
    foreach ($name in $required) {
        Invoke-External $Lipo @('-verify_arch', 'x86_64', 'arm64', (Join-Path $nativeRoot $name))
    }
    $bundleName = "jammer-$Version-$Rid"
    $stage = Join-Path $OutputRoot "staging/$bundleName"
    if (Test-Path $stage) { Remove-Item -Recurse -Force $stage }
    New-Item -ItemType Directory -Force (Join-Path $stage 'locales') | Out-Null
    Copy-Required (Join-Path $PublishDirectory 'Jammer.CLI') (Join-Path $stage 'Jammer.bin')
    foreach ($name in $required) { Copy-Required (Join-Path $nativeRoot $name) (Join-Path $stage $name) }
    Copy-Item (Join-Path $RepositoryRoot 'locales/*') -Destination (Join-Path $stage 'locales')
    Copy-Required (Join-Path $RepositoryRoot 'LICENSE') (Join-Path $stage 'LICENSE')
    $launcher = @'
#!/bin/sh
SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"
export DYLD_LIBRARY_PATH="$SCRIPT_DIR${DYLD_LIBRARY_PATH:+:$DYLD_LIBRARY_PATH}"
exec "$SCRIPT_DIR/Jammer.bin" "$@"
'@
    Set-Content -NoNewline -Encoding utf8 (Join-Path $stage 'Jammer') $launcher
    Invoke-External (Require-Command 'chmod' 'chmod is required on macOS.') @('+x', (Join-Path $stage 'Jammer'), (Join-Path $stage 'Jammer.bin'))
    $Tar = Require-Command 'tar' 'The system tar command is required to create the macOS archive.'
    $artifact = Join-Path $OutputRoot "$bundleName.tar.gz"
    Invoke-External $Tar @('-czf', $artifact, '-C', (Split-Path -Parent $stage), (Split-Path -Leaf $stage))
}

foreach ($rid in $Targets) {
    Write-Host "Publishing Jammer $Version for $rid..."
    $published = Publish-Target $rid
    if ($SkipPackage) { continue }
    switch ($rid) {
        'linux-x64' { Package-Linux $published }
        'win-x64' { Package-Windows $published }
        'osx-x64' { Package-MacOS $rid $published }
        'osx-arm64' { Package-MacOS $rid $published }
    }
}

Write-Host "Artifacts are in $OutputRoot"
