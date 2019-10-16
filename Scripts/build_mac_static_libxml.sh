#!/bin/bash

# [brew install libxml2 generated messages]
#If you need to have libxml2 first in your PATH run:
#  echo 'export PATH="/usr/local/opt/libxml2/bin:$PATH"' >> ~/.zshrc
#
#For compilers to find libxml2 you may need to set:
#  export LDFLAGS="-L/usr/local/opt/libxml2/lib"
#  export CPPFLAGS="-I/usr/local/opt/libxml2/include"

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
./configure --enable-static --disable-shared --prefix=$LIB_PREFIX CFLAGS=-Os --with-minimum --without-lzma --with-tree --with-writer
make install
# rm -f $LIB_PREFIX/lib/libxml2.la;
make clean
popd
