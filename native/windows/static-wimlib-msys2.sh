#!/bin/bash
# static_wimlib.sh: compile shared wimlib linked with static libxml

# Check script arguments
while getopts "a:t:" opt; do
  case $opt in
    a) # architecture
      ARCH=$OPTARG
      ;;
    t) # toolchain, required for aarch64
      TOOLCHAIN_DIR=$OPTARG
      ;;
    :)
      echo "Usage: $0 <-a i686|x86_64|aarch64> [-t TOOLCHAIN_DIR] <WIMLIB_SRCDIR>" >&2
      exit 1
      ;;
  esac
done
# Parse <FILE_SRCDIR>
shift $(( OPTIND - 1 ))
SRCDIR="$@"
if ! [[ -d "${SRCDIR}" ]]; then
    echo "[${SRCDIR}] is not a directory!" >&2
    exit 1
fi

# Set target triple
TARGET_TRIPLE="${ARCH}-w64-mingw32"
if [ "${ARCH}" = i686 ]; then
    :
elif [ "${ARCH}" = x86_64 ]; then
    # Turn on SSSE3 asm optimization on x64 build
    EXTRA_ARGS="--enable-ssse3-sha1"
elif [ "${ARCH}" = aarch64 ]; then
    # Check custom toolchain, as MinGW does not support ARM64 build
    if [[ -z "${TOOLCHAIN_DIR}" ]]; then
        echo "Please provide llvm-mingw as [TOOLCHAIN_DIR] for aarch64 build." >&2
        exit 1
    fi
else
    echo "${ARCH} is not a supported architecture" >&2
    exit 1
fi

# Set path and command vars
# BASE_ABS_PATH: Absolute path of this script, e.g. /home/user/bin/foo.sh
# BASE_DIR: Absolute path of the parent dir of this script, e.g. /home/user/bin
BASE_ABS_PATH=$(readlink -f "$0")
CORES=$(grep -c ^processor /proc/cpuinfo)
DEST_LIB="libwim-15.dll"
STRIP="${TARGET_TRIPLE}-strip"
CHECKDEP="ldd"

BASE_DIR=$(dirname "${BASE_ABS_PATH}")
DEST_DIR="${BASE_DIR}/build-bin-${ARCH}"
LIB_PREFIX="${BASE_DIR}/build-prefix-${ARCH}"
PKGCONF_DIR="${LIB_PREFIX}/lib/pkgconfig"
mkdir -p "${DEST_DIR}"

# Check if libxml2 was properly compiled
if ! [[ -d "${LIB_PREFIX}" ]]; then
    echo "Prefix directory [${LIB_PREFIX}] not found!" >&2
    echo "Please run [static-libxml2-msys2.sh] first." >&2
    exit 1
fi
if ! [[ -d "${PKGCONF_DIR}" ]]; then
    echo "PKGCONFIG directory [${PKGCONF_DIR}] not found!" >&2
    echo "Please run [static-libxml2-msys2.sh] first. " >&2
    exit 1
fi
if ! [[ -s "${PKGCONF_DIR}/libxml-2.0.pc" ]]; then
    echo "libxml2 not installed in [${LIB_PREFIX}]!" >&2
    echo "Please run [static-libxml2-msys2.sh] first. " >&2
    exit 1
fi

# Required dependencies: nasm
# MSYS2: pacman -S nasm
which nasm > /dev/null
if [[ $? -ne 0 ]]; then # Unable to find nasm
    echo "Please install nasm!" >&2
    echo "Run \"pacman -S nasm\"." >&2
    exit 1
fi

# Compile wimlib
# Adapted from https://wimlib.net/git/?p=wimlib;a=tree;f=tools/make-windows-release;
pushd "${SRCDIR}" > /dev/null
make clean
# ./configure --host=${TARGET_TRIPLE} --disable-static \
./configure --host=${TARGET_TRIPLE} \
    CPPFLAGS="-I${LIB_PREFIX}/include" LDFLAGS="-L${LIB_PREFIX}/lib" PKG_CONFIG_PATH="${PKGCONF_DIR}" \
    --without-libcrypto --without-ntfs-3g --without-fuse \
    ${EXTRA_ARGS}
make -j${CORES}
cp ".libs/${DEST_LIB}" "${DEST_DIR}/${DEST_LIB}"
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
