# Third License ![.NET Core](https://github.com/pleonex/ThirdLicense/workflows/.NET%20Core/badge.svg?branch=master) [![MIT License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://choosealicense.com/licenses/mit/)

> .NET tool to generate _third-party_ notice files from all the transitive
> dependencies of a .NET project.

## Installation

Get the latest release from the
[GitHub release tab](https://github.com/pleonex/ThirdLicense/releases) or
install it as a global tool from
[nuget.org](https://www.nuget.org/packages/ThirdLicense).

### Run as a program

If you download the zip file, you can run the tool as a .NET program as follow:
`dotnet ThirdLicense.dll`.

### Install as a .NET global tool

You can install it as a .NET global tool and it will be added to your `PATH`
environment variable. Install the latest stable release from
[nuget.org](https://www.nuget.org/packages/ThirdLicense) with:

```text
dotnet tool install --global ThirdLicense
```

or from a local downloaded NuGet package (.nupkg):

```text
dotnet tool install --global --add-source <folder_with_nupkg> ThirdLicense
```

Then, you can run it just by the command name: `thirdlicense`

## Usage

To run the tool make sure you have installed
[.NET Core SDK 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1).

The program accepts the following arguments:

- **project**: the project file to analyze for third-parties.
- **output**: the output file path. By default in the current directory with
  name `THIRD-PARTY-NOTICES.TXT`.
- **endpoint**: optional extra NuGet repository endpoint to look for packages.
  It will also load the enabled sources, including from local `nuget.config`
  files.

Example of output file:

```text
License notice for NuGet.Protocol (v5.6.0+636570e68732c1f718ede9ca07802d7b1cc69aa0)
------------------------------------

https://github.com/NuGet/NuGet.Client at 636570e68732c1f718ede9ca07802d7b1cc69aa0

https://aka.ms/nugetprj

Copyright © Microsoft Corporation. All rights reserved.

Licensed under Apache-2.0

Available at https://licenses.nuget.org/Apache-2.0


License notice for System.CommandLine (v2.0.0-beta1.20253.1)
------------------------------------
[...]
```

## Build

Install
[.NET Core SDK 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1) and
go to the _src_ folder to run `dotnet build`.

To create a runtime application run `dotnet publish -c Release` and to prepare
the NuGet run `dotnet pack -c Release`.
