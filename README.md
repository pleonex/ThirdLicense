# Third License [![MIT License](https://img.shields.io/badge/license-MIT-blue.svg?style=flat)](https://choosealicense.com/licenses/mit/)

> .NET tool to generate _third-party_ notice files from all the transitive
> dependencies of a .NET project.

## Usage

The program accepts the following arguments:

* **project**: the project file to analyze for third-parties.
* **output**: the output file path. By default in the current directory with name `THIRD-PARTY-NOTICES.TXT`.
* **endpoint**: the NuGet repository endpoint (v2 only). By default it uses the one from nuget.org.

## Build

Install [.NET Core SDK 3.1](https://dotnet.microsoft.com/download/dotnet-core/3.1)
and go to the _src_ folder to run `dotnet build`.
