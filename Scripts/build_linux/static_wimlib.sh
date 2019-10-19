#!/bin/bash

# Adapted from https://wimlib.net/git/?p=wimlib;a=tree;f=tools/make-windows-release;
# Run after running build_linux_static_libxml.sh

# Absolute path to this script, e.g. /home/user/bin/foo.sh
BASE_ABS_PATH=$(readlink -f "$0")
# Absolute path this script is in, thus /home/user/bin
BASE_DIR=$(dirname "$BASE_ABS_PATH")
CORES=$(grep -c ^processor /proc/cpuinfo)

# sudo apt install libfuse-dev
pushd $PWD
LIB_PREFIX=$HOME/wimlib-build
cd $1
if [ $ARCH == x86_64 ]; then
    extra_args="--enable-ssse3-sha1"
fi
make clean
./configure --enable-dynamic --disable-static \
    LIBXML2_CFLAGS="-I$LIB_PREFIX/include/libxml2" \
    LIBXML2_LIBS="-L$LIB_PREFIX/lib -lxml2" \
    --without-libcrypto --without-ntfs-3g $extra_args
make -j$CORES
cp .libs/*.so $BASE_DIR
strip $BASE_DIR/*.so
popd
