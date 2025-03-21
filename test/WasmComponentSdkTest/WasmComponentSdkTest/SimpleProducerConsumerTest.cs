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
        var (stdout, stderr, code) = ExecuteCommandComponent(composed);
         if (code != 0) {
            Assert.Fail(stderr);
        }
        Assert.StartsWith("Hello, world on Wasm", stdout);
        Assert.Contains("123 + 456 = 579", stdout);
    }

    [Fact]
    public void CanBuildAppFromOci()
    {
        var composed = FindModulePath($"../testapps/OciWit/bin/{Config}", "ociwit.wasm");
        var (stdout, stderr, code) = ExecuteCommandComponent(composed, "-S http");
        if (code != 0) {
            Assert.Fail(stderr);
        }
        Assert.StartsWith("Oci is awesome!", stdout);
    }

    private static (string, string, int) ExecuteCommandComponent(string componentFilePath)
    {
        return ExecuteCommandComponent(componentFilePath, string.Empty);
    }

    private static (string, string, int) ExecuteCommandComponent(string componentFilePath, string wasmtime_args)
    {
        var startInfo = new ProcessStartInfo(WasmtimeExePath, $" {wasmtime_args} {componentFilePath}") { RedirectStandardOutput = true, RedirectStandardError = true };
        var process = Process.Start(startInfo) ?? throw new InvalidOperationException("Failed to start process.");
        process.WaitForExit();
        int code = process.ExitCode;
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();

        return (stdout, stderr, code);
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
        if (matches.Count() == 1)
        {
            return Path.GetFullPath(matches.Single());
        }
        else if (matches.Count() == 2 && matches.Any(x => Path.GetFullPath(x).Contains($"wasi-wasm\\native")))
        {
            return Path.GetFullPath(matches.First(x => Path.GetFullPath(x).Contains($"wasi-wasm\\native")));
        }
        else if (matches.Count() == 2 && matches.Any(x => Path.GetFullPath(x).Contains($"wasi-wasm/native")))
        {
            return Path.GetFullPath(matches.First(x => Path.GetFullPath(x).Contains($"wasi-wasm/native")));
        }
        else
        {
            throw new Exception($"Failed to get modules path, matched {matches.Count()} entries for directory {resolvedSearchDir} and filename {filename}.");
        }
    }
}
