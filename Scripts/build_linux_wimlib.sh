#!/bin/bash

# Adapted from https://wimlib.net/git/?p=wimlib;a=tree;f=tools/make-windows-release;
# Run after running build_linux_static_libxml.sh

# Absolute path to this script, e.g. /home/user/bin/foo.sh
BASE_ABS_PATH=$(readlink -f "$0")
# Absolute path this script is in, thus /home/user/bin
BASE_DIR=$(dirname "$BASE_ABS_PATH")

# wimlib source directory must be parameter
# Ex)./build_posix_wimlib.sh
# sudo apt install libxml2-dev libfuse-dev
pushd $PWD
WIMLIB_SRC_DIR=$(readlink -f "$1")
cd $WIMLIB_SRC_DIR
if [ $ARCH = x86_64 ]; then
    extra_args="--enable-ssse3-sha1"
fi
./configure --enable-dynamic --disable-static --without-libcrypto --without-ntfs-3g $extra_args
make -j12
cp .libs/*.so $BASE_DIR
strip $BASE_DIR/*.so
make clean
popd
