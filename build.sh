#!/bin/bash

set -e
dotnet publish -r linux-x64 -c Release /p:PublishSingleFile=true


