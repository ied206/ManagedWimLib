# ChangeLog

## v2.x

### v2.1.0

Released on 2021-04-10

- Update inlcuded wimlib to 1.13.3.
    - Better support for WIM referencing, as 1.13.3 fixs a critical bug which crashes wimlib on debug mode.
- Official support for Windows ARM64.

### v2.0.1

Released on 2020-06-07

- Fixed `ExtractFlags.NopAttributes` typo by renaming into `ExtractFlags.NoAttributes`.

### v2.0.0

Released on 2020-06-07

- Updated included wimlib to 1.13.2.
- Native libraries are now placed following [NuGet convention-based working directory](https://docs.microsoft.com/en-US/nuget/create-packages/creating-a-package#create-the-nuspec-file) on .NET Standard build.
- Added `Compressor`, `Decompressor` classes.
- Redesigned public APIs.
    - Applied consistent standard naming convention.
        - Renamed `ProgressInfo_*` structs into `*Progress` classes.
        - Renamed enums to PascalCase style from C_CONST style.
        - Renamed `ChangeFlags.*Flag` into `ChangeFlags.*`.
        - Renamed `*.Default` enum into `*.None`.
    - Renamed `ProgressInfo_*` structs into `*Progress` classes.
    - Rewrote progress callback strcuts into classes to improve callback performance.
    - Rewrote iterate functions and their callbacks.
        - Renamed `IterateFlags` into `IterateDirTreeFlags`.
        - Reserve `IterateLookupTableFlags` to used in `Wim.IterateLookupTable()`.
        - `IterateDirTreeCallback` now returns int instead of `CallbackStatus`. A callback must return `Wim.IterateCallbackSuccess` on success, or an `ErrorCode` on error.
    - Added `InitFlags` and overloads of `GlobalInit()`.
    - Replaced `ManagedWimLib.FileAttribute` with standard [System.IO.FileAttributes](https://docs.microsoft.com/en-US/dotnet/api/system.io.fileattributes).
    - Removed `Wim.PathSeparator`, using standard [Path.DirectorySeparatorChar](https://docs.microsoft.com/en-us/dotnet/api/system.io.path.directoryseparatorchar) is recommended.
    - Rewrote version functions into properties.

## v1.x

### v1.4.3

Released on 2019-11-01

- Improved RHEL/CentOS compatibility.

### v1.4.2

Released on 2019-10-25

- Fixed crashing of `Wim.GlobalCleanup()`.

### v1.4.1

Released on 2019-10-21

- Fixed `Wim.GetErrors()`, `Wim.GetLastError()` regression.
- Fixed `Wim.RootPath`, `Wim.PathSeparator` regression.
- The state of error printing is now readable from `Wim.ErrorPrintState`.

### v1.4.0

Released on 2019-10-20

- Supports the macOS platform.
- Applied improved native library loader, [Joveler.DynLoader](https://github.com/ied206/Joveler.DynLoader).

### v1.2.4

Released on 2018-12-05

- Updated included wimlib to 1.13.0.

### v1.2.2, v1.2.3

Released on 2018-10-30

- Supports ARM, ARM64 on Linux.

### v1.2.1

Released on 2018-10-19

- Supports .NET Framework 4.5.1.

### v1.2.0

Released on 2018-10-16

- Supports .NET Standard 1.3.
- Supports the Linux platform.
- Added `Wim.MountImage()`, `Wim.UnmountImage()` APIs.

### v1.1.2

Released on 2018-10-15

- Fixed stack corruption of `Wim.UpdateImage()` API.

### v1.1.1

Released on 2018-10-13

- Fixed `Wim.GetErrorString()` to report correct error message.

### v1.1.0

Released on 2018-09-06

- Updated included wimlib to 1.13.0-BETA5.

### v1.0.0

Released on 2018-05-23

- Included wimlib 1.13.0-BETA2.
- Initial release.
