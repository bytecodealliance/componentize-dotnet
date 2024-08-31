# Submitting bugs and fixes
Open an issue detailing the issue you've encountered or feature you would like to see.

Bug fixes and new features must be submitted using a pull request and pass CI to be included in the project.

## Building

Requires [.NET 8+](https://dotnet.microsoft.com/en-us/download)

```
## needed to avoid errors with multiple projects calling and downloading the sdks at same time (https://github.com/bytecodealliance/componentize-dotnet/issues/8)
dotnet msbuild src/WitBindgen/build/ByteCodeAlliance.Componentize.DotNet.WitBindgen.targets /t:PrepareWasmSdks
dotnet build
```

## Testing

Run the tests:

```
dotnet test
```

>  tip: If you've already built the project you can speed up the tests by running `dotnet test --no-build`

## Build Wasm tools locally

> requires [rust](https://www.rust-lang.org/tools/install)

The project is configured by default to pull tools such as [wit-bindgen](https://github.com/bytecodealliance/wit-bindgen) from their releases.  It is possible to use custom builds of these tools via submodules:

```
## get submodules
git submodule update --int

## get latest code from configured branch
git submodule update --recursive --remote

## optional, change the branch for the project
cd modules/<project>
git checkout <branch>
```

Modify the [WasmComponentSdk.csproj](./src/WasmComponent.Sdk/WasmComponent.Sdk.csproj) to enable building from source:

```
<BuildWasmToolsLocally>true</BuildWasmToolsLocally>
```

Modify the [WitBindgen.csproj](./src/WitBindgen/WitBindgen.csproj) to enable building from source:

```
<BuildWitBindgenLocally>true</BuildWitBindgenLocally>
```

And then follow the [project build steps](#building).

### Debugging

Create a msbuild debug log: 

```
dotnet build /bl
```

View the log with https://www.msbuildlog.com/.

Learn more at [trouble shooting techniques](https://learn.microsoft.com/en-us/visualstudio/ide/msbuild-logs?view=vs-2022) for msbuild.

## Getting help

> If you have any questions please drop a note in the [c# zulip chat](https://bytecodealliance.zulipchat.com/#narrow/stream/407028-C.23.2F.2Enet-collaboration).

This project uses MSbuild and .NET Project SDKS.  Learn more about this tooling in:

- [MSbuild docs](https://learn.microsoft.com/en-us/visualstudio/msbuild/msbuild?view=vs-2022)
- [.NET Project SDKS](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/overview)
- [Creating Reliable Builds](https://learn.microsoft.com/en-us/archive/msdn-magazine/2009/february/msbuild-best-practices-for-creating-reliable-builds-part-1#id0090093)

