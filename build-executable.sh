#!/bin/bash

# Change to the directory of the script
cd "$(dirname "$0")"

# Clean up previous build artifacts
rm -rf build
mkdir build

cd Jammer.CLI
rm -rf bin obj

# Check if LD_LIBRARY_PATH includes the target directory
if [[ ! $LD_LIBRARY_PATH == *"../libs/linux/x86_64"* ]]; then
    # Append the current directory to LD_LIBRARY_PATH
    export LD_LIBRARY_PATH=$LD_LIBRARY_PATH:$(realpath ../libs/linux/x86_64)
    # Print updated LD_LIBRARY_PATH
    echo "Updated LD_LIBRARY_PATH: $LD_LIBRARY_PATH"
fi

# Publish the .NET application
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true
cd ..

# Copy necessary files to build directory
cp Jammer.CLI/bin/Release/net8.0/linux-x64/publish/Jammer.CLI ./build/jammer
cp Jammer.CLI/bin/Release/net8.0/linux-x64/publish/libuiohook.so ./build/libuiohook.so
cp -r locales ./build/
cp -r docs ./build/
cp libs/linux/x86_64/*.so ./build/

# Create a temporary directory for tar extraction
EXTRACT_ARCHIVE=JAMMER_BUNDLED_TARGZ
mkdir $EXTRACT_ARCHIVE

# Copy files into the temporary directory
cp -r ./build/* ./$EXTRACT_ARCHIVE/

# Create the tar archive
tar -czvf jammer.tar.gz $EXTRACT_ARCHIVE/*

# Clean up temporary directory
rm -rf $EXTRACT_ARCHIVE
