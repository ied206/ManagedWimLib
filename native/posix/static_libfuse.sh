#!/bin/bash
# static_lifuse.sh: compile static libxml

# Check script arguments
if [[ "$#" -ne 1 ]]; then
    echo "Usage: $0 <LIBFUSE_SRCDIR>" >&2
    exit 1
fi
if ! [[ -d "$1" ]]; then
    echo "[$1] is not a directory!" >&2
    exit 1
fi

# Query environment info
ARCH=$(uname -m) # x86_64, armv7l, aarch64, ...
OS=$(uname -s) # Linux, Darwin, MINGW64_NT-10.0-18363, MSYS_NT-10.0-18363, ...

# Set path and command vars
# BASE_ABS_PATH: Absolute path of this script, e.g. /home/user/bin/foo.sh
# BASE_DIR: Absolute path of the parent dir of this script, e.g. /home/user/bin
if [ "${OS}" = Linux ]; then
    BASE_ABS_PATH=$(readlink -f "$0")
    CORES=$(grep -c ^processor /proc/cpuinfo)
    DEST_LIB="libwim.so"
    STRIP="strip"
    CHECKDEP="ldd"
elif [ "${OS}" = Darwin ]; then
    BASE_ABS_PATH="$(cd $(dirname "$0");pwd)/$(basename "$0")"
    CORES=$(sysctl -n hw.logicalcpu)
    DEST_LIB="libwim.dylib"
    STRIP="strip -x"
    CHECKDEP="otool -L"
else
    echo "${OS} is not a supported platform!" >&2
    exit 1
fi
BASE_DIR=$(dirname "${BASE_ABS_PATH}")
LIB_PREFIX="${HOME}/wimlib-build"
PKGCONF_DIR="${LIB_PREFIX}/lib/pkgconfig"

# Create prefix directory
mkdir "${LIB_PREFIX}"

# Compile libfuse
# Adapted from https://wimlib.net/git/?p=wimlib;a=tree;f=tools/windeps/Makefile;
pushd "${PWD}" > /dev/null
cd $1
rm -rf build
mkdir build
CFLAGS="-Os -fPIC" meson . build \
    --strip --default-library static \
    --prefix "${LIB_PREFIX}" \
    --buildtype release \
    -Dutils=false -Dexamples=false -Duseroot=false -Ddisable-mtab=false
cd build
ninja -j "${CORES}"
ninja install
popd > /dev/null
