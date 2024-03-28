#!/bin/bash

# Check if the required argument is provided
if [ $# -ne 1 ]; then
    echo "Usage: $0 <architecture>"
    echo "aarch64 | armhf | x86 | x64"
    exit 1
fi

# Define the output directory
output="./linux_installation"

# Remove contents if directory exists
if [ -d "$output" ]; then
    rm -rf "$output"/jammer/*
fi
    
# Publish the .NET application
dotnet publish -r linux-$1 -c Release /p:PublishSingleFile=true -o "$output"/jammer

rm -f "$output"/jammer/jammer.pdb

# Copy required shared libraries based on architecture
if [ "$1" == "x64" ]; then
    cp ./libs/linux/x86_64/libbass.so "$output"/jammer
    cp ./libs/linux/x86_64/libbass_aac.so "$output"/jammer
else
    cp ./libs/linux/"$1"/libbass.so "$output"/jammer
    cp ./libs/linux/"$1"/libbass_aac.so "$output"/jammer
fi

cd "$output"

# Create the tarball
tar -czvf jammer.tar.gz ./jammer/

# Move tarball into output directory
# mv jammer.tar.gz "$output"

rm -rf ./jammer/