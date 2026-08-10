#!/usr/bin/env bash

check_and_install_submodules() {
    if [ ! -d .git ]; then
        echo "This is not a git repository."
        exit 1
    fi

    if [ ! -f .gitmodules ]; then
        echo "No submodules found in this repository."
        return
    fi

    if git submodule status | grep -q '^[-+]'; then
        echo "Initializing and updating submodules..."
        git submodule update --init --recursive
    else
        echo "All submodules are up to date."
    fi
}

