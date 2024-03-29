#!/bin/bash
set -e -b

dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true
mkdir -p jammer.AppDir/usr/{bin,lib}
cp bin/Release/net7.0/linux-x64/publish/jammer jammer.AppDir/usr/bin
cp -v libs/linux/x86_64/libbass* jammer.AppDir/usr/lib
ARCH=x86_64 ./appimagetool-x86_64.AppImage jammer.AppDir jammer-$(cat VERSION).AppImage
