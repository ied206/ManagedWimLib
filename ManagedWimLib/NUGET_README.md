# ManagedWimLib

<div style="text-align: left">
    <img src="./Image/Logo.svg" height="128">
</div>

Cross-platform [wimlib](https://wimlib.net) pinvoke library for .NET.

[wimlib](https://wimlib.net) is a library that handles Windows Imaging (WIM) archives, written by [Eric Biggers](https://github.com/ebiggers).

## Support

### Targeted .NET platforms

- .NET Core 3.1
- .NET Standard 2.0
- .NET Framework 4.6

#### Discontinued frameworks

| Platform | Last Supported Version |
|----------|------------------------|
| .NET Standard 1.3 | [v1.1.2](https://www.nuget.org/packages/ManagedWimLib/1.1.2) |
| .NET Framework 4.5 | [v1.2.4](https://www.nuget.org/packages/ManagedWimLib/1.2.4) |
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
| x64          | Ubuntu 20.04 | Tested on AppVeyor CI         |
| arm          | Debian 11    | Emulated on QEMU's virt board |
| arm64        | Debian 11    | Emulated on QEMU's virt board |

### Supported wimlib version

- 1.13.6 (Included)

## Usage

See [USAGE.md](./USAGE.md).

## Changelog

See [CHANGELOG.md](./CHANGELOG.md).

## License

- `ManagedWimLib` is licensed under [LGPLv3](./LICENSE).
- The logo, [disc](https://thenounproject.com/term/disc/772617) by Ralf Schmitzer from the Noun Project, is licensed under CC BY 3.0 US.
