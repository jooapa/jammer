#!/bin/bash
cd ~/Documents/GitHub/jammer/Jammer.CLI
export LD_LIBRARY_PATH=~/Documents/GitHub/jammer/libs/linux/x86_64:$LD_LIBRARY_PATH
dotnet run -- "$@"