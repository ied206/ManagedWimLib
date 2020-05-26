#!/bin/bash
# static_libxml.sh: compile static libxml

# Check script arguments
if [[ "$#" -ne 1 ]]; then
    echo "Usage: $0 <LIBXML2_SRCDIR>" >&2
    exit 1
fi
if ![[ -d "$1" ]]; then
    echo "[$1] is not a directory!" >&2
    exit 1
fi

# Query environment info
OS=$(uname -s) # Linux, Darwin, MINGW64_NT-10.0-18363, MSYS_NT-10.0-18363, ...

# Set path and command vars
if [ "${OS}" = Linux ]; then
    CORES=$(grep -c ^processor /proc/cpuinfo)
elif [ "${OS}" = Darwin ]; then
    CORES=$(sysctl -n hw.logicalcpu)
else
    echo "${OS} is not a supported platform!" >&2
    exit 1
fi
LIB_PREFIX="${HOME}/wimlib-build"

# Create prefix directory
mkdir "${LIB_PREFIX}"

# Compile and copy wimlib binary
# Adapted from https://wimlib.net/git/?p=wimlib;a=tree;f=tools/windeps/Makefile;
pushd "${PWD}" > /dev/null
cd "$1"
make clean
./configure --enable-static --disable-shared \
    --prefix="${LIB_PREFIX}" CFLAGS="-Os -fPIC" \
    --with-minimum --without-lzma --with-tree --with-writer
make "-j${CORES}"
make install
popd > /dev/null
