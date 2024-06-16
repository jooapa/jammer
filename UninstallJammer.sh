#!/bin/bash

INSTALL_DIR="/usr/local/$APP_NAME"
BIN_DIR="/usr/local/bin/$APP_NAME"
LIB_DIR="/usr/local/lib/$APP_NAME"

sudo rm -rf "$INSTALL_DIR"
sudo rm -rf "$BIN_DIR"
sudo rm -rf "$LIB_DIR"