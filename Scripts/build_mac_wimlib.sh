#!/bin/bash

# Adapted from https://wimlib.net/git/?p=wimlib;a=tree;f=tools/make-windows-release;
# Run after running build_linux_static_libxml.sh

# wimlib source directory must be parameter
# Ex)./build_posix_wimlib.sh

# This build requires nasm
# $ brew install nasm
pushd $PWD
LIB_PREFIX=$HOME/wimlib-build
WIMLIB_SRC_DIR=$1
cd $WIMLIB_SRC_DIR
./configure --enable-dynamic --disable-static \
    LIBXML2_CFLAGS="-I$LIB_PREFIX/include/libxml2" \
    LIBXML2_LIBS="-L$LIB_PREFIX/lib" \
    PKG_CONFIG_PATH="$LIB_PREFIX/lib/pkgconfig" \
    --without-libcrypto --without-ntfs-3g --without-fuse --enable-ssse3-sha1
make -j4
cp .libs/*.dylib .
strip ./*.dylib
#make clean
popd
