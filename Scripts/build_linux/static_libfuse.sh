#!/bin/bash

# Adapted from https://wimlib.net/git/?p=wimlib;a=tree;f=tools/windeps/Makefile;

# Absolute path to this script, e.g. /home/user/bin/foo.sh
BASE_ABS_PATH=$(readlink -f "$0")
# Absolute path this script is in, thus /home/user/bin
BASE_DIR=$(dirname "$BASE_ABS_PATH")

# libxml2 source directory must be parameter
pushd $PWD
LIB_PREFIX=$HOME/wimlib-build
LIBFUSE_SRC_DIR=$(readlink -f "$1")
mkdir $LIB_PREFIX
cd $LIBFUSE_SRC_DIR
mkdir build
CFLAGS=-Os meson . build --strip --default-library shared --prefix=$LIB_PREFIX 
cd build
ninja -j 12
ninja install
popd
