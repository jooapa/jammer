#!/bin/bash

# Download the tarball
wget http://example.com/path/to/my_program_files.tar.gz

cd ./linux_installation

# Extract the contents
tar -xzvf ./jammer.tar.gz

# Make the executable executable
chmod +x ./jammer
ls ./

# Clean up (optional)
rm ./jammer.tar.gz

sudo ln -s "$CURDIR/$1" /usr/local/bin

echo "Installation complete."
