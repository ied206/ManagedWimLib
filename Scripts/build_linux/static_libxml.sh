#!/bin/bash

# Adapted from https://wimlib.net/git/?p=wimlib;a=tree;f=tools/windeps/Makefile;

# Absolute path to this script, e.g. /home/user/bin/foo.sh
BASE_ABS_PATH=$(readlink -f "$0")
# Absolute path this script is in, thus /home/user/bin
BASE_DIR=$(dirname "${BASE_ABS_PATH}")
CORES=$(grep -c ^processor /proc/cpuinfo)

# libxml2 source directory must be parameter
pushd "${PWD}"
LIB_PREFIX="${HOME}/wimlib-build"
mkdir "${LIB_PREFIX}"
cd "$1"
make clean
./configure --enable-static --disable-shared \
    --prefix="${LIB_PREFIX}" CFLAGS="-Os -fPIC" \
    --with-minimum --without-lzma --with-tree --with-writer
make "-j${CORES}"
make install
popd
