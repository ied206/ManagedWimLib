#!/bin/bash
# wimlib-msys2.sh: compile wimlib for Windows

# [*] If libtool fails to link libwim-15.dll with this message,
#     Install proper toolchain of the MSYS2 shell
#     EVEN IF YOU ARE USING LLVM_MINGW!
#   CCLD     libwim.la
#
# *** Warning: linker path does not have real file for library -lntdll.
# *** I have the capability to make that library automatically link in when
# *** you link to this library.  But I can only do this if you have a
# *** shared version of the library, which you do not appear to have
# *** because I did check the linker path looking for a file starting
# *** with libntdll and none of the candidates passed a file format test
# *** using a file magic. Last file checked: C:/Joveler/Tools/llvm-mingw/aarch64-w64-mingw32/lib/libntdll.a
# *** The inter-library dependencies that have been dropped here will be
# *** automatically added whenever a program is linked with this library
# *** or is declared to -dlopen it.

# *** Since this library must not contain undefined symbols,
# *** because either the platform does not support them or
# *** it was explicitly requested with -no-undefined,
# *** libtool will only create a static version of it.


function print_help() {
    echo "Usage: $0 <-a i686|x86_64|aarch64> [-t TOOLCHAIN_DIR] [-r RADARE2_DIR] <SRCDIR>" >&2
}

# [*] Check script arguments
while getopts "a:t:r:h:" opt; do
    case $opt in
    a) # architecture
        ARCH=$OPTARG
        ;;
    t) # toolchain, required for aarch64
        TOOLCHAIN_DIR=$OPTARG
        ;;
    r) # radare2, optional
        RADARE2_DIR=$OPTARG
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
# [*] Parse <SRCDIR>
shift $(( OPTIND - 1 ))
SRCDIR="$@"
if ! [[ -d "${SRCDIR}" ]]; then
    print_help
    echo "Source [${SRCDIR}] is not a directory!" >&2
    exit 1
fi

# [*] Set target triple
TARGET_TRIPLE="${ARCH}-w64-mingw32"
# -static-libgcc causes warning in clang. Supress it with -Wno-unused-command-line-argument.
WIMLIB_CFLAGS="-Os -static-libgcc -Wno-unused-command-line-argument "
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
DEST_EXE="wimlib-imagex.exe"
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

# [*] If radare2 is available, use rabin2 instead of ldd.
# Win32 ldd is not that correct when checking cross-compiled binaries.
if [[ ! -z "${RADARE2_DIR}" ]]; then # -r not set
    RABIN2="${RADARE2_DIR}/bin/rabin2.exe"
    which "${RABIN2}" > /dev/null
    if [[ $? -ne 0 ]]; then # rabin2 does not exist
        RABIN2=
    fi
fi
if [[ -z "${RABIN2}" ]] ;then
    which rabin2 > /dev/null
    if [[ $? -eq 0 ]]; then # rabin is callableUnable to find rabin2
        RABIN2=rabin2
    fi
fi
if [[ ! -z "${RABIN2}" ]]; then
    CHECKDEP="${RABIN2} -Al"
fi

# [*] Required dependencies: nasm (x86_64 only)
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
BUILD_MODES=( "exe" "lib" )
pushd "${SRCDIR}" > /dev/null
for BUILD_MODE in "${BUILD_MODES[@]}"; do
    CONFIGURE_ARGS=""
    if [ "$BUILD_MODE" = "lib" ]; then
        CONFIGURE_ARGS="--disable-static --enable-shared"
        # WIMLIB_LDFLAGS="-no-undefined"
    elif [ "$BUILD_MODE" = "exe" ]; then
        CONFIGURE_ARGS="--enable-static --disable-shared"
    fi
    
    make clean
    # ./configure --host=${TARGET_TRIPLE} --disable-static CFLAGS="-static-libgcc" \
    # --libdir=${SRCDIR} required for cross-compiling wimlib-imagex.exe.
    # If not, libtool automatically include `-L/ucrt64/lib`, causing x86_64 libmsvcrt.a/libmingw32.a to be always linked and cause an error.
    ./configure --host=${TARGET_TRIPLE} --libdir="${SRCDIR}" \
        ${CONFIGURE_ARGS} \
        --without-ntfs-3g --without-fuse \
        "CFLAGS=${WIMLIB_CFLAGS}" "LDFLAGS=${WIMLIB_LDFLAGS}" \
        ${EXTRA_ARGS}
    if [[ $? -ne 0 ]]; then # configure failed
        echo "./configure failed, please check config.log." >&2
        exit 1
    fi
    make -j${CORES}

    if [ "$BUILD_MODE" = "lib" ]; then
        cp ".libs/${DEST_LIB}" "${DEST_DIR}"
    elif [ "$BUILD_MODE" = "exe" ]; then
        cp "${DEST_EXE}" "${DEST_DIR}"
    fi
done
popd > /dev/null

# Strip binaries
pushd "${DEST_DIR}" > /dev/null
echo
echo "[*] Stripping [${DEST_LIB}]" 
ls -lh *.dll *.exe
${STRIP} *.dll *.exe
ls -lh *.dll *.exe
popd > /dev/null

# Check dependency of binaries
pushd "${DEST_DIR}" > /dev/null
echo
echo "[*] Linked libraries of [${DEST_LIB}]" 
${CHECKDEP} *.dll
echo
echo "[*] Linked libraries of [${DEST_EXE}]" 
${CHECKDEP} *.exe
popd > /dev/null
