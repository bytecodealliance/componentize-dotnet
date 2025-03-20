using System.Runtime.InteropServices;
using ConsumerWorld.wit.imports.test.producerConsumer;

Console.WriteLine($"Hello, world on {RuntimeInformation.OSArchitecture}");

var result = OperationsInterop.Add(123, 456);
Console.WriteLine($"123 + 456 = {result}");

// this is just here to make sure we can enable features and pass flags via WitBindgenAddtionalArgs
// the fact that it compiles is enough to test this worked.
var floatResult = OperationsInterop.AddFloat(1.1f, 1.2f);
Console.WriteLine($"1.1 + 1.2 = {floatResult}");
