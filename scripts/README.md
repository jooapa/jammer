# Scripts

This directory contains helper scripts for building, running, packaging, and local development tasks.

## Release builds

Run all release builds through PowerShell 7:

```powershell
pwsh ./scripts/build.ps1 -Target linux-x64
pwsh ./scripts/build.ps1 -Target win-x64
pwsh ./scripts/build.ps1 -Target osx-x64
pwsh ./scripts/build.ps1 -Target osx-arm64
pwsh ./scripts/build.ps1 -Target all
```

Useful options include `-Configuration Debug`, `-Clean`, `-OutputDirectory <path>`,
`-Version <version>`, and `-SkipPackage`. `-SkipPackage` still runs the explicit,
self-contained, single-file `dotnet publish` and is useful for cross-RID validation.

Packaging constraints are enforced:

- Linux AppImage creation requires a Linux x64 host. The build uses `appimagetool` from
  `PATH` when available; otherwise it downloads the official x86_64 AppImage into
  `<output>/tools/` and caches it there. The first packaging run therefore needs network
  access when `appimagetool` is not already installed.
- Windows installer creation requires Windows and `makensis` on `PATH`.
- macOS archives require macOS and universal `libbass.dylib`, `libbassmidi.dylib`, and
  `libbassopus.dylib` under `libs/macos/universal/`. BASS AAC is not used on macOS. Both
  x64 and arm64 slices are verified with `lipo`; archives are unsigned and unnotarized.
  Install `ffmpeg` separately and keep it on `PATH`.

The script never bundles yt-dlp. Jammer installs it into its user-writable tools directory.

## Local .NET install on unsupported Linux distros

If your distro does not provide a usable `dotnet` package, you can use `dotnet-install.sh` to install a local .NET SDK without changing system packages.

`dotnet-install.sh` is the official Microsoft install script copied into this repository for convenience.

Example:

```bash
./scripts/dotnet-install.sh --channel 8.0 --install-dir "$HOME/.dotnet"
export DOTNET_ROOT="$HOME/.dotnet"
export PATH="$DOTNET_ROOT:$PATH"
```

After that, you can build Jammer normally:

```bash
dotnet build jammer.sln
```
