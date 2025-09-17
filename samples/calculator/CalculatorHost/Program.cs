using HostappWorld.wit.imports.example.calculator;

var left = 123;
var right = 456;
var result = OperationsInterop.Add(left, right);
Console.WriteLine($"{left} + {right} = {result}");

Console.WriteLine(OperationsInterop.ToUpper("Hello, World!"));

// Invoke GetRepo for a known repository (no environment variables required)
const string repoOwner = "bytecodealliance";
const string repoName = "componentize-dotnet";
try {
    var repoJson = OperationsInterop.GetRepo(repoOwner, repoName);
    Console.WriteLine("GetRepo JSON:");
    Console.WriteLine(string.IsNullOrEmpty(repoJson) ? "<empty response>" : repoJson);
} catch (Exception ex) {
    Console.Error.WriteLine($"GetRepo failed: {ex.Message}");
    Environment.ExitCode = 1; // propagate failure so outer script can detect
}
