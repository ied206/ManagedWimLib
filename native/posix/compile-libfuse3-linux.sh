#!/bin/bash
# static_lifuse3.sh: compile static libfuse3

# Check script arguments
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
SRC_DIR="$@"
if ! [[ -d "${SRC_DIR}" ]]; then
    print_help
    echo "Source [${SRC_DIR}] is not a directory!" >&2
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

# Check command
function check_cmd() {
    local TOCHECK=$1
    local APT_INSTALL_PKG=$2
    local BREW_INSTALL_PKG=$3
    which $TOCHECK > /dev/null
    if [[ $? -ne 0 ]]; then
        echo "Please install ${TOCHECK}!" >&2
        if [[ "${OS}" == Linux && "${APT_INSTALL_PKG}" != "" ]]; then 
            echo "Run \"sudo apt install ${APT_INSTALL_PKG}\"." >&2
        elif [[ "${OS}" == Darwin && "${BREW_INSTALL_PKG}" != "" ]]; then
            echo "Run \"brew install ${BREW_INSTALL_PKG}\"." >&2
        fi
        exit 1
    fi
}

check_cmd meson meson meson

# Set target triple (for Linux) or mac_arch (for macOS)
EXTRA_ARGS=""
if [[ "${CROSS_ARCH}" != "" ]]; then
    if [[ "${OS}" == Linux ]]; then
        if [[ "${CROSS_ARCH}" == i686 ]]; then
            TARGET_TRIPLE="i686-linux-gnu"
        elif [[ "${CROSS_ARCH}" == x86_64 ]]; then
            TARGET_TRIPLE="x86_64-linux-gnu"
        elif [[ "${CROSS_ARCH}" == armhf ]]; then
            TARGET_TRIPLE="arm-linux-gnueabihf"
        elif [[ "${CROSS_ARCH}" == aarch64 || "${CROSS_ARCH}" == arm64 ]]; then
            TARGET_TRIPLE="aarch64-linux-gnu"
            CROSS_ARCH=aarch64
        else
            echo "[${CROSS_ARCH}] is not a pre-defined architecture" >&2
            exit 1
        fi

        # Check for required toolchain
        check_cmd "${TARGET_TRIPLE}-gcc" "gcc-${TARGET_TRIPLE}"
        check_cmd "${TARGET_TRIPLE}-ld" "binutils-${TARGET_TRIPLE}"
        check_cmd "${TARGET_TRIPLE}-pkg-config" "pkg-config-${TARGET_TRIPLE}"

        LIB_PREFIX="${LIB_PREFIX}-${CROSS_ARCH}"
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
rm -rf "${LIB_PREFIX}"
mkdir -p "${LIB_PREFIX}"

# Compile libfuse
# Adapted from https://wimlib.net/git/?p=wimlib;a=tree;f=tools/windeps/Makefile;
pushd "${SRC_DIR}" > /dev/null
rm -rf build-meson
mkdir build-meson
meson setup build-meson . \
    ${EXTRA_ARGS} \
    --strip --default-library shared \
    -Dc_args="-Os" \
    --prefix "${LIB_PREFIX}" \
    --buildtype release \
    -Dutils=false -Dexamples=false -Duseroot=false -Ddisable-mtab=false
cd build-meson
ninja -j "${CORES}"
ninja install
popd > /dev/null

