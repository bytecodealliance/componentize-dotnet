<a href="https://www.nuget.org/packages/BytecodeAlliance.Componentize.DotNet.Wasm.SDK">
      <img src="https://img.shields.io/nuget/v/BytecodeAlliance.Componentize.DotNet.Wasm.SDK" alt="Latest Version"/>
</a>

# componentize-dotnet

Simplifying C# wasm components 

**A [Bytecode Alliance](https://bytecodealliance.org/) hosted project**

If you have any questions of problems feel free to reach out on the  [c# zulip chat](https://bytecodealliance.zulipchat.com/#narrow/stream/407028-C.23.2F.2Enet-collaboration).

## Purpose

This is to simplify using Wasm components in c#.

Without this package, if you wanted to build a WASI 0.2 component with .NET, including using WIT imports/exports, there are several different tools you'd need to discover, download, configure, and manually chain together. Just figuring out which versions of each are compatible with the others is a big challenge. Working out how to get started would be very painful.

With this package, you can add one NuGet reference. The build output is fully AOT compiled and is known to work in recent versions of wasmtime and WAMR.

:construction: All the underlying technologies are under heavy development and are missing features. Please try to file it on the relevant underlying [tool](#credits) if relevant to that tool.

## Getting started

### 1. Set up SDKs

If you don't already have it, install [.NET 10+ preview SDK](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

### 2. Install template and build

```bash
dotnet new install BytecodeAlliance.Componentize.DotNet.Templates
dotnet new componentize.wasi.cli -o MyApp
dotnet build MyApp
```

### 3. Run the WebAssembly binary

If you have a recent version of [wasmtime](https://github.com/bytecodealliance/wasmtime/releases) on your path, you can now run

```bash
wasmtime  ./MyApp/bin/Debug/net10.0/wasi-wasm/publish/MyApp.wasm
Hello world from compontize-dotnet!
```

(if needed, replace `MyApp` with the actual name of your project)

## Creating a WASI 0.2 component, including WIT support
Lastest version of NativeAOT compiler package and the mono support in dotnet 9-preview 7 build native wasi 0.2 components with no additional tools.

### Referencing a WIT file

The whole point of the WASI 0.2 component model is to be able to interoperate across components. This is achieved using [WebAssembly Interface Type (WIT)](https://github.com/WebAssembly/component-model/blob/main/design/mvp/WIT.md) files that specify data structures and functions to be imported or exported across components.

There is a full sample of this walk through creating WIT components for you reference in [samples](./samples/).  

This package wraps `wit-bindgen` so that any `.wit` files in your project will automatically generate corresponding C# sources, allowing you to import or export functionality. 

For example, add a file called `calculator.wit` into your project, containing the following:

```js
package example:calculator;

interface operations {
  add: func(left: s32, right: s32) -> s32;
}

world computer {
  export operations;
}

world hostapp {
  import operations;
}
```

Before you can build, you'll now need to specify which *world* you're generating code for, i.e., choose whether you're importing or exporting functions.

#### Importing an implementation

One way to pick a world is by editing your `.csproj`, adding the following:

```xml
  <ItemGroup>
    <Wit Update="calculator.wit" World="hostapp" />
  </ItemGroup>
```

Another option, if you use Visual Studio, is to select the WIT file in *Solution Explorer*, and then look at the *Properties* pane:

![image](https://github.com/dotnet/runtimelab/assets/1101362/86a204d1-985d-4d36-8bbd-5581375d989e)

You can simply type the world name `hostapp` into the properties pane.

Now you can call the imported `Add` function by putting the following in `Program.cs`:

```cs
using HostappWorld.wit.imports.example.calculator;

var left = 123;
var right = 456;
var result = OperationsInterop.Add(left, right);
Console.WriteLine($"{left} + {right} = {result}");
```

Since your component is no longer a self-contained application, you can no longer run it without also composing it with another WASI 0.2 component that implements the `add` function. To do that, either:

 * Create another .NET project and this time follow the steps for [exporting an implementation](#exporting-an-implementation) below
 * Or, read docs for other platforms such as Rust or TinyGo, to implement a WASI component containing the implementation.

#### Exporting an implementation

If you're **exporting** functionality, you'll be building a class library, not an executable. So be sure to go to your `.csproj` and change `<OutputType>` from `exe` to `library` and delete any `Program.cs`.

Once you've done that, change your WIT file to use the `computer` world using one of the two techniques described above (i.e., either edit the `.csproj` or use the VS Properties pane).

Now when you build, you'll get an error like `The name 'OperationsImpl' does not exist in the current context`. This is because you've said you'll provide an implementation, but haven't yet done so. To fix this, add the following class to your project:

```cs
namespace ComputerWorld.wit.exports.example.calculator;

public class OperationsImpl : IOperations
{
    public static int Add(int left, int right)
    {
        return left + right;
    }

    public static string ToUpper(string input)
    {
        return input.ToUpperInvariant();
    }
}
```

Make sure to get the namespace exactly correct! Although this is quite difficult to figure out at the moment, hopefully a future version of the C# support in wit-bindgen will make it easier.

Now when you build, you'll get a real WASI 0.2 component that exports an implementation for this WIT definition. You can confirm it using [wasm-tools](https://github.com/bytecodealliance/wasm-tools) by running:

```bash
wasm-tools component wit bin\Debug\net8.0\wasi-wasm\native\MyApp.wasm
```

Outputs:

```js
package root:component;

world root {
  import ... various things ...

  export example:calculator/operations;
}
```

## Composing components

Once you have a components containing the Adder and Calculator host, you can use [wac](https://github.com/bytecodealliance/wac) to compose a runnable application:

```bash
wac plug MyApp.wasm --plug AddImplementation.wasm -o composed.wasm
```

then run it:

```bash
wasmtime composed.wasm
```

While you can run wac manually, you can also generate this automatically.  One way to do this is to [create a new project](./samples/calculator/CalculatorComposed/) and add the following:

```xml
 <Target Name="ComposeWasmComponent" AfterTargets="Publish">
        <PropertyGroup>
            <EntrypointComponent>../CalculatorHost/bin/$(Configuration)/$(TargetFramework)/wasi-wasm/native/calculatorhost.wasm</EntrypointComponent>
            <DependencyComponent>../Adder/bin/$(Configuration)/$(TargetFramework)/wasi-wasm/native/adder.wasm</DependencyComponent>
        </PropertyGroup>
        
        <MakeDir Directories="dist" />
        <Exec Command="$(WacExe) plug $(EntrypointComponent) --plug $(DependencyComponent) -o dist/calculator.wasm" />
    </Target>
```

Another option is to do it from the project where the final component will be composed as the output.  See the example in the [e2e tests](test/E2ETest/testapps/E2EConsumer/E2EConsumer.csproj)

You can also use this technique to use other [wasm-tools](https://github.com/bytecodealliance/wasm-tools) functionality such as `wasm-tools strip` to produce a final binary.

This final component can be used anywhere that WASI 0.2 components can be used. For example, use `wasmtime` as illustrated above.

### Referencing Wit Packages

By default the project will find all wit files and execute wit-bindgen against each one. This is makes it easy to get started with a single wit file.  If you have more complicated wit files, then you can create wit packages. To use folder with all the wit in it you can add the following to your `.csproj`. 

```xml
<ItemGroup>
    <Wit Remove="**\*.wit"  />
    <Wit Include="wit-folder" World="wit-world" /> 
</ItemGroup>
```

### Configuring location of generated wit files

By default the wit files are generated under the itermediate path `$(IntermediateOutputPath)wit_bindgen` which is by default at a location like `\obj\Debug\net10.0\wit_bindgen`.  This means you can't really see the files and they will be ignored and not checked in to source by default (assuming you have a standard .net `.gitignore`) but intillisense will work. 

If you would like to have the files in a more discoverable place and potentially check the source files in you can configure the output location with a folder name and location of your choice, for example the following will create a folder `genererated/wit/` at the root of your project and put all the files in there.

```xml
<PropertyGroup>
    <WitGeneratedFilesRoot>generated/wit</WitGeneratedFilesRoot>
</PropertyGroup>
```

### Passing additional wit-bindgen args

[wit-bindgen](https://github.com/bytecodealliance/wit-bindgen/tree/main) for c# has some advanced settings that can be set by using `WitBindgenAddtionalArgs` property. A non-exhustive list of example args that might be useful are:

- `--features <featurename>` - turn on [wit features](https://github.com/WebAssembly/component-model/blob/main/design/mvp/WIT.md#feature-gates)
- `--with-wit-results` - use [wit Result types](https://github.com/WebAssembly/component-model/blob/main/design/mvp/WIT.md#wit-types) instead of generating exceptions
- `--skip-support-files` - don't output files like `WasmImportLinkageAttribute`

Example: 

```xml
<PropertyGroup>
  <WitBindgenAddtionalArgs>--with-wit-results --features tls</WitBindgenAddtionalArgs>
</PropertyGroup>
```

To learn about addtional args run  `wit-bindgen c-sharp -h`

### Referencing Wit Packages from OCI Registries

Wit can be packaged into [OCI Artifacts](https://tag-runtime.cncf.io/wgs/wasm/deliverables/wasm-oci-artifact/) which can be pushed to OCI registries.  To import a WIT definition from an OCI registry specify the Registry on the `Wit` Element.  This will pull a WASM component binary that contains the WIT definition. wit-bindgen can use the binary format directly to generate the bindings. To view the WIT directly use [`wasm-tools component wit wit/wit.wasm`](https://github.com/bytecodealliance/wasm-tools).  

```xml
  <ItemGroup>
    <Wit Remove="**\*.wit" />
    <Wit Include="wit/wit.wasm" World="command" Registry="ghcr.io/webassembly/wasi/cli:0.2.0" />
  </ItemGroup>
```

### WIT strings and memory

The calculator example above works easily because it doesn't need to allocate memory dynamically. Once you start working with strings, you must add an extra line to the `<PropertyGroup>` in your _host_ `.csproj` file (that is, the application that's _importing_ the interface):

```xml
    <IlcExportUnmanagedEntrypoints>true</IlcExportUnmanagedEntrypoints>
```

(You don't need to add this to your class library/exporting `.csproj`.)

If you get a build error along the lines of _failed to encode a component from module ... module does not export a function named `cabi_realloc`_ then check you have remembered to add this line.

### Create a project manually

* `dotnet new console -o MyApp`
* `cd MyApp`

Edit the `.csproj` file:

1. Add the following line under `<Project>`:

  ```xml
  <Project Sdk="BytecodeAlliance.Componentize.DotNet.WitBindgen" Version="[LatestVersion]" />
  ```
2. Add the following inside the `<PropertyGroup>`:

  ```xml
  <RuntimeIdentifier>wasi-wasm</RuntimeIdentifier>
  <PublishTrimmed>true</PublishTrimmed>
  <InvariantGlobalization>true</InvariantGlobalization>
  <SelfContained>true</SelfContained>
  ```

Now you can `dotnet build` to produce a `.wasm` file using NativeAOT compilation.

### Troubleshooting

#### Imports Wrong Type
```bash
*import 'wasi:...' has the wrong type* 
```

You need a different version of Wasmtime. Currently this package targets [Wasmtime](https://github.com/bytecodealliance/wasmtime/releases/tag/v23.0.2). WASI 0.2 is now stable and so you shouldn't run into this often.

#### Component imports missing
```bash
Error: component imports instance `wasi:cli/environment@0.2.0`, but a matching implementation was not found in the linker

Caused by:
    0: instance export `get-environment` has the wrong type
    1: function implementation is missing
```

Some imports automatically imported since they are so common.  In this case you should tell the runtime to implement those imports. For instance for the error above, in wasmtime you might add `-S cli` to the `wasmtime serve` command like `wasmtime serve -S cli` to include the `wasi:cli/environment@0.2.0` in wasmtime runtime host implementation.

## SDK configuration
You can configure the SDK by setting properties in your project file. The following are supported:

| Property Name | Description | Default |
|---------------|-------------|---------|
| `WasiSdkVersion` | Version of the WASI SDK to use.<br />Beware that not all versions of the WASI SDK are compatible with NativeAOT-LLVM. You'll see an warning if you pick an incompatible version from NativeAOT-LLVM.<br /><br />⚠️ When changing this value, or the NativeAOT-LLVM version, check the build output for any warnings! | `24.0` |
| `NativeAotLlvmVersion` | Version of the NativeAOT-LLVM package to use.<br />Find latest versions in the [dotnet-experimental feed](https://dev.azure.com/dnceng/public/_artifacts/feed/dotnet-experimental/NuGet/Microsoft.DotNet.ILCompiler.LLVM/versions). | `10.0.0-alpha.1.25162.1` |
| `WitBindgenVersion` | Version of the `BytecodeAlliance.Componentize.DotNet.WitBindgen` package to use. | Current SDK version |
| `RegisterExperimentalNuGetSource` | Whether to register the [dotnet-experimental feed](https://dev.azure.com/dnceng/public/_artifacts/feed/dotnet-experimental/NuGet) to find the NativeAOT-LLVM package. Set this to `false` if you have a NuGet.config that already includes this feed. | `true` |
| `WitBindgenAddtionalArgs` | Additional arguments to pass to `wit-bindgen` when generating C# bindings. Separate multiple args with spaces. | (empty) |
| `WitGeneratedFilesRoot` | Folder to place generated WIT files. Relative to the project root. | (empty) |

## Credits

This project was original developed and forked from https://github.com/SteveSandersonMS/wasm-component-sdk under the Apache 2.0 License with a LLVM exception.

This is a wrapper around various other bits of tooling:

 * [NativeAOT-LLVM](https://github.com/dotnet/runtimelab/tree/feature/NativeAOT-LLVM) for compilation.
 * [wit-bindgen](https://github.com/bytecodealliance/wit-bindgen) for supporting WIT imports and exports
 * [wasm-tools](https://github.com/bytecodealliance/wasm-tools) for converting WebAssembly core modules into WASI 0.2 components.
 * [WASI SDK](https://github.com/WebAssembly/wasi-sdk) which is used by NativeAOT-LLVM.
   * Compatible versions of these will be downloaded and cached on your machine the first time you run a build, so the first build will take a few minutes. After that it will only take seconds.

## Contributing

See our [contributing docs](./CONTRIBUTING.md) for details on how to build and contribute to this project.
