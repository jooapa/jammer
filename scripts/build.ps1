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
    & $Command @Arguments | Out-Host
    $exitCode = $LASTEXITCODE
    if ($exitCode -ne 0) {
        throw "Command failed with exit code $exitCode`: $Command $($Arguments -join ' ')"
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

function Get-NativePublishFile([string]$PublishDirectory, [string]$Rid, [string]$Name) {
    $publishedPath = Join-Path $PublishDirectory $Name
    if (Test-Path $publishedPath) { return $publishedPath }

    # Some NuGet build targets copy native assets beside the intermediate apphost but
    # do not include them in a single-file publish directory.
    return Join-Path $RepositoryRoot "Jammer.CLI/bin/$Configuration/net8.0/$Rid/$Name"
}

function Get-AppImageTool {
    $installed = Get-Command 'appimagetool' -ErrorAction SilentlyContinue
    if ($null -ne $installed) { return $installed.Source }

    $toolDirectory = Join-Path $OutputRoot 'tools'
    $toolPath = Join-Path $toolDirectory 'appimagetool-x86_64.AppImage'
    if (Test-Path $toolPath) { return $toolPath }

    $downloadUrl = 'https://github.com/AppImage/appimagetool/releases/download/continuous/appimagetool-x86_64.AppImage'
    $temporaryPath = "$toolPath.download"
    New-Item -ItemType Directory -Force $toolDirectory | Out-Null
    Remove-Item -Force $temporaryPath -ErrorAction SilentlyContinue
    Write-Host "appimagetool was not found on PATH; downloading the official x86_64 build..."
    try {
        Invoke-WebRequest -Uri $downloadUrl -OutFile $temporaryPath

        # AppImages are ELF files with an additional AI type-2 magic value at byte 8.
        $header = [byte[]]::new(11)
        $stream = [IO.File]::OpenRead($temporaryPath)
        try { $bytesRead = $stream.Read($header, 0, $header.Length) } finally { $stream.Dispose() }
        if ($bytesRead -ne $header.Length -or
            $header[0] -ne 0x7f -or $header[1] -ne 0x45 -or $header[2] -ne 0x4c -or $header[3] -ne 0x46 -or
            $header[8] -ne 0x41 -or $header[9] -ne 0x49 -or $header[10] -ne 0x02) {
            throw 'The downloaded file is not a type-2 AppImage.'
        }

        Move-Item -Force $temporaryPath $toolPath
        Invoke-External (Require-Command 'chmod' 'Install coreutils.') @('+x', $toolPath)
    } catch {
        Remove-Item -Force $temporaryPath -ErrorAction SilentlyContinue
        throw "Could not download appimagetool from $downloadUrl. Check the network connection or install appimagetool on PATH. $($_.Exception.Message)"
    }
    return $toolPath
}

function Package-Linux([string]$PublishDirectory) {
    if ($HostPlatform -ne 'linux') { throw 'AppImage packaging must run on a Linux x64 host. Use -SkipPackage elsewhere to validate publishing.' }
    $AppImageTool = Get-AppImageTool
    $appDir = Join-Path $OutputRoot 'staging/jammer.AppDir'
    if (Test-Path $appDir) { Remove-Item -Recurse -Force $appDir }
    New-Item -ItemType Directory -Force (Join-Path $appDir 'usr/bin'), (Join-Path $appDir 'usr/lib'), (Join-Path $appDir 'usr/locales') | Out-Null
    Copy-Required (Join-Path $RepositoryRoot 'jammer.AppDir/AppRun') (Join-Path $appDir 'AppRun')
    Copy-Required (Join-Path $RepositoryRoot 'jammer.AppDir/jammer.desktop') (Join-Path $appDir 'jammer.desktop')
    Copy-Required (Join-Path $RepositoryRoot 'jammer.AppDir/Jammer-icon.png') (Join-Path $appDir 'Jammer-icon.png')
    Copy-Required (Join-Path $PublishDirectory 'Jammer.CLI') (Join-Path $appDir 'usr/bin/Jammer')
    Get-ChildItem (Join-Path $RepositoryRoot 'libs/linux/x86_64/libbass*.so') | Copy-Item -Destination (Join-Path $appDir 'usr/lib')
    Copy-Required (Get-NativePublishFile $PublishDirectory 'linux-x64' 'libuiohook.so') (Join-Path $appDir 'usr/lib/libuiohook.so')
    Copy-Item (Join-Path $RepositoryRoot 'locales/*') -Destination (Join-Path $appDir 'usr/locales')
    Invoke-External (Require-Command 'chmod' 'Install coreutils.') @('+x', (Join-Path $appDir 'AppRun'), (Join-Path $appDir 'usr/bin/Jammer'))
    $artifact = Join-Path $OutputRoot "jammer-$Version-x86_64.AppImage"
    $previousArch = $env:ARCH
    $previousExtractAndRun = $env:APPIMAGE_EXTRACT_AND_RUN
    $env:ARCH = 'x86_64'
    $env:APPIMAGE_EXTRACT_AND_RUN = '1'
    try {
        Invoke-External $AppImageTool @($appDir, $artifact)
    } finally {
        if ($null -eq $previousArch) { Remove-Item Env:ARCH -ErrorAction SilentlyContinue } else { $env:ARCH = $previousArch }
        if ($null -eq $previousExtractAndRun) { Remove-Item Env:APPIMAGE_EXTRACT_AND_RUN -ErrorAction SilentlyContinue } else { $env:APPIMAGE_EXTRACT_AND_RUN = $previousExtractAndRun }
    }
}

function Package-Windows([string]$PublishDirectory) {
    if ($HostPlatform -ne 'windows') { throw 'NSIS installer packaging must run on Windows. Use -SkipPackage elsewhere to validate publishing.' }
    $MakeNsis = Require-Command 'makensis' 'Install NSIS and add makensis.exe to PATH.'
    $stage = Join-Path $OutputRoot 'staging/nsis'
    if (Test-Path $stage) { Remove-Item -Recurse -Force $stage }
    New-Item -ItemType Directory -Force (Join-Path $stage 'locales'), (Join-Path $stage 'docs') | Out-Null
    Copy-Required (Join-Path $PublishDirectory 'Jammer.CLI.exe') (Join-Path $stage 'Jammer.exe')
    Copy-Required (Get-NativePublishFile $PublishDirectory 'win-x64' 'uiohook.dll') (Join-Path $stage 'uiohook.dll')
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
        Invoke-External $Lipo @((Join-Path $nativeRoot $name), '-verify_arch', 'x86_64', 'arm64')
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
