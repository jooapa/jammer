#!/bin/bash
# Build'n'run

LD_LIBRARY_PATH=libs/linux/x86_64:$LD_LIBRARY_PATH dotnet run --project Jammer.CLI -- "${@:1}"
