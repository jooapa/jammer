#!/bin/bash

# Clear the terminal screen
clear

# Check current directory
echo "Current directory: $(pwd)"

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

# Check for sound cards
echo "Checking for sound cards..."
aplay -l

# Check for loaded sound modules
echo "Loaded sound modules:"
lsmod | grep snd

# Check for sound card detection in kernel messages
echo "Kernel messages for sound card detection:"
dmesg | grep snd

# Run the .NET application with any passed arguments, suppressing build output
dotnet run -- "$@"
