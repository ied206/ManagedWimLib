# Compile libwim-15.dll

To compile wimlib on Windows, you should use MSYS2.

## Required Tools

- [MSYS2](https://www.msys2.org/)
    - Install `base-devel`, `mingw-w64-x86_64-toolchain`, `mingw-w64-i686-toolchain` packages.
- [libmagic source](http://www.darwinsys.com/file/)
- [llvm-mingw](https://github.com/mstorsjo/llvm-mingw)
    - Binaries compiled from MSYS2 MinGW 32bit compiler depends on `libgcc_s_dw2-1.dl` and `libwinpthread-1.dll`. Compiling with clang avoids this problem.
    - MSYS2 does not provide ARM64 gcc toolchain, so instead use clang to cross-compile ARM64 binary.

## Manual

1. Open MSYS2 shell.
    | Arch  | MSYS2 shell       | Native or Cross? |
    |-------|-------------------|------------------|
    | x86   | MSYS2 MinGW 32bit | native compile   |
    | x64   | MSYS2 MinGW 64bit | native compile   |
    | arm64 | MSYS2 MinGW 64bit | cross compile    |
1. Extract the `libmagic` source code.
1. Apply patches to `libmagic` source if needed.
    - Refer to [patch README](patch\README.md) to when and why the patches are necessary.
1. Run `libmagic-msys2.sh`.
    - You are recommended to pass a path of `llvm-mingw` to compile x86/x64 binaries.
    - You must pass a path of `llvm-mingw` to compile ARM64 binaries.
    ```
    [Examples]
    x86: ./libmagic-msys2.sh -a i686 -t /c/llvm-mingw /d/build/native/file-5.40 
    x64: ./libmagic-msys2.sh -a x86_64 -t /c/llvm-mingw /d/build/native/file-5.40 
    aarch64: ./libmagic-msys2.sh -a aarch64 -t /c/llvm-mingw /d/build/native/file-5.40 
    ```
1. Gather binaries from `build-<arch>` directory.


wim.c: __declspec(dllexport) void 함수이름(구현)
wimlib.h: extern void 함수이름(선언);



https://stackoverflow.com/questions/12163406/mingw32-compliation-issue-when-static-linking-is-required-adol-c-links-colpack

https://github.com/llvm-mirror/clang/blob/master/test/Sema/dllexport.c