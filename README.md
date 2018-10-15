# ManagedWimLib

<div style="text-align: left">
    <img src="./Image/Logo.svg" height="128">
</div>

C# pinvoke library for native [wimlib](https://wimlib.net).

wimlib is a library handles Windows Imaging (WIM) archives, written by Eric Biggers.

| Branch    | Build Status   |
|-----------|----------------|
| Master    | [![CI Master Branch Build Status](https://ci.appveyor.com/api/projects/status/wtb8ong8c112f4ug/branch/master?svg=true)](https://ci.appveyor.com/project/ied206/managedwimlib/branch/master) |
| Develop   | [![CI Develop Branch Build Status](https://ci.appveyor.com/api/projects/status/wtb8ong8c112f4ug/branch/develop?svg=true)](https://ci.appveyor.com/project/ied206/managedwimlib/branch/develop) |

## Install

ManagedWimLib can be installed via [nuget](https://www.nuget.org/packages/ManagedWimLib).

[![NuGet](https://buildstats.info/nuget/ManagedWimLib)](https://www.nuget.org/packages/ManagedWimLib)

## Support

### Targeted .Net platforms

- .Net Standard 1.3 (.Net Framework 4.6+, .Net Core 1.0+)
- .Net Standard 2.0 (.Net Framework 4.6.1+, .Net Core 2.0+)

If you need .Net Framework 4.5 support, use Windows only [1.1.x branch](https://www.nuget.org/packages/ManagedWimLib/1.1.2).

### Supported OS platforms

- Windows x86, x64
- Linux x64

### Supported wimlib version

- 1.13.0-BETA5 (Included)

## Usage

See [USAGE.md](./USAGE.md).

## License

Licensed under LGPLv3.
See [LICENSE](./LICENSE) for details.
