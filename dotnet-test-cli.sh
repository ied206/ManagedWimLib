#!/bin/bash

# Check script arguments
CROSS_ARCH=""
while getopts "a:" opt; do
    case $opt in
        a) # architecture for cross-compile
            CROSS_ARCH=$OPTARG
            ;;
    esac
done

DOTNET_CMD="dotnet"
if [ "${CROSS_ARCH}" != "" ]; then
    DOTNET_CMD="${DOTNET_CMD}-${CROSS_ARCH}"
    echo "Running as [${DOTNET_CMD}]"
fi

${DOTNET_CMD} test -c Release -l "console;verbosity=detailed" --
