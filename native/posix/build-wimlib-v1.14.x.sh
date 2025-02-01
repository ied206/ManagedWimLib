#!/bin/bash
# static_wimlib.sh: compile shared wimlib linked with static libxml

function print_help() {
    echo "Usage: $0 [-a armhf|aarch64] <SRC_DIR>" >&2
    echo "" >&2
    echo "-a: Specify architecture for cross-compiling (Optional)" >&2
}

# Check script arguments
CROSS_ARCH=""
while getopts "a:h" opt; do
    case $opt in
        a) # pre-defined Architecture for cross-compile
            CROSS_ARCH=$OPTARG
            ;;
        h)
            print_help
            exit 1
            ;;
        :)
            print_help
            exit 1
            ;;
    esac
done
# Parse <SRC_DIR>
shift $(( OPTIND - 1 ))
SRCDIR="$@"
if ! [[ -d "${SRCDIR}" ]]; then
    print_help
    echo "Source [${SRCDIR}] is not a directory!" >&2
    exit 1
fi

# Query environment info
ARCH=$(uname -m) # x86_64, armv7l, aarch64, ...
OS=$(uname -s) # Linux, Darwin, MINGW64_NT-10.0-18363, MSYS_NT-10.0-18363, ...

# Set path and command vars
# BASE_ABS_PATH: Absolute path of this script, e.g. /home/user/bin/foo.sh
# BASE_DIR: Absolute path of the parent dir of this script, e.g. /home/user/bin
if [[ "${OS}" == Linux ]]; then
    BASE_ABS_PATH=$(readlink -f "$0")
    CORES=$(grep -c ^processor /proc/cpuinfo)
    DEST_LIB="libwim.so"
    STRIP="strip"
    CHECKDEP="ldd"
elif [[ "${OS}" == Darwin ]]; then
    export MACOSX_DEPLOYMENT_TARGET=11
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

# Required dependencies
# Debian/Ubuntu: sudo apt install libfuse3-dev nasm pkg-config
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

# Set target triple (for Linux) or mac_arch (for macOS)
TARGET_TRIPLE=""
TARGET_MAC_ARCH=""
if [[ "${OS}" == Linux ]]; then
    if [[ "${CROSS_ARCH}" == i686 ]]; then
        TARGET_TRIPLE="i686-linux-gnu"
    elif [[ "${CROSS_ARCH}" == x86_64 ]]; then
        TARGET_TRIPLE="x86_64-linux-gnu"
    elif [[ "${CROSS_ARCH}" == armhf ]]; then
        TARGET_TRIPLE="arm-linux-gnueabihf"
    elif [[ "${CROSS_ARCH}" == aarch64 || "${CROSS_ARCH}" == arm64 ]]; then
        TARGET_TRIPLE="aarch64-linux-gnu"
    elif [[ "${CROSS_ARCH}" != "" ]]; then
        echo "[${ARCH}] is not a pre-defined architecture" >&2
        exit 1
    fi

    if [[ "${CROSS_ARCH}" != "" ]]; then
        DEST_DIR="${DEST_DIR}-${CROSS_ARCH}"
        LIB_PREFIX="${LIB_PREFIX}-${CROSS_ARCH}"
    fi
    if [ "${TARGET_TRIPLE}" != "" ]; then
        echo "(Cross compile) Target triple set to [${TARGET_TRIPLE}]"
    fi 
elif [[ "${OS}" == Darwin ]]; then
    # https://developer.apple.com/documentation/apple-silicon/building-a-universal-macos-binary
    # https://gist.github.com/andrewgrant/477c7037b1fc0dd7275109d3f2254ea9
    if [[ "${CROSS_ARCH}" == x86_64 ]]; then
        TARGET_MAC_ARCH="x86_64"
    elif [[ "${CROSS_ARCH}" == aarch64 || "${CROSS_ARCH}" == arm64 ]]; then
        TARGET_MAC_ARCH="arm64"
    elif [[ "${CROSS_ARCH}" != "" ]]; then
        echo "[${ARCH}] is not a pre-defined architecture" >&2
        exit 1
    fi

    if [[ "${TARGET_MAC_ARCH}" != "" ]]; then
        DEST_DIR="${DEST_DIR}-${TARGET_MAC_ARCH}"
        LIB_PREFIX="${LIB_PREFIX}-${TARGET_MAC_ARCH}"
        echo "(Cross compile) Target architecture set to [${TARGET_MAC_ARCH}]"
    fi 
fi
PKGCONF_DIR="${LIB_PREFIX}/lib/pkgconfig"

# Turn off fuse on macOS build
if [[ "${OS}" == Darwin ]]; then 
    EXTRA_ARGS="${EXTRA_ARGS} --without-fuse"
fi

# Cross compile
if [[ "${TARGET_TRIPLE}" != "" ]]; then
    EXTRA_ARGS="${EXTRA_ARGS} --host=${TARGET_TRIPLE}"
fi 
if [[ "${TARGET_MAC_ARCH}" != "" ]]; then
    CPPFLAGS="${CPPFLAGS} -arch ${TARGET_MAC_ARCH}"
    CFLAGS="${CFLAGS} -arch ${TARGET_MAC_ARCH}"
    LDFLAGS="${LDFLAGS} -arch ${TARGET_MAC_ARCH}"
    #CPPFLAGS="${CPPFLAGS} --target=${TARGET_ARCH}"
    #CFLAGS="${CFLAGS} --target=${TARGET_ARCH}"
    #LDFLAGS="${LDFLAGS} --target=${TARGET_ARCHi}"
fi

rm -rf "${DEST_DIR}"
mkdir -p "${DEST_DIR}"

# Compile wimlib
# Adapted from https://wimlib.net/git/?p=wimlib;a=tree;f=tools/make-windows-release;
pushd "${SRCDIR}" > /dev/null
make clean
./configure --disable-static --enable-shared \
    --without-ntfs-3g \
    ${EXTRA_ARGS} \
    CPPFLAGS="${CPPFLAGS}" \
    CFLAGS="${CFLAGS} -Os" \
    LDFLAGS="${LDFLAGS}"

make "-j${CORES}"
cp ".libs/${DEST_LIB}" "${DEST_DIR}"
popd > /dev/null

# Strip a binary
pushd "${DEST_DIR}" > /dev/null
ls -lh "${DEST_LIB}"
${STRIP} "${DEST_LIB}"
ls -lh "${DEST_LIB}"
popd > /dev/null

# Check dependency of a binary
pushd "${DEST_DIR}" > /dev/null
file "${DEST_LIB}"
${CHECKDEP} "${DEST_LIB}"
popd > /dev/null

