# Usage

## Initialization

ManagedWimLib requires the explicit loading of a wimlib library.

You must call `Wim.GlobalInit()` before using `ManagedWimLib`. 

To configure behaviors of wimlib, pass `InitFlags` to `Wim.GlobalInit()`.

### Init Code Snippet

Please put this code snippet in your application init code:

**WARNING**: The caller process and callee library must have the same architecture!

#### On .NET/.NET Core

```cs
public static void InitNativeLibrary()
{
    string libBaseDir = AppDomain.CurrentDomain.BaseDirectory;
    string libDir = "runtimes";
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        libDir = Path.Combine(libDir, "win-");
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        libDir = Path.Combine(libDir, "linux-");
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        libDir = Path.Combine(libDir, "osx-");

    switch (RuntimeInformation.ProcessArchitecture)
    {
        case Architecture.X86:
            libDir += "x86";
            break;
        case Architecture.X64:
            libDir += "x64";
            break;
        case Architecture.Arm:
            libDir += "arm";
            break;
        case Architecture.Arm64:
            libDir += "arm64";
            break;
    }
    libDir = Path.Combine(libDir, "native");

    // Some platforms require a native library custom path to be an absolute path.
    string libPath = null;
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        libPath = Path.Combine(libBaseDir, libDir, "libwim-15.dll");
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        libPath = Path.Combine(libBaseDir, libDir, "libwim.so");
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        libPath = Path.Combine(libBaseDir, libDir, "libwim.dylib");

    if (libPath == null)
        throw new PlatformNotSupportedException($"Unable to find native library.");
    if (!File.Exists(libPath))
        throw new PlatformNotSupportedException($"Unable to find native library [{libPath}].");

    Wim.GlobalInit(libPath, InitFlags.None);
}
```

#### On .NET Framework

```cs
public static void InitNativeLibrary()
{
    string arch = null;
    switch (RuntimeInformation.ProcessArchitecture)
    {
        case Architecture.X86:
            arch = "x86";
            break;
        case Architecture.X64:
            arch = "x64";
            break;
        case Architecture.Arm64:
            arch = "arm64";
            break;
    }
    string libPath = Path.Combine(arch, "libwim-15.dll");

    if (!File.Exists(libPath))
        throw new PlatformNotSupportedException($"Unable to find native library [{libPath}].");

    Wim.GlobalInit(libPath, InitFlags.None);
}
```

### Embedded binary

ManagedWimLib comes with sets of binaries of `wimlib 1.14.2`. They will be copied into the build directory at build time.

#### On .NET Framework

| Platform         | Binary                          | License | C Runtime     |
|------------------|---------------------------------|---------|---------------|
| Windows x86      | `$(OutDir)\x86\libwim-15.dll`   | LGPLv3  | Universal CRT |
| Windows x64      | `$(OutDir)\x64\libwim-15.dll`   | LGPLv3  | Universal CRT |
| Windows arm64    | `$(OutDir)\arm64\libwim-15.dll` | LGPLv3  | Universal CRT |

- Create an empty file named `ManagedWimLib.Precompiled.Exclude` in the project directory to prevent a copy of the package-embedded binary.

#### On .NET/.NET Core & .NET Standard

| Platform             | Binary                                              | License              | C Runtime     |
|----------------------|-----------------------------------------------------|----------------------|---------------|
| Windows x86          | `$(OutDir)\runtimes\win-x86\native\libwim-15.dll`   | LGPLv3               | Universal CRT |
| Windows x64          | `$(OutDir)\runtimes\win-x64\native\libwim-15.dll`   | LGPLv3               | Universal CRT |
| Windows arm64        | `$(OutDir)\runtimes\win-arm64\native\libwim-15.dll` | LGPLv3               | Universal CRT |
| Ubuntu 20.04 x64     | `$(OutDir)\runtimes\linux-x64\native\libwim.so`     | LGPLv3 (w/o NTFS-3G) | glibc         | 
| Debian 11 armhf      | `$(OutDir)\runtimes\linux-arm\native\libwim.so`     | LGPLv3 (w/o NTFS-3G) | glibc         | 
| Debian 11 arm64      | `$(OutDir)\runtimes\linux-arm64\native\libwim.so`   | LGPLv3 (w/o NTFS-3G) | glibc         |
| macOS Big Sur x64    | `$(OutDir)\runtimes\osx-x64\native\libwim.dylib`    | LGPLv3 (w/o NTFS-3G) | libSystem     |
| macOS Ventura arm64  | `$(OutDir)\runtimes\osx-arm64\native\libwim.dylib`  | LGPLv3 (w/o NTFS-3G) | libSystem     |

- Linux binaries are not portable by nature. Included binaries may not work on your distribution.
    - On Linux, wimlib depends on system-installed `libfuse3-3`.
- If you call `Wim.GlobalInit()` without the `libPath` parameter on Linux or macOS, `ManagedWimLib` will search for system-installed wimlib.
- Linux binaries were compiled without NTFS-3G support to make them LGPLv3-licensed.
    - If you want NTFS-3G functionality, load the system-installed library and make sure your program is compatible with **GPLv3**.

#### Build Command

| Platform             | Binary Source                      | Dependency      |
|----------------------|------------------------------------|-----------------|
| Windows x86          | Compiled with MSYS2 and llvm-mingw | -               |
| Windows x64          | Compiled with MSYS2 and llvm-mingw | -               |
| Windows arm64        | Compiled with MSYS2 and llvm-mingw | -               |
| Ubuntu 20.04 x64     | Compiled without NTFS-3G           | libfuse3-3      |
| Debian 12 armhf      | Compiled without NTFS-3G           | libfuse3-3      |
| Debian 12 arm64      | Compiled without NTFS-3G           | libfuse3-3      |
| macOS Big Sur x64    | Compiled without fuse3             | -               |
| macOS Ventura arm64  | Compiled without fuse3             | -               |

### Custom binary

To use the custom wimlib binary instead, call `Wim.GlobalInit()` with a path to the custom binary.

### Cleanup

To unload the wimlib library explicitly, call `Wim.GlobalCleanup()`.

## API

ManagedWimLib provides a set of APIs matched to its original. Most of the use cases follow this flow:

1. Create Wim instance with `Wim.OpenWim()`
2. Do your job by calling APIs of your interest.
3. Clean up Wim instance with the Disposable pattern.

[ManagedWimLib.Tests](./ManagedWimLib.Tests) provides a lot of examples of how to use ManagedWimLib.
