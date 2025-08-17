#!/bin/bash
set -e -b
DOTNET_CLI_TELEMETRY_OPTOUT=1

cd "$(dirname "$0")/.."

if [ -f jammer-*.AppImage ]; then
    rm jammer-*.AppImage
fi

if [ -f jammer.AppDir/user/bin/Jammer ]; then
    rm jammer.AppDir/usr/bin/Jammer
fi

cd Jammer.CLI
rm -rf bin
rm -rf obj
# Check if LD_LIBRARY_PATH includes the target directory
if [[ ! $LD_LIBRARY_PATH == *"../libs/linux/x86_64"* ]]; then
    # Change to the target directory
    cd ../libs/linux/x86_64
    # Append the current directory to LD_LIBRARY_PATH
    export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:$(pwd)
    # Print updated LD_LIBRARY_PATH
    echo "Updated LD_LIBRARY_PATH: $LD_LIBRARY_PATH"
    # Return to the previous directory
    cd -
fi
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true
cd ..

mkdir -p jammer.AppDir/usr/{bin,lib,locales}
cp -v Jammer.CLI/bin/Release/net8.0/linux-x64/publish/Jammer.CLI jammer.AppDir/usr/bin/Jammer
cp -v Jammer.CLI/bin/Release/net8.0/linux-x64/publish/libuiohook.so jammer.AppDir/usr/lib/libuiohook.so
cp -v libs/linux/x86_64/libbass* jammer.AppDir/usr/lib
cp -v locales/* jammer.AppDir/usr/locales

if [ ! -f ./appimagetool-x86_64.AppImage ]; then
        curl -LO https://github.com/AppImage/appimagetool/releases/download/continuous/appimagetool-x86_64.AppImage
        chmod 700 ./appimagetool-x86_64.AppImage
fi

ARCH=x86_64 ./appimagetool-x86_64.AppImage jammer.AppDir jammer-$(cat VERSION)-x86_64.AppImage

#./jammer-$(cat VERSION)-x86_64.AppImage 
