# Compile libwim-15.dll for Windows ARM64

Use MSYS2 and llvm-mingw to compile ARM64 wimlib dll.

## Status

- `libwim-15.dll` is compilable with a bit of a patch.
- `ld.lld` fails to link `wimlib-imagex.exe`.

## Required Tools

- [MSYS2](https://www.msys2.org/)
    - Install `base-devel`, `mingw-w64-x86_64-toolchain`, `mingw-w64-i686-toolchain` packages.
- [llvm-mingw](https://github.com/mstorsjo/llvm-mingw)
    - MSYS2 does not provide ARM64 gcc toolchain, so instead use clang to cross-compile ARM64 binary.
    - Tested with llvm-mingw 20201020 with LLVM stable 11.0.0.

## Required Sources

- [libxml2](http://www.xmlsoft.org/downloads.html): Tested with 2.9.10.
- [wimlib](https://wimlib.net/downloads/index.html): Tested with 1.13.3.

## Build Manual

1. Extract the `libxml2` and `wimlib` source code.
1. Open MSYS2 MinGW 64bit shell.
1. Apply `patch/libxml2-llvm-linker.patch` to `libxml2` source.
1. Build `libxml2` by running `libxml2-msys2.sh`.
    - You must pass a path of `llvm-mingw` to compile ARM64 binaries.
    ```
    [Examples]
    aarch64: ./libxml2-msys2.sh.sh -a aarch64 -t /c/llvm-mingw /d/build/native/libxml2-
    ```
1. Edit `include/wimlib.h` of `wimlib` source.
    - Define `WIMLIBAPI` as `__declspec(dllexport)`.
    ```c
    #define WIMLIBAPI __declspec(dllexport)
    ```
    - Replace every `extern ` with `extern WIMLIBAPI `.
    ```c
    // AS-IS EXAMPLE
    extern void
    wimlib_free(WIMStruct *wim);
    // TO-BE EXAMPLE
    extern WIMLIBAPI void
    wimlib_free(WIMStruct *wim);
    ```
1. Build `wimlib` by running `wimlib-msys2.sh`.
    - You must pass a path of `llvm-mingw` to compile ARM64 binaries.
    ```
    [Examples]
    aarch64: ./wimlib-msys2.sh.sh -a aarch64 -t /c/llvm-mingw /d/build/native/wimlib-1.13.3
    ```
1. Copy `$LLVM_MINGW/aarch64-w64-mingw32/bin/libwinpthreads-1.dll` to `build-bin-<arch>` directory.
    - Strip `libwinpthreads-1.dll` to reduce its size.
1. Gather binaries from the `build-bin-<arch>` directory.

## Patches required for clang

### Why libxml2 have an issue with the LLVM linker?

`configure` identifies `ld.lld` as GNU ld and tries to use version script. LLVM linker does not support it, thus linking fails.

```
  CCLD     libxml2.la
lld: error: unknown argument: --version-script=./libxml2.syms
clang-11: error: linker command failed with exit code 1 (use -v to see invocation)
make[2]: *** [Makefile:1064: libxml2.la] Error 1
```

Fortunately, @jeremyd2019 found a solution: borrow `libxslt`'s linker detection script ([ref](https://github.com/msys2/CLANG-packages/issues/19)). 

### Why clang refuses to build vanilla wimlib?

Clang requires every function symbols have consistency on dllimport/dllexport-ness ([ref1](https://github.com/llvm-mirror/clang/blob/master/test/Sema/dllexport.c), [ref2](http://clang-developers.42468.n3.nabble.com/Latest-clang-shows-failure-in-redeclaration-with-dllimport-td4045316.html), while GCC and MSVC allows this behavior.

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

#### Unable to build wimlib-imagex.exe
- llvm-mingw fails to link `wimlib-imagex.exe`, but I couldn't know why.
    ```
    CCLD     libwim.la
    CCLD     wimlib-imagex.exe
    lld-link: error: undefined symbol: __declspec(dllimport) __winitenv
    >>> referenced by C:\Joveler\llvm-mingw\aarch64-w64-mingw32\lib\crt2u.o:(.refptr.__imp___winitenv)
    clang-11: error: linker command failed with exit code 1 (use -v to see invocation)
    make[1]: *** [Makefile:1318: wimlib-imagex.exe] Error 1
    ```
- libtool refuses to link `wimlib` with static `libxml2`.
    ```
    *** Warning: Trying to link with static lib archive <LIBXML2_STATIC_LIB>.
    *** I have the capability to make that library automatically link in when
    *** you link to this library.  But I can only do this if you have a
    *** shared version of the library, which you do not appear to have
    *** because the file extensions .$libext of this argument makes me believe
    *** that it is just a static archive that I should not use here.
    ```
