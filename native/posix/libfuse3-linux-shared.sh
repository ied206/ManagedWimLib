#!/bin/bash
# static_lifuse3.sh: compile static libfuse3

# Check script arguments
if [[ "$#" -ne 1 ]]; then
    echo "Usage: $0 <LIBFUSE3_SRCDIR>" >&2
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
    STRIP="strip"
    CHECKDEP="ldd"
elif [ "${OS}" = Darwin ]; then
    BASE_ABS_PATH="$(cd $(dirname "$0");pwd)/$(basename "$0")"
    CORES=$(sysctl -n hw.logicalcpu)
    STRIP="strip -x"
    CHECKDEP="otool -L"
else
    echo "${OS} is not a supported platform!" >&2
    exit 1
fi
BASE_DIR=$(dirname "${BASE_ABS_PATH}")
LIB_PREFIX="${BASE_DIR}/build-prefix"
PKGCONF_DIR="${LIB_PREFIX}/lib/pkgconfig"

which meson > /dev/null
if [[ $? -ne 0 ]]; then
    echo "Please install meson!" >&2
    exit 1
fi

# Set target triple (for Linux) or mac_arch (for macOS)
EXTRA_ARGS=""
if [[ "${CROSS_ARCH}" != "" ]]; then
    if [[ "${OS}" == Linux ]]; then
        if [[ "${CROSS_ARCH}" == i686 ]]; then
            :
        elif [[ "${CROSS_ARCH}" == x86_64 ]]; then
            :
        elif [[ "${CROSS_ARCH}" == armhf ]]; then
            :
        elif [[ "${CROSS_ARCH}" == aarch64 || "${CROSS_ARCH}" == arm64 ]]; then
            CROSS_ARCH=aarch64
        else
            echo "[${CROSS_ARCH}] is not a pre-defined architecture" >&2
            exit 1
        fi

        EXTRA_ARGS="${EXTRA_ARGS} --cross-file=${BASE_DIR}/fuse-meson-cross/linux-${CROSS_ARCH}.txt"
    elif [[ "${OS}" == Darwin ]]; then
        # https://developer.apple.com/documentation/apple-silicon/building-a-universal-macos-binary
        # https://gist.github.com/andrewgrant/477c7037b1fc0dd7275109d3f2254ea9
        if [[ "${CROSS_ARCH}" == x86_64 ]]; then
            TARGET_ARCH="x86_64"
            #TARGET_ARCH="x86_64-apple-macos"
        elif [[ "${CROSS_ARCH}" == aarch64 || "${CROSS_ARCH}" == arm64 ]]; then
            TARGET_ARCH="arm64"
            #TARGET_ARCH="arm64-apple-macos"
        else
            echo "[${ARCH}] is not a pre-defined architecture" >&2
            exit 1
        fi

        echo "(Cross compile) Target architecture set to [${CROSS_ARCH}]"
    fi
fi
PKGCONF_DIR="${LIB_PREFIX}/lib/pkgconfig"

# Create prefix directory
mkdir -p "${LIB_PREFIX}"

# Compile libfuse
# Adapted from https://wimlib.net/git/?p=wimlib;a=tree;f=tools/windeps/Makefile;
pushd "${PWD}" > /dev/null
cd $1
rm -rf build
mkdir build
#CFLAGS="-Os -fPIC" meson . build \
export CFLAGS="-Os -fPIC"
meson . build ${EXTRA_ARGS} \
    --strip --default-library shared \
    --prefix "${LIB_PREFIX}" \
    --buildtype release \
    -Dutils=false -Dexamples=false -Duseroot=false -Ddisable-mtab=false
cd build
ninja -j "${CORES}"
ninja install
popd > /dev/null

