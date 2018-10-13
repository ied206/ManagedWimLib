# Usage

## Initialization

ManagedWimLib requires binary of wimlib to work.  
Internally it is done by loading functions dynamically (using `LoadLibrary` and `GetProcAddress`).

 `Wim.GlobalInit()` should be explicitly called before using `ManagedWimLib`.

Put this snippet in your application's init code:

```cs
if (IntPtr.Size == 8) // This app is running on 64bit .Net Framework
    Wim.GlobalInit(Path.Combine("x64", "libwim-15.dll"));
else // This app is running on 32bit .Net Framework
    Wim.GlobalInit(Path.Combine("x86", "libwim-15.dll"));
```

**WARNING**: Architecture of `libwim-15.dll` must be matched with caller!

### Embedded precompiled binary

ManagedWimLib comes with `libwim-15.dll`, precompiled binaries of `wimlib 1.13.0-BETA5`.  
They will be copied into `$(OutDir)\x86\libwim-15.dll` and `$(OutDir)\x64\libwim-15.dll` automatically at build.

### Custom binary

To use custom `wimlib` binary instead, call `Wim.GlobalInit()` with path to custom `libwim-15.dll`.

**NOTE**: Create empty file named `ManagedWimLib.Precompiled.Exclude` in project directory to prevent copy of package-embedded `libwim-15.dll`.

## Cleanup

To unload `wimlib` explicitly, call `Wim.GlobalCleanup()`.
