#!/bin/bash
# wimlib-msys2.sh: compile wimlib for Windows

# Sometimes, llvm-mingw cannot cross compile wimlib-imagex.exe.
# It may happen when the LLVM linker is used.
# If it happens, try using default ld from MSYS2 MinGW

# [*] Check script arguments
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
# [*] Parse <FILE_SRCDIR>
shift $(( OPTIND - 1 ))
SRCDIR="$@"
if ! [[ -d "${SRCDIR}" ]]; then
    echo "[${SRCDIR}] is not a directory!" >&2
    exit 1
fi

# [*] Set target triple
TARGET_TRIPLE="${ARCH}-w64-mingw32"
# -static-libgcc causes warning in clang. Supress it with -Wno-unused-command-line-argument.
WIMLIB_CFLAGS="-Os -static-libgcc -Wno-unused-command-line-argument "
#WIMLIB_LDFLAGS="-lucrt"  # Force linking to UCRT, to unify CRT with .NET runtime
#WIMLIB_LD="ld"
#WIMLIB_LDFLAGS="-m i386pe"
WIMLIB_LDFLAGS=""
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
    echo "[${ARCH}] is not a supported architecture" >&2
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
if ! command -v "${STRIP}" &> /dev/null
then
    STRIP="strip"
fi

BASE_DIR=$(dirname "${BASE_ABS_PATH}")
DEST_DIR="${BASE_DIR}/build-bin-${ARCH}"
rm -rf "${DEST_DIR}"
mkdir -p "${DEST_DIR}"

# Required dependencies: nasm (x86_64 only)
# MSYS2: pacman -S nasm
which nasm > /dev/null
if [[ $? -ne 0 ]]; then # Unable to find nasm
    echo "Please install nasm!" >&2
    echo "Run \"pacman -S nasm\"." >&2
    exit 1
fi

# Let custom toolchain is called first in PATH
if ! [[ -z "${TOOLCHAIN_DIR}" ]]; then
    export PATH=${TOOLCHAIN_DIR}/bin:${PATH}
fi

# Compile wimlib
# Adapted from https://wimlib.net/git/?p=wimlib;a=tree;f=tools/make-windows-release;
pushd "${SRCDIR}" > /dev/null
make clean
# ./configure --host=${TARGET_TRIPLE} --disable-static CFLAGS="-static-libgcc" \
./configure --host=${TARGET_TRIPLE} --disable-static \
    CFLAGS="${WIMLIB_CFLAGS}" LDFLAGS="${WIMLIB_LDFLAGS}" \
    --without-ntfs-3g --without-fuse \
    ${EXTRA_ARGS}
if [[ $? -ne 0 ]]; then # configure failed
    echo "./configure failed, please check config.log." >&2
    exit 1
fi
make -j${CORES}
cp ".libs/${DEST_LIB}" "${DEST_DIR}"
popd > /dev/null

# Strip binaries
pushd "${DEST_DIR}" > /dev/null
ls -lh *.dll
${STRIP} *.dll
ls -lh *.dll
popd > /dev/null

# Check dependency of binaries
pushd "${DEST_DIR}" > /dev/null
${CHECKDEP} *.dll
popd > /dev/null

# winpthreads-1.dll warning
echo ""
echo "Please check if winpthreads-1.dll is required."
 