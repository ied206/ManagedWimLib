# ManagedWimLib

<div style="text-align: left">
    <img src="./Image/Logo.svg" height="128">
</div>

Cross-platform [wimlib](https://wimlib.net) pinvoke library for .NET.

[wimlib](https://wimlib.net) is a library that handles Windows Imaging (WIM) archives, written by Eric Biggers.

| CI Server       | Branch  | Build Status   |
|-----------------|---------|----------------|
| AppVeyor        | Master  | [![CI Master Branch Build Status](https://ci.appveyor.com/api/projects/status/wtb8ong8c112f4ug/branch/master?svg=true)](https://ci.appveyor.com/project/ied206/managedwimlib/branch/master) |
| AppVeyor        | Develop | [![CI Develop Branch Build Status](https://ci.appveyor.com/api/projects/status/wtb8ong8c112f4ug/branch/develop?svg=true)](https://ci.appveyor.com/project/ied206/managedwimlib/branch/develop) |
| Azure Pipelines | Master  | [![Azure Pipelines CI Master Branch Build Status](https://ied206.visualstudio.com/ManagedWimLib/_apis/build/status/ied206.ManagedWimLib?branchName=master)](https://dev.azure.com/ied206/ManagedWimLib/_build) |
| Azure Pipelines | Develop | [![Azure Pipelines CI Develop Branch Build Status](https://ied206.visualstudio.com/ManagedWimLib/_apis/build/status/ied206.ManagedWimLib?branchName=develop)](https://dev.azure.com/ied206/ManagedWimLib/_build) |

## Install

ManagedWimLib can be installed via [nuget](https://www.nuget.org/packages/ManagedWimLib).

[![NuGet](https://buildstats.info/nuget/ManagedWimLib)](https://www.nuget.org/packages/ManagedWimLib)

## Support

### Targeted .NET platforms

- .NET Standard 2.1 (.NET Core 3.0+)
- .NET Standard 2.0 (.NET Framework 4.6.1+, .NET Core 2.0+)
- .NET Framework 4.5.1

If you need .NET Standard 1.3 support, use [1.2.x version](https://www.nuget.org/packages/ManagedWimLib/1.2.4) instead.<br>
If you need .NET Framework 4.5 support, use [1.1.x version](https://www.nuget.org/packages/ManagedWimLib/1.1.2) instead.

### Supported OS platforms

| Platform | Architecture | Tested |
|----------|--------------|--------|
| Windows  | x86          | Yes    |
|          | x64          | Yes    |
| Linux    | x64          | Yes    |
|          | armhf        | Yes    |
|          | arm64        | Yes    |
| macOS    | x64          | Yes    |

#### Tested linux distributions

| Architecture | Distribution | Note |
|--------------|--------------|------|
| x64          | Ubuntu 18.04 | Tested on AppVeyor CI         |
| armhf        | Debian 10    | Emulated on QEMU's virt board |
| arm64        | Debian 10    | Emulated on QEMU's virt board |

### Supported wimlib version

- 1.13.0
- 1.13.1
- 1.13.2 (Included)

## Usage

See [USAGE.md](./USAGE.md).

## Changelog

See [CHANGELOG.md](./CHANGELOG.md).

## License

- `ManagedWimLib` is licensed under [LGPLv3](./LICENSE).
- The logo, [disc](https://thenounproject.com/term/disc/772617) by Ralf Schmitzer from the Noun Project, is licensed under CC BY 3.0 US.
