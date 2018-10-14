# Usage

## Initialization

ManagedWimLib requires explicit loading of a wimlib library.

You must call  `Wim.GlobalInit()` before using `ManagedWimLib`.

Put this snippet in your application's init code:

```cs
public static void InitNativeLibrary()
{
    string libPath = null;
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        switch (RuntimeInformation.ProcessArchitecture)
        {
            case Architecture.X64:
                libPath = Path.Combine("x64", "libwim-15.dll");
                break;
            case Architecture.X86:
                libPath = Path.Combine("x86", "libwim-15.dll");
                break;
        }
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        switch (RuntimeInformation.ProcessArchitecture)
        {
            case Architecture.X64:
                libPath = Path.Combine("x64", "libwim.so");
                break;
        }
    }

    if (libPath == null)
        throw new PlatformNotSupportedException();

    Wim.GlobalInit(libPath);
}
```

**WARNING**: Caller process and callee library must have the same architecture!

### Embedded binary

ManagedWimLib comes with sets of binaries of `wimlib 1.13.0-BETA5`.  
They will be copied into the build directory at build time.

| Platform         | Binary                        |
|------------------|-------------------------------|
| Windows x86      | `$(OutDir)\x86\libwim-15.dll` |
| Windows x64      | `$(OutDir)\x64\libwim-15.dll` |
| Ubuntu 18.04 x64 | `$(OutDir)\x64\libwim.so`     |

### Custom binary

To use custom wimlib binary instead, call `Wim.GlobalInit()` with a path to the custom binary.

#### NOTES

- Create an empty file named `ManagedWimLib.Precompiled.Exclude` in project directory to prevent copy of package-embedded binary.

### Cleanup

To unload wimlib library explicitly, call `Wim.GlobalCleanup()`.

## API

ManagedWimLib provides sets of APIs match to its original.

Most of the use cases follow this flow.

1. Create Wim instance with `Wim.OpenWim()`
2. Do your job by calling API of your interest.
3. Cleanup Wim instance with the Disposable pattern.

[ManagedWimLib.Tests](./ManagedWimLib.Tests) provide a lot of examples of how to use ManagedWimLib.