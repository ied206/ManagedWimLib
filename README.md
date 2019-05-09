# ManagedWimLib

<div style="text-align: left">
    <img src="./Image/Logo.svg" height="128">
</div>

C# pinvoke library for native [wimlib](https://wimlib.net).

wimlib is a library that handles Windows Imaging (WIM) archives, written by Eric Biggers.

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
- .Net Standard 2.0 (.Net Framework 4.6.1+, .Net Core 2.0+)

If you need .Net Framework 4.5 support, use [1.1.x version](https://www.nuget.org/packages/ManagedWimLib/1.1.2) instead.
If you need .Net Standard 1.3 support, use [1.2.x version](https://www.nuget.org/packages/ManagedWimLib/1.2.4) instead.

### Supported OS platforms

| Platform | Architecture | Tested |
|----------|--------------|--------|
| Windows  | x86          | Yes    |
|          | x64          | Yes    |
| Linux    | x64          | Yes    |
|          | armhf        | Yes    |
|          | arm64        | Yes    |

**Note:** I want to support macOS, but I do not have any macOS device. Please contribute to macOS support!

#### Tested linux distributions

| Architecture | Distribution | Note |
|--------------|--------------|------|
| x64          | Ubuntu 18.04 | Tested on AppVeyor CI         |
| armhf        | Debian 9     | Emulated on QEMU's virt board |
| arm64        | Debian 9     | Emulated on QEMU's virt board |

### Supported wimlib version

- 1.13.0
- 1.13.1 (Included)

## Usage

See [USAGE.md](./USAGE.md).

## License

Licensed under LGPLv3.  
See [LICENSE](./LICENSE) for details.

The logo is licensed under CC BY 3.0 US.  
[disc](https://thenounproject.com/term/disc/772617) by Ralf Schmitzer from the Noun Project.
