using System.Diagnostics;
using System.Reflection;
using Xunit;

namespace WasmComponentSdkTest;

public class SimpleProducerConsumerTest
{
    // Unfortunately it doesn't seem possible to use wasmtime-dotnet with the component model yet,
    // (there's literally no mention of the entire concept within the wasmtime-dotnet repo), so for
    // now the tests work by invoking the wasmtime CLI

#if DEBUG
    const string Config = "Debug";
#else
    const string Config = "Release";
#endif

    [Fact]
    public void CanBuildComponentWithImport()
    {
        var witInfo = GetWitInfo(FindModulePath($"../testapps/SimpleConsumer/bin/{Config}", "simpleconsumer.wasm"));
        Assert.Contains("import test:producer-consumer/operations", witInfo);
    }

    [Fact]
    public void CanBuildComponentWithExport()
    {
        var witInfo = GetWitInfo(FindModulePath($"../testapps/SimpleProducer/bin/{Config}", "simpleproducer.wasm"));
        Assert.Contains("export test:producer-consumer/operations", witInfo);
    }

    [Fact]
    public void CanBuildComponentWithWitPackage()
    {
        var witInfo = GetWitInfo(FindModulePath($"../testapps/AppWithWitFolder/bin/{Config}", "appwithwitfolder.wasm"));
        Assert.Contains("import test:pkg/folder", witInfo);
    }

    [Fact]
    public void CanComposeImportWithExport()
    {
        var composed = FindModulePath("../testapps/SimpleConsumer", "composed.wasm");
        var stdout = ExecuteCommandComponent(composed);
        Assert.StartsWith("Hello, world on Wasm", stdout);
        Assert.Contains("123 + 456 = 579", stdout);
    }

    [Fact]
    public void CanBuildAppFromOci()
    {
        var composed = FindModulePath("../testapps/OciWit", "ociwit.wasm");
        var stdout = ExecuteCommandComponent(composed);
        Assert.StartsWith("Oci is awesome!", stdout);
    }

    private static string ExecuteCommandComponent(string componentFilePath)
    {
        var startInfo = new ProcessStartInfo(WasmtimeExePath, $"-W component-model {componentFilePath}") { RedirectStandardOutput = true };
        var stdout = Process.Start(startInfo)!.StandardOutput.ReadToEnd();
        return stdout;
    }

    private static string GetWitInfo(string componentFilePath)
    {
        var startInfo = new ProcessStartInfo(WasmToolsExePath, $"component wit {componentFilePath}") { RedirectStandardOutput = true };
        var witInfo = Process.Start(startInfo)!.StandardOutput.ReadToEnd();
        return witInfo;
    }

    private static string WasmtimeExePath { get; } = GetAssemblyMetadataValue("WasmtimeExe")!;
    private static string WasmToolsExePath { get; } = GetAssemblyMetadataValue("WasmToolsExe")!;

    private static string? GetAssemblyMetadataValue(string key) =>
        typeof(SimpleProducerConsumerTest).Assembly
        .GetCustomAttributes<AssemblyMetadataAttribute>()
        .SingleOrDefault(x => x.Key == key)?.Value;

    private static string FindModulePath(string searchDir, string filename)
    {
        var resolvedSearchDir = Path.Combine(
            Path.GetDirectoryName(typeof(SimpleProducerConsumerTest).Assembly.Location)!,
            "../../..",
            searchDir);

        if (!Directory.Exists(resolvedSearchDir))
        {
            throw new InvalidOperationException($"No such directory: {Path.GetFullPath(resolvedSearchDir)}");
        }

        var matches = Directory.GetFiles(resolvedSearchDir, filename, SearchOption.AllDirectories);
        return Path.GetFullPath(matches.Single());
    }
}
