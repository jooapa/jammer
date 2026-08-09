#!/usr/bin/env python3
"""Cross-platform build and packaging script for Jammer.

This is a Python port of scripts/build.ps1. It publishes the .NET project for
one or more runtime identifiers and optionally packages the results into the
platform-specific distribution formats.
"""

from __future__ import annotations

import argparse
import os
import platform
import re
import shutil
import stat
import struct
import subprocess
import sys
import urllib.request
from pathlib import Path
from typing import Iterable, List, Optional, Tuple

VALID_TARGETS = ("linux-x64", "win-x64", "osx-x64", "osx-arm64", "all")
VALID_CONFIGURATIONS = ("Release", "Debug")
VERSION_PATTERN = re.compile(r"^\d+\.\d+(?:\.\d+)?(?:-[0-9A-Za-z.-]+)?$")


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description="Build and package Jammer.")
    parser.add_argument(
        "-t",
        dest="target",
        choices=VALID_TARGETS,
        default="all",
        help="Runtime identifier to build for (default: all).",
    )
    parser.add_argument(
        "-c",
        dest="configuration",
        choices=VALID_CONFIGURATIONS,
        default="Release",
        help="Build configuration (default: Release).",
    )
    parser.add_argument(
        "-C",
        dest="clean",
        action="store_true",
        help="Remove the output directory before building.",
    )
    parser.add_argument(
        "-o",
        dest="output_directory",
        default="artifacts",
        help="Directory for build outputs (default: artifacts).",
    )
    parser.add_argument(
        "-v",
        dest="version",
        default=None,
        help="Override the version read from the VERSION file.",
    )
    parser.add_argument(
        "-s",
        dest="skip_package",
        action="store_true",
        help="Publish only; skip platform-specific packaging.",
    )
    parser.add_argument(
        "--nsis",
        dest="nsis",
        action="store_true",
        help="Build the Windows NSIS installer (requires makensis or WINE_NSIS_DIR).",
    )
    args = parser.parse_args(argv)

    script_dir = Path(__file__).resolve().parent
    repository_root = script_dir.parent
    project = repository_root / "Jammer.CLI" / "Jammer.CLI.csproj"
    version_file = repository_root / "VERSION"

    version = args.version
    if not version:
        version = version_file.read_text(encoding="utf-8").strip()
    if not VERSION_PATTERN.match(version):
        parser.error(
            f"Invalid version '{version}'. Expected a semantic version such as 1.2 or 1.2.3."
        )

    dotnet = require_command("dotnet", "Install the .NET 8 SDK: https://dotnet.microsoft.com/download/dotnet/8.0")

    if os.path.isabs(args.output_directory):
        output_root = Path(args.output_directory).resolve()
    else:
        output_root = (repository_root / args.output_directory).resolve()

    if args.clean and output_root.exists():
        if output_root == Path(output_root.anchor) or output_root == repository_root:
            raise RuntimeError(f"Refusing to clean unsafe output directory: {output_root}")
        shutil.rmtree(output_root)

    output_root.mkdir(parents=True, exist_ok=True)

    targets = (
        ["linux-x64", "win-x64", "osx-x64", "osx-arm64"]
        if args.target == "all"
        else [args.target]
    )
    host_platform = _detect_host_platform()

    for rid in targets:
        print(f"Publishing Jammer {version} for {rid}...")
        published = publish_target(
            dotnet=dotnet,
            project=project,
            rid=rid,
            configuration=args.configuration,
            version=version,
            output_root=output_root,
        )
        if args.skip_package:
            continue
        if rid == "linux-x64":
            package_linux(
                repository_root=repository_root,
                output_root=output_root,
                publish_directory=published,
                configuration=args.configuration,
                version=version,
                host_platform=host_platform,
            )
        elif rid == "win-x64":
            if not args.nsis:
                print("Skipping Windows NSIS packaging (pass --nsis to enable).")
            else:
                package_windows(
                    repository_root=repository_root,
                    output_root=output_root,
                    publish_directory=published,
                    configuration=args.configuration,
                    version=version,
                    host_platform=host_platform,
                )
        elif rid in ("osx-x64", "osx-arm64"):
            package_macos(
                repository_root=repository_root,
                output_root=output_root,
                rid=rid,
                publish_directory=published,
                configuration=args.configuration,
                version=version,
                host_platform=host_platform,
            )

    print(f"Artifacts are in {output_root}")
    return 0


def _detect_host_platform() -> str:
    system = platform.system()
    if system == "Windows":
        return "windows"
    if system == "Linux":
        return "linux"
    if system == "Darwin":
        return "macos"
    return "unknown"


def require_command(name: str, guidance: str) -> str:
    path = shutil.which(name)
    if path is None:
        raise RuntimeError(f"Required tool '{name}' was not found. {guidance}")
    return path


def invoke_external(command: str, arguments: Iterable[str]) -> None:
    result = subprocess.run([command, *arguments], check=False)
    if result.returncode != 0:
        raise RuntimeError(
            f"Command failed with exit code {result.returncode}: {command} {' '.join(arguments)}"
        )


def _try_invoke_silent(command: str, arguments: Iterable[str]) -> int:
    return subprocess.run([command, *arguments], check=False).returncode


def _resolve_wine_makensis() -> Optional[Tuple[str, List[str]]]:
    """Return (wine, [wine_args..., makensis.exe]) for a Windows NSIS run under Wine, or None."""
    nsis_dir = os.environ.get("WINE_NSIS_DIR")
    if not nsis_dir:
        return None
    nsis_path = Path(nsis_dir)
    makensis_exe = nsis_path / "makensis.exe"
    if not makensis_exe.is_file():
        raise RuntimeError(
            f"WINE_NSIS_DIR is set to '{nsis_dir}' but '{makensis_exe}' was not found."
        )
    wine = shutil.which("wine")
    if wine is None:
        raise RuntimeError(
            "WINE_NSIS_DIR is set but 'wine' was not found on PATH. Install Wine to use it."
        )
    wine_args = os.environ.get("WINE_ARGS", "").split()
    return wine, [*wine_args, str(makensis_exe)]


def publish_target(
    dotnet: str,
    project: Path,
    rid: str,
    configuration: str,
    version: str,
    output_root: Path,
) -> Path:
    publish_directory = output_root / "publish" / rid
    publish_directory.mkdir(parents=True, exist_ok=True)
    arguments = [
        "publish",
        str(project),
        "--runtime",
        rid,
        "--configuration",
        configuration,
        "--self-contained",
        "true",
        "--output",
        str(publish_directory),
        "-p:PublishSingleFile=true",
        "-p:IncludeNativeLibrariesForSelfExtract=false",
        f"-p:Version={version}",
        f"-p:InformationalVersion={version}",
    ]
    if rid in ("osx-x64", "osx-arm64"):
        # RuntimeIdentifier is not propagated to a referenced project's compile in all
        # SDK versions. Keep its package graph and compile symbols in sync explicitly.
        arguments.extend(["-p:DisableSharpHook=true", "-p:DisableBassAac=true"])
    invoke_external(dotnet, arguments)
    return publish_directory


def copy_required(source: Path, destination: Path) -> None:
    if not source.exists():
        raise RuntimeError(f"Required packaging file is missing: {source}")
    destination.parent.mkdir(parents=True, exist_ok=True)
    shutil.copy2(source, destination)


def get_native_publish_file(
    repository_root: Path, publish_directory: Path, rid: str, configuration: str, name: str
) -> Path:
    published_path = publish_directory / name
    if published_path.exists():
        return published_path

    # Some NuGet build targets copy native assets beside the intermediate apphost but
    # do not include them in a single-file publish directory.
    return repository_root / "Jammer.CLI" / "bin" / configuration / "net8.0" / rid / name


def get_appimage_tool(output_root: Path) -> Path:
    installed = shutil.which("appimagetool")
    if installed:
        return Path(installed)

    tool_directory = output_root / "tools"
    tool_path = tool_directory / "appimagetool-x86_64.AppImage"
    if tool_path.exists():
        return tool_path

    download_url = (
        "https://github.com/AppImage/appimagetool/releases/download/continuous/"
        "appimagetool-x86_64.AppImage"
    )
    temporary_path = Path(f"{tool_path}.download")
    tool_directory.mkdir(parents=True, exist_ok=True)
    temporary_path.unlink(missing_ok=True)
    print("appimagetool was not found on PATH; downloading the official x86_64 build...")
    try:
        urllib.request.urlretrieve(download_url, temporary_path)

        # AppImages are ELF files with an additional AI type-2 magic value at byte 8.
        header = temporary_path.read_bytes()[:11]
        if len(header) < 11 or header[:4] != b"\x7fELF" or header[8:11] != b"AI\x02":
            raise RuntimeError("The downloaded file is not a type-2 AppImage.")

        temporary_path.rename(tool_path)
        invoke_external(require_command("chmod", "Install coreutils."), ["+x", str(tool_path)])
    except Exception as exc:
        temporary_path.unlink(missing_ok=True)
        raise RuntimeError(
            f"Could not download appimagetool from {download_url}. "
            f"Check the network connection or install appimagetool on PATH. {exc}"
        ) from exc
    return tool_path


def package_linux(
    repository_root: Path,
    output_root: Path,
    publish_directory: Path,
    configuration: str,
    version: str,
    host_platform: str,
) -> None:
    if host_platform != "linux":
        raise RuntimeError(
            "AppImage packaging must run on a Linux x64 host. Use -SkipPackage elsewhere to validate publishing."
        )

    appimage_tool = get_appimage_tool(output_root)
    app_dir = output_root / "staging" / "jammer.AppDir"
    if app_dir.exists():
        shutil.rmtree(app_dir)

    (app_dir / "usr" / "bin").mkdir(parents=True)
    (app_dir / "usr" / "lib").mkdir(parents=True)
    (app_dir / "usr" / "locales").mkdir(parents=True)

    copy_required(repository_root / "jammer.AppDir" / "AppRun", app_dir / "AppRun")
    copy_required(
        repository_root / "jammer.AppDir" / "jammer.desktop", app_dir / "jammer.desktop"
    )
    copy_required(
        repository_root / "jammer.AppDir" / "Jammer-icon.png", app_dir / "Jammer-icon.png"
    )
    copy_required(
        publish_directory / "Jammer.CLI", app_dir / "usr" / "bin" / "Jammer"
    )

    for bass_lib in (repository_root / "libs" / "linux" / "x86_64").glob("libbass*.so"):
        shutil.copy2(bass_lib, app_dir / "usr" / "lib")

    copy_required(
        get_native_publish_file(
            repository_root, publish_directory, "linux-x64", configuration, "libuiohook.so"
        ),
        app_dir / "usr" / "lib" / "libuiohook.so",
    )

    for locale_file in (repository_root / "locales").iterdir():
        shutil.copy2(locale_file, app_dir / "usr" / "locales")

    invoke_external(
        require_command("chmod", "Install coreutils."),
        ["+x", str(app_dir / "AppRun"), str(app_dir / "usr" / "bin" / "Jammer")],
    )

    artifact = output_root / f"jammer-{version}-x86_64.AppImage"
    previous_arch = os.environ.get("ARCH")
    previous_extract_and_run = os.environ.get("APPIMAGE_EXTRACT_AND_RUN")
    os.environ["ARCH"] = "x86_64"
    os.environ["APPIMAGE_EXTRACT_AND_RUN"] = "1"
    try:
        invoke_external(str(appimage_tool), [str(app_dir), str(artifact)])
    finally:
        if previous_arch is None:
            os.environ.pop("ARCH", None)
        else:
            os.environ["ARCH"] = previous_arch
        if previous_extract_and_run is None:
            os.environ.pop("APPIMAGE_EXTRACT_AND_RUN", None)
        else:
            os.environ["APPIMAGE_EXTRACT_AND_RUN"] = previous_extract_and_run


def package_windows(
    repository_root: Path,
    output_root: Path,
    publish_directory: Path,
    configuration: str,
    version: str,
    host_platform: str,
) -> None:
    if host_platform != "windows" and not shutil.which("makensis"):
        raise RuntimeError(
            "NSIS (makensis) is not installed. Install NSIS to package the Windows installer on this platform, "
            "or use -SkipPackage elsewhere to validate publishing."
        )

    make_nsis = "makensis" if shutil.which("makensis") else require_command(
        "makensis.exe", "Install NSIS and add makensis.exe to PATH."
    )
    stage = output_root / "staging" / "nsis"
    if stage.exists():
        shutil.rmtree(stage)

    (stage / "locales").mkdir(parents=True)
    (stage / "docs").mkdir(parents=True)

    copy_required(
        publish_directory / "Jammer.CLI.exe", stage / "Jammer.exe"
    )
    copy_required(
        get_native_publish_file(
            repository_root, publish_directory, "win-x64", configuration, "uiohook.dll"
        ),
        stage / "uiohook.dll",
    )
    copy_required(
        repository_root / "icons" / "trans_icon512x512.ico", stage / "Jammer.ico"
    )

    for name in ("bass.dll", "bass_aac.dll", "bassmidi.dll", "bassopus.dll", "ffmpeg.exe"):
        copy_required(repository_root / "libs" / "win" / "x64" / name, stage / name)

    for name in (
        "setup.nsi",
        "setup.ps1",
        "uninstall.ps1",
        "run_command.bat",
        "open_with_jammer.cmd",
    ):
        copy_required(repository_root / "nsis" / name, stage / name)

    copy_required(repository_root / "LICENSE", stage / "LICENSE")

    for locale_file in (repository_root / "locales").iterdir():
        shutil.copy2(locale_file, stage / "locales")

    for doc_file in (repository_root / "docs").iterdir():
        shutil.copy2(doc_file, stage / "docs")

    original_cwd = os.getcwd()
    os.chdir(stage)
    try:
        native_rc = _try_invoke_silent(
            make_nsis, [f"-DVERSION={version}", "setup.nsi"]
        )
        if native_rc != 0:
            wine_invocation = _resolve_wine_makensis()
            if wine_invocation is not None:
                wine_cmd, wine_prefix = wine_invocation
                print(
                    f"Native makensis failed (exit {native_rc}); retrying via Wine "
                    f"({wine_cmd} ... {wine_prefix[-1]})."
                )
                invoke_external(
                    wine_cmd,
                    [*wine_prefix, f"-DVERSION={version}", "setup.nsi"],
                )
            else:
                raise RuntimeError(
                    f"Command failed with exit code {native_rc}: "
                    f"{make_nsis} -DVERSION={version} setup.nsi"
                )
    finally:
        os.chdir(original_cwd)

    copy_required(
        stage / f"Jammer-Setup_V{version}.exe",
        output_root / f"Jammer-Setup_V{version}.exe",
    )


def assert_universal_macos_library(path: Path) -> None:
    data = path.read_bytes()
    if len(data) < 28:
        raise RuntimeError(f"macOS native library is too small to be a universal binary: {path}")

    magic = struct.unpack(">I", data[:4])[0]
    if magic == 0xCAFEBABE:
        entry_size = 20
    elif magic == 0xCAFEBABF:
        entry_size = 32
    else:
        raise RuntimeError(f"macOS native library is not a universal Mach-O binary: {path}")

    architecture_count = struct.unpack(">I", data[4:8])[0]
    if architecture_count > (len(data) - 8) // entry_size:
        raise RuntimeError(f"macOS native library has an invalid universal Mach-O header: {path}")

    architectures: set[int] = set()
    for index in range(architecture_count):
        cputype = struct.unpack(">I", data[8 + index * entry_size : 12 + index * entry_size])[0]
        architectures.add(cputype)

    x64_cpu_type = 0x01000007
    arm64_cpu_type = 0x0100000C
    if x64_cpu_type not in architectures or arm64_cpu_type not in architectures:
        raise RuntimeError(f"macOS native library must contain both x86_64 and arm64 slices: {path}")


def package_macos(
    repository_root: Path,
    output_root: Path,
    rid: str,
    publish_directory: Path,
    configuration: str,
    version: str,
    host_platform: str,
) -> None:
    if host_platform not in ("macos", "linux"):
        raise RuntimeError(
            "macOS distribution packaging must run on macOS or Linux. Use -SkipPackage elsewhere to validate publishing."
        )

    native_root = repository_root / "libs" / "macos" / "universal"
    required = ["libbass.dylib", "libbassmidi.dylib", "libbassopus.dylib"]
    missing = [name for name in required if not (native_root / name).exists()]
    if missing:
        raise RuntimeError(
            f"Cannot package {rid}: missing native files in {native_root}: {', '.join(missing)}. "
            "macOS native files must be universal binaries; no runnable archive was created."
        )

    for name in required:
        assert_universal_macos_library(native_root / name)

    bundle_name = f"jammer-{version}-{rid}"
    stage = output_root / "staging" / bundle_name
    if stage.exists():
        shutil.rmtree(stage)

    (stage / "locales").mkdir(parents=True)

    copy_required(publish_directory / "Jammer.CLI", stage / "Jammer.bin")
    for name in required:
        copy_required(native_root / name, stage / name)

    for locale_file in (repository_root / "locales").iterdir():
        shutil.copy2(locale_file, stage / "locales")

    copy_required(repository_root / "LICENSE", stage / "LICENSE")

    launcher = (
        '#!/bin/sh\n'
        'SCRIPT_DIR="$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)"\n'
        'export DYLD_LIBRARY_PATH="$SCRIPT_DIR${DYLD_LIBRARY_PATH:+:$DYLD_LIBRARY_PATH}"\n'
        'exec "$SCRIPT_DIR/Jammer.bin" "$@"\n'
    )
    launcher_path = stage / "Jammer"
    launcher_path.write_text(launcher, encoding="utf-8")
    launcher_path.chmod(launcher_path.stat().st_mode | stat.S_IXUSR | stat.S_IXGRP | stat.S_IXOTH)

    invoke_external(
        require_command("chmod", "chmod is required on macOS."),
        ["+x", str(stage / "Jammer"), str(stage / "Jammer.bin")],
    )

    tar = require_command("tar", "The system tar command is required to create the macOS archive.")
    artifact = output_root / f"{bundle_name}.tar.gz"
    invoke_external(
        tar,
        [
            "-czf",
            str(artifact),
            "-C",
            str(stage.parent),
            stage.name,
        ],
    )


if __name__ == "__main__":
    try:
        sys.exit(main())
    except Exception as error:
        print(f"ERROR: {error}", file=sys.stderr)
        sys.exit(1)
