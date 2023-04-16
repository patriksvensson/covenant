# Covenant

A tool to generate SBOM (Software Bill of Material) from source code artifacts.

NOTE:
Covenant requires all projects to have been built, and all dependencies to have been restored to make an as accurate analysis as possible.

## Supported SBOM formats

* [CycloneDx](https://cyclonedx.org/)
* [SPDX](https://spdx.dev/)

## Supported sources

* .NET 5 / .NET 6
* .NET Core
* NPM

## Installation

Install by running the following command:

```bash
dotnet tool install covenant
```

## Configuration file

The configuration file is used to configure different aspects of Covenant.

```json
{
    "$schema": "https://raw.githubusercontent.com/patriksvensson/covenant/main/schema/0.14.json"
    
    // Used for arbitrary files to be included in the SBOM (optional)
    "files": [
        {
            "path": "./files/lol.txt",
            "license": "MIT"
        },
        {
            "path": "./**/foo.c"
        }
    ],
    
    // Used for compliance checks (optional)
    "licenses": { 
        "banned": [
            "MIT"
        ]
    }
}
```

## Generate Covenant SBOM

```
Usage:
  covenant generate [<PATH>] [options]

Arguments:
  <PATH>  A file or directory to use as input

Options:
  -o, --output <FILE>         The output path of the SBOM file
  -n, --name <NAME>           The SBOM name
  -v, --version <VERSION>     The SBOM version [default: 0.0.0]
  -m, --metadata <metadata>   Arbitrary metadata in the form 'key=value'
  -c, --configuration <FILE>  The Covenant configuration file to use
  --design-time-build         Performs a design time build for .NET projects [default: False]
  --no-dev-dependencies       Excludes dev dependencies for NPM projects [default: False]
  -?, -h, --help              Show help and usage information
```

To generate an Covenant SBOM from the current directory:

```bash
dotnet covenant generate 
```

To generate an Covenant SBOM from a specific directory:

```bash
dotnet covenant generate "C:\Source\Foo"
```

To generate an Covenant SBOM from a specific file:

```bash
dotnet covenant generate "C:\Source\Foo\Foo.sln"
```

## Convert Covenant SBOM to third party SBOM formats

```
Usage:
  covenant convert [command] [options]

Options:
  -?, -h, --help  Show help and usage information

Commands:
  cyclonedx <PATH>
  spdx <PATH>
```

### SPDX

```
Usage:
  covenant convert spdx <PATH> [options]

Arguments:
  <PATH>  The Covenant SBOM file to convert

Options:
  -o, --output <FILE>      The output path
  --namespace <namespace>  The SPDX namespace
  -?, -h, --help           Show help and usage information
```

```bash
dotnet covenant convert spdx "C:\Source\Foo\Foo.covenant.json"
```

### CycloneDX

```
Usage:
  covenant convert cyclonedx <PATH> [options]

Arguments:
  <PATH>  The Covenant SBOM file to convert

Options:
  -o, --output <FILE>  The output path
  -?, -h, --help       Show help and usage information
```

```bash
dotnet covenant convert cyclonedx "C:\Source\Foo\Foo.covenant.json"
```

## Creating reports

```
Usage:
  covenant report <PATH> [options]

Arguments:
  <PATH>  The Covenant SBOM file to create a HTML report for

Options:
  -o, --output <FILE>  The output path of the HTML report
  -?, -h, --help       Show help and usage information
```

```bash
dotnet covenant report "C:\Source\Foo\Foo.covenant.json"
```

## Checking compliance

```
Usage:
  covenant check <PATH> [options]

Arguments:
  <PATH>  The Covenant SBOM file to run compliance checks for

Options:
  -c, --configuration <FILE>  The Covenant configuration file to use
  -?, -h, --help              Show help and usage information
```

```bash
dotnet covenant check "C:\Source\Foo\Foo.covenant.json"
```

You can put a file called `covenant.config` next to the SPDX report,
or providing one via the `--config` parameter, to configure the 
compliance rules.

```json
{
    "licenses": {
        "banned": [
            "MIT"
        ]
    }
}
```

## Building

We're using [Cake](https://github.com/cake-build/cake) as a 
[dotnet tool](https://docs.microsoft.com/en-us/dotnet/core/tools/global-tools) 
for building. So make sure that you've restored Cake by running 
the following in the repository root:

```
> dotnet tool restore
```

After that, running the build is as easy as writing:

```
> dotnet cake
```