#!/bin/bash

set -e

APP_NAME=jammer
EXTRACT_ARCHIVE=JAMMER_BUNDLED_TARGZ
INSTALL_DIR="/usr/local/$APP_NAME"
BIN_DIR="/usr/local/bin/$APP_NAME"
LIB_DIR="/usr/local/lib/$APP_NAME"

sudo rm -rf "$INSTALL_DIR"
sudo rm -rf "$BIN_DIR"
sudo rm -rf "$LIB_DIR"

# Change to the home directory
cd ~

# Download the latest release
curl -L -o "jammer.tar.gz" "https://github.com/jooapa/signal-Jammer/releases/latest/download/jammer.tar.gz"

# Extract the downloaded tar file
tar -xzvf "jammer.tar.gz"

# Create the installation director
sudo mkdir -p "$INSTALL_DIR"
sudo mkdir -p "$BIN_DIR"
# sudo mkdir -p "$LIB_DIR" # TEMPORARILY REMOVED

# Move shared object files (.so) and the executable to the installation directory
# sudo cp "./$EXTRACT_ARCHIVE"/*.so "$LIB_DIR/"  # TEMPORARILY REMOVED
sudo cp "./$EXTRACT_ARCHIVE"/*.so "$BIN_DIR/"    # TEMPORARY FIX
sudo cp "./$EXTRACT_ARCHIVE"/jammer "$BIN_DIR/"

# Copy locales and docs to the application directory
sudo cp -r "./$EXTRACT_ARCHIVE/locales" "$INSTALL_DIR/"
sudo cp -r "./$EXTRACT_ARCHIVE/docs" "$INSTALL_DIR/"

# Clean up extracted directory and downloaded tar file
rm -rf "$EXTRACT_ARCHIVE"
rm -f "jammer.tar.gz"

# Add the installation directory to the PATH if not already present
if [[ ":$PATH:" != *":$BIN_DIR:"* ]]; then

    export PATH="$BIN_DIR:$PATH"
    echo "Added $BIN_DIR to PATH"
fi
# Add if not already present in .bashrc
if ! grep -q "export PATH=\"$BIN_DIR:\$PATH\"" ~/.bashrc; then
    echo "export PATH=\"$BIN_DIR:\$PATH\"" >> ~/.bashrc
    echo "Added $BIN_DIR to .bashrc"
fi

echo Installation completed
echo run "'source ~/.bashrc'" to update the PATH
echo Run: 'jammer --help' to get started