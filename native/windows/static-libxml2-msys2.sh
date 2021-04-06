#!/bin/bash
# static_libxml.sh: compile static libxml

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
      echo "Usage: $0 <-a i686|x86_64|aarch64> [-t TOOLCHAIN_DIR] <LIBXML2_SRCDIR>" >&2
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
    :
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

# Let custom toolchain is called first in PATH
if ! [[ -z "${TOOLCHAIN_DIR}" ]]; then
    export PATH=${TOOLCHAIN_DIR}/bin:${PATH}
fi

# Set path and command vars
# BASE_ABS_PATH: Absolute path of this script, e.g. /home/user/bin/foo.sh
# BASE_DIR: Absolute path of the parent dir of this script, e.g. /home/user/bin
BASE_ABS_PATH=$(readlink -f "$0")
BASE_DIR=$(dirname "${BASE_ABS_PATH}")
CORES=$(grep -c ^processor /proc/cpuinfo)

# Create prefix directory
LIB_PREFIX="${BASE_DIR}/build-prefix-${ARCH}"
mkdir "${LIB_PREFIX}"

# Compile libxml2
# Adapted from https://wimlib.net/git/?p=wimlib;a=tree;f=tools/windeps/Makefile;
pushd "${SRCDIR}" > /dev/null
make clean
./configure --host=${TARGET_TRIPLE} \
    --with-minimum --with-tree --with-writer \
    --with-zlib=no --with-lzma=no --with-iconv=no \
    --prefix="${LIB_PREFIX}" CFLAGS="-Os"
# --enable-static --disable-shared \ -> If we put this line, libxml2.dll.a is not generated.
make -j${CORES}
make install
popd > /dev/null
