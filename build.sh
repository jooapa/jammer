#!/bin/bash
set -e -b

cd Jammer.CLI
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true
cd ..

mkdir -p Jammer.AppDir/usr/{bin,lib,locales}
cp -v Jammer.CLI/bin/Release/net7.0/linux-x64/publish/Jammer.CLI jammer.AppDir/usr/bin/jammer
cp -v libs/linux/x86_64/libbass* Jammer.AppDir/usr/lib
cp -v locales/* Jammer.AppDir/usr/locales
ARCH=x86_64 ./appimagetool-x86_64.AppImage Jammer.AppDir jammer-$(cat VERSION)-x86_64.AppImage
