#!/bin/bash
# static_wimlib.sh: compile shared wimlib linked with static libxml

# Check script arguments
if [[ "$#" -ne 1 ]]; then
    echo "Usage: $0 <WIMLIB_SRCDIR>" >&2
    exit 1
fi
if ! [[ -d "$1" ]]; then
    echo "[$1] is not a directory!" >&2
    exit 1
fi
SRCDIR=$1

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
DEST_DIR="${BASE_DIR}/build-bin"
LIB_PREFIX="${BASE_DIR}/build-prefix"
PKGCONF_DIR="${LIB_PREFIX}/lib/pkgconfig"

# Check if libxml2 was properly compiled
if ! [[ -d "${LIB_PREFIX}" ]]; then
    echo "Prefix directory [${LIB_PREFIX}] not found!" >&2
    echo "Please run static_wimlib.sh first." >&2
    exit 1
fi
if ! [[ -d "${PKGCONF_DIR}" ]]; then
    echo "PKGCONFIG directory [${PKGCONF_DIR}] not found!" >&2
    echo "Please run static_wimlib.sh first. " >&2
    exit 1
fi
if ! [[ -s "${PKGCONF_DIR}/libxml-2.0.pc" ]]; then
    echo "libxml2 not installed in [${LIB_PREFIX}]!" >&2
    echo "Please run static_wimlib.sh first. " >&2
    exit 1
fi

# Required dependencies
# Debian/Ubuntu: sudo apt install libfuse-dev nasm pkg-config
# macOS:         brew install nasm
if [ "${ARCH}" = "x86_64" ]; then
    which nasm > /dev/null
    if [[ $? -ne 0 ]]; then # Unable to find nasm
        echo "Please install nasm!" >&2
        if [ "${OS}" = Linux ]; then 
            echo "Run \"sudo apt install nasm\"." >&2
        elif [ "${OS}" = Darwin ]; then 
            echo "Run \"brew install nasm\"." >&2
        fi
        exit 1
    fi
fi

which pkg-config > /dev/null
if [[ $? -ne 0 ]]; then
    echo "Please install pkg-config!" >&2
    if [ "${OS}" = Linux ]; then 
        echo "Run \"sudo apt install pkg-config\"." >&2
    fi
    exit 1
fi

# Prepare to compiled wimlib
# Turn on SSSE3 asm optimization on x64 build
if [ "${ARCH}" = "x86_64" ]; then
    EXTRA_ARGS="--enable-ssse3-sha1"
fi
# Turn off fuse on macOS build
if [ "${OS}" = Darwin ]; then 
    EXTRA_ARGS="${EXTRA_ARGS} --without-fuse"
fi

rm -rf "${DEST_DIR}"
mkdir -p "${DEST_DIR}"

# Compile wimlib
# Adapted from https://wimlib.net/git/?p=wimlib;a=tree;f=tools/make-windows-release;
pushd "${SRCDIR}" > /dev/null
make clean
./configure --disable-static \
    PKG_CONFIG_PATH="${PKGCONF_DIR}" \
    CPPFLAGS="-I${LIB_PREFIX}/include" LDFLAGS="-L${LIB_PREFIX}/lib" \
    --without-libcrypto --without-ntfs-3g ${EXTRA_ARGS}
make "-j${CORES}"
cp ".libs/${DEST_LIB}" "${DEST_DIR}"
popd > /dev/null
#LIBXML2_CFLAGS="-I${LIB_PREFIX}/include/libxml2" \
#LIBXML2_LIBS="-L${LIB_PREFIX}/lib -lxml2" \

# Strip a binary
pushd "${DEST_DIR}" > /dev/null
ls -lh "${DEST_LIB}"
${STRIP} "${DEST_LIB}"
ls -lh "${DEST_LIB}"
popd > /dev/null

# Check dependency of a binary
pushd "${DEST_DIR}" > /dev/null
${CHECKDEP} "${DEST_LIB}"
popd > /dev/null

