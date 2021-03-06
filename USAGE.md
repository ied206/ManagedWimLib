# Usage

## Initialization

ManagedWimLib requires the explicit loading of a wimlib library.

You must call `Wim.GlobalInit()` before using `ManagedWimLib`. Please put this code snippet in your application init code:

#### For .NET Framework 4.5.1+

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
        case Architecture.Arm:
            arch = "armhf";
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

#### For .NET Standard 2.0+:

```cs
public static void InitNativeLibrary()
{
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

    string libPath = null;
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        libPath = Path.Combine(libDir, "libwim-15.dll");
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        libPath = Path.Combine(libDir, "libwim.so");
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        libPath = Path.Combine(libDir, "libwim.dylib");

    if (libPath == null)
        throw new PlatformNotSupportedException($"Unable to find native library.");
    if (!File.Exists(libPath))
        throw new PlatformNotSupportedException($"Unable to find native library [{libPath}].");

    Wim.GlobalInit(libPath, InitFlags.None);
}
```

To configure behaviors of wimlib, pass `InitFlags` to `Wim.GlobalInit()`.

**WARNING**: Caller process and callee library must have the same architecture!

### Embedded binary

ManagedWimLib comes with sets of binaries of `wimlib 1.13.4`. They will be copied into the build directory at build time.

#### For .NET Framework 4.5.1+

| Platform         | Binary                        | License |
|------------------|-------------------------------|---------|
| Windows x86      | `$(OutDir)\x86\libwim-15.dll` | LGPLv3  |
| Windows x64      | `$(OutDir)\x64\libwim-15.dll` | LGPLv3  |

- Create an empty file named `ManagedWimLib.Precompiled.Exclude` in the project directory to prevent a copy of the package-embedded binary.

#### For .NET Standard 2.0+

| Platform          | Binary                                              | License              |
|-------------------|-----------------------------------------------------|----------------------|
| Windows x86       | `$(OutDir)\runtimes\win-x86\native\libwim-15.dll`   | LGPLv3               |
| Windows x64       | `$(OutDir)\runtimes\win-x64\native\libwim-15.dll`   | LGPLv3               |
| Windows arm64     | `$(OutDir)\runtimes\win-arm64\native\libwim-15.dll` | LGPLv3               |
| Ubuntu 18.04 x64  | `$(OutDir)\runtimes\linux-x64\native\libwim.so`     | LGPLv3 (w/o NTFS-3G) |
| Debian 10 armhf   | `$(OutDir)\runtimes\linux-arm\native\libwim.so`     | LGPLv3 (w/o NTFS-3G) |
| Debian 10 arm64   | `$(OutDir)\runtimes\linux-arm64\native\libwim.so`   | LGPLv3 (w/o NTFS-3G) |
| macOS 10.15 x64   | `$(OutDir)\runtimes\osx-x64\native\libwim.dylib`    | LGPLv3 (w/o NTFS-3G) |

- Linux binaries are not portable. Included binaires may not work on your distribution.
    - On Linux, wimlib depends on system-installed `libfuse`.
- If you call `Wim.GlobalInit()` without `libPath` parameter on Linux or macOS, `ManagedWimLib` will search for system-installed wimlib.
- POSIX binaries were compiled without NTFS-3G support to make them as LGPLv3-licensed.
    - If you want NTFS-3G functionality, load the system-installed library and make sure your program is compatible with **GPLv3**.

#### Build Command

| Platform         | Binary Source                                                                         | Dependency      |
|------------------|---------------------------------------------------------------------------------------|-----------------|
| Windows x86      | [Official Release](https://wimlib.net/downloads/wimlib-1.13.3-windows-i686-bin.zip)   | -               |
| Windows x64      | [Official Release](https://wimlib.net/downloads/wimlib-1.13.3-windows-x86_64-bin.zip) | -               |
| Windows arm64    | Compile with MSYS2 and llvm-mingw                                                     | -               |
| Ubuntu 18.04 x64 | Compile with libxml2 dependency                                                       | libfuse         |
| Debian 10 armhf  | Compile with libxml2 dependency                                                       | libfuse         |
| Debian 10 arm64  | Compile with libxml2 dependency                                                       | libfuse         |
| macOS 10.15      | Compile with libxml2 dependency                                                       | -               |

### Custom binary

To use custom wimlib binary instead, call `Wim.GlobalInit()` with a path to the custom binary.

### Cleanup

To unload the wimlib library explicitly, call `Wim.GlobalCleanup()`.

## API

ManagedWimLib provides sets of APIs matched to its original. Most of the use cases follow this flow:

1. Create Wim instance with `Wim.OpenWim()`
2. Do your job by calling APIs of your interest.
3. Cleanup Wim instance with the Disposable pattern.

[ManagedWimLib.Tests](./ManagedWimLib.Tests) provides a lot of examples of how to use ManagedWimLib.
