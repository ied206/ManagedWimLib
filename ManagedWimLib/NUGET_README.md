# ManagedWimLib

Cross-platform [wimlib](https://wimlib.net) pinvoke library for .NET.

[wimlib](https://wimlib.net) is a library that handles Windows Imaging (WIM) archives, written by [Eric Biggers](https://github.com/ebiggers).

## Usage

Please refer to this [document](https://github.com/ied206/ManagedWimLib/blob/v2.4.1/USAGE.md).

## Tested wimlib Versions

- 1.13.6 (Included)

## Support

### Targeted .NET platforms

- .NET Core 3.1
- .NET Standard 2.0
- .NET Framework 4.6

### Discontinued target frameworks

| Platform             | Last Supported Version                                       |
|----------------------|--------------------------------------------------------------|
| .NET Standard 1.3    | [v1.1.2](https://www.nuget.org/packages/ManagedWimLib/1.1.2) |
| .NET Framework 4.5   | [v1.2.4](https://www.nuget.org/packages/ManagedWimLib/1.2.4) |
| .NET Framework 4.5.1 | [v2.4.0](https://www.nuget.org/packages/ManagedWimLib/2.4.0) |

### Supported OS platforms

| Platform | Architecture | Tested |
|----------|--------------|--------|
| Windows  | x86          | Yes    |
|          | x64          | Yes    |
|          | arm64        | Yes    |
| Linux    | x64          | Yes    |
|          | arm          | Yes    |
|          | arm64        | Yes    |
| macOS    | x64          | Yes    |
|          | arm64        | Yes    |

#### Tested linux distributions

| Architecture | Distribution | Note |
|--------------|--------------|------|
| x64          | Ubuntu 20.04 | CI testd |
| arm          | Debian 11    | Emulated on QEMU's virt board |
| arm64        | Debian 11    | Emulated on QEMU's virt board |

### Supported wimlib version

- 1.13.6 (Included)

## Usage

See [USAGE.md](./USAGE.md).
