# Submitting bugs and fixes
Open an issue detailing the issue you've encountered or feature you would like to see.

Bug fixes and new features must be submitted using a pull request and pass CI to be included in the project.

## Building the project locally

Requires [.NET 8+](https://dotnet.microsoft.com/en-us/download)

```
dotnet msbuild src/WasmComponent.Sdk/build/WasmComponent.Sdk.targets /t:PrepareWasmSdks
git submodule update --init
dotnet build
```

If you are experiencing issues with values not being updated, try running `dotnet clean` and using the steps above

## Testing

Run the tests:

```
dotnet test
```

## Getting help
While we work on improving the documentation for contributing, if you have any questions please drop a note in the [c# zulip chat](https://bytecodealliance.zulipchat.com/#narrow/stream/407028-C.23.2F.2Enet-collaboration).