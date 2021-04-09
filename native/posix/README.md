# Compile libwim-15.dll for POSIX

## Required Tools

- nasm

## Required Sources

- [libxml2](http://www.xmlsoft.org/downloads.html): Tested with 2.9.10.
- [wimlib](https://wimlib.net/downloads/index.html): Tested with 1.13.3.
- System libfuse-dev package

## Build Manual

1. Build libxml2 with `static-libxml2.sh`.
2. Build wimlib with `static-wimlib.sh`.
