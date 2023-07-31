# Compile libwim-15.dll for Windows ARM64

Use MSYS2 and llvm-mingw to compile ARM64 wimlib dll.

## Status

- `libwim-15.dll` is compilable with both MinGW and LLVM.

## Required Tools

- [MSYS2](https://www.msys2.org/)
    - Install `base-devel`, `mingw-w64-x86_64-toolchain`, `mingw-w64-i686-toolchain` packages.
- [llvm-mingw](https://github.com/mstorsjo/llvm-mingw)
    - MSYS2 does not provide ARM64 gcc toolchain, so instead use clang to cross-compile ARM64 binary.
    - Tested with llvm-mingw 20230614 with LLVM stable 16.0.6.

## Required Sources

- [wimlib](https://wimlib.net/downloads/index.html): Tested with 1.14,1.

## Build Manual

1. Extract the `wimlib` source code.
1. Open MSYS2 MinGW 64bit shell.
1. Build `wimlib` by running `wimlib-msys2.sh`.
    - You must pass a path of `llvm-mingw` to compile ARM64 binaries.
    ```
    [Examples]
    aarch64: ./wimlib-msys2.sh.sh -a aarch64 -t /c/llvm-mingw /d/build/native/wimlib-1.13.3
    ```
1. `$LLVM_MINGW/aarch64-w64-mingw32/bin/libwinpthreads-1.dll` is may required. If so, copy it to `build-bin-<arch>` directory.
    - Strip `libwinpthreads-1.dll` to reduce its size.
1. Gather binaries from the `build-bin-<arch>` directory.

## Patches required to use clang on wimlib v1.13.x

### Why libxml2 have an issue with the LLVM linker?

`configure` identifies `ld.lld` as GNU ld and tries to use version script. LLVM linker does not support it, thus linking fails.

```
  CCLD     libxml2.la
lld: error: unknown argument: --version-script=./libxml2.syms
clang-11: error: linker command failed with exit code 1 (use -v to see invocation)
make[2]: *** [Makefile:1064: libxml2.la] Error 1
```

Fortunately, @jeremyd2019 found a solution: borrow `libxslt`'s linker detection script ([ref](https://github.com/msys2/CLANG-packages/issues/19)). 

### Why clang refuses to build vanilla wimlib v1.13.x?

Clang requires every function symbols have consistency on dllimport/dllexport-ness ([ref1](https://github.com/llvm-mirror/clang/blob/master/test/Sema/dllexport.c), [ref2](http://clang-developers.42468.n3.nabble.com/Latest-clang-shows-failure-in-redeclaration-with-dllimport-td4045316.html)), while GCC and MSVC allows this behavior.

For example, GCC allows this code, while CLANG throws an error.

```c
#define WIMLIBAPI __declspec(dllexport)
wimlib.h: extern void wimlib_free(WIMStruct *wim);
wim.c: WIMLIBAPI void wimlib_free(WIMStruct *wim);
```

```
[clang build error message]
src/wim.c:907:1: error: redeclaration of 'wimlib_free' cannot add 'dllexport' attribute
wimlib_free(WIMStruct *wim)
^
./include\wimlib.h:3182:1: note: previous declaration is here
wimlib_free(WIMStruct *wim);
^
1 error generated.
```

## Unsolvable issues

### Unable to build wimlib-imagex.exe

- llvm-mingw sometimes fails to link `wimlib-imagex.exe`.
    - It is suspected LLVM ld.lld has some issues on cross compiling.

### Unable to link static libxml2

- libtool refuses to link `wimlib` with static `libxml2`.
    ```
    *** Warning: Trying to link with static lib archive <LIBXML2_STATIC_LIB>.
    *** I have the capability to make that library automatically link in when
    *** you link to this library.  But I can only do this if you have a
    *** shared version of the library, which you do not appear to have
    *** because the file extensions .$libext of this argument makes me believe
    *** that it is just a static archive that I should not use here.
    ```
