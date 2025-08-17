#!/bin/bash
set -e -b
cd "$(dirname "$0")/.."

dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true
mkdir -p Jammer.AppDir/usr/{bin,lib,locales}
cp -v bin/Release/net8.0/linux-x64/publish/Jammer Jammer.AppDir/usr/bin
cp -v libs/linux/x86_64/libbass* Jammer.AppDir/usr/lib
cp -v locales/* Jammer.AppDir/usr/locales
ARCH=x86_64 ./appimagetool-x86_64.AppImage Jammer.AppDir Jammer-test.AppImage
./Jammer-test.AppImage -D $@
