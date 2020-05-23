# Third License [![MIT License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://choosealicense.com/licenses/mit/)

> .NET tool to generate _third-party_ notice files from all the transitive
> dependencies of a .NET project.

## Installation

Get the latest release from the [GitHub release tab](https://github.com/pleonex/ThirdLicense/releases) or install it as a global tool from [nuget.org](https://www.nuget.org/packages/ThirdLicense).

### Run as a program

If you download the zip file, you can run the tool as a .NET program as follow:
`dotnet ThirdLicense.dll`.

### Install as a .NET global tool

You can install it as a .NET global tool and it will be added to your `PATH`
environment variable. Install the latest stable release from [nuget.org](https://www.nuget.org/packages/ThirdLicense)
with:

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

* **project**: the project file to analyze for third-parties.
* **output**: the output file path. By default in the current directory with name `THIRD-PARTY-NOTICES.TXT`.
* **endpoint**: the NuGet repository endpoint (v2 only). By default it uses the one from nuget.org.

## Build

Install [.NET Core SDK 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)
and go to the _src_ folder to run `dotnet build`.
