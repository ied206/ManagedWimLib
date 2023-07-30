#!/bin/bash
# static_libxml.sh: compile static libxml

# Check script arguments
if [[ "$#" -ne 1 ]]; then
    echo "Usage: $0 <LIBXML2_SRCDIR>" >&2
    exit 1
fi
if ! [[ -d "$1" ]]; then
    echo "[$1] is not a directory!" >&2
    exit 1
fi
SRCDIR=$1

# Query environment info
OS=$(uname -s) # Linux, Darwin, MINGW64_NT-10.0-18363, MSYS_NT-10.0-18363, ...

# Set path and command vars
if [ "${OS}" = Linux ]; then
    BASE_ABS_PATH=$(readlink -f "$0")
    CORES=$(grep -c ^processor /proc/cpuinfo)
elif [ "${OS}" = Darwin ]; then
    BASE_ABS_PATH="$(cd $(dirname "$0");pwd)/$(basename "$0")"
    CORES=$(sysctl -n hw.logicalcpu)
else
    echo "${OS} is not a supported platform!" >&2
    exit 1
fi
BASE_DIR=$(dirname "${BASE_ABS_PATH}")
LIB_PREFIX="${BASE_DIR}/build-prefix"

# Create prefix directory
rm -rf "${LIB_PREFIX}"
mkdir -p "${LIB_PREFIX}"

# Compile libxml2
# Adapted from https://wimlib.net/git/?p=wimlib;a=tree;f=tools/windeps/Makefile;
pushd "${SRCDIR}" > /dev/null
make clean
./configure --enable-static --disable-shared \
    --prefix="${LIB_PREFIX}" CFLAGS="-Os -fPIC" \
    --with-zlib=no --with-lzma=no --with-iconv=no \
    --with-minimum --with-tree --with-writer
make "-j${CORES}"
make install
popd > /dev/null
