#!/bin/bash
set -e -b

dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true -p:UseForms=false Jammer.CLI/Jammer.CLI.csproj
mkdir -p jammer.AppDir/usr/{bin,lib,locales}
cp -v Jammer.CLI/bin/Release/net7.0/linux-x64/publish/Jammer.CLI jammer.AppDir/usr/bin/jammer
cp -v libs/linux/x86_64/libbass* jammer.AppDir/usr/lib
cp -v locales/* jammer.AppDir/usr/locales
ARCH=x86_64 ./appimagetool-x86_64.AppImage jammer.AppDir jammer-$(cat VERSION).AppImage
