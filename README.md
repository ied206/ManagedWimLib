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

- .Net Framework 4.5.1
- .Net Standard 1.3 (.Net Framework 4.6+, .Net Core 1.0+)
- .Net Standard 2.0 (.Net Framework 4.6.1+, .Net Core 2.0+)

If you need .Net Framework 4.5 support, use [1.1.x version](https://www.nuget.org/packages/ManagedWimLib/1.1.2) instead.

### Supported OS platforms

| Platform | Architecture | Tested |
|----------|--------------|--------|
| Windows  | x86, x64     | Yes    |
| Linux    | x64, armhf   | Yes    |
|          | arm64        | No     |

#### Tested linux distributions

| Architecture | Distribution | Note |
|--------------|--------------|------|
| x64          | Ubuntu 18.04 |      |
| armhf        | Debian 9     | Emulated on QEMU's virt board |

### Supported wimlib version

- 1.13.0 (Included)

## Usage

See [USAGE.md](./USAGE.md).

## License

Licensed under LGPLv3.  
See [LICENSE](./LICENSE) for details.

Logo is licensed under CC BY 3.0 US.  
[disc](https://thenounproject.com/term/disc/772617) by Ralf Schmitzer from the Noun Project.
