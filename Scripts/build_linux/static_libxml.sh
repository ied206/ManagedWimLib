#!/bin/bash

# Adapted from https://wimlib.net/git/?p=wimlib;a=tree;f=tools/windeps/Makefile;

# Absolute path to this script, e.g. /home/user/bin/foo.sh
BASE_ABS_PATH=$(readlink -f "$0")
# Absolute path this script is in, thus /home/user/bin
BASE_DIR=$(dirname "$BASE_ABS_PATH")

# libxml2 source directory must be parameter
pushd $PWD
LIB_PREFIX=$(readlink -f "$HOME/wimlib-build")
LIBXML2_SRC_DIR=$(readlink -f "$1")
mkdir $LIB_PREFIX
cd $LIBXML2_SRC_DIR
./configure --enable-static --disable-shared \
    --prefix=$LIB_PREFIX CFLAGS=-Os \
    --with-minimum --without-lzma --with-tree --with-writer
make install
popd
