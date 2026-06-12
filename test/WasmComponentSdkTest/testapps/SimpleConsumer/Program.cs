using System.Runtime.InteropServices;
using ConsumerWorld.wit.Imports.test.producerConsumer;

Console.WriteLine($"Hello, world on {RuntimeInformation.OSArchitecture}");

var result = IOperationsImports.Add(123, 456);
Console.WriteLine($"123 + 456 = {result}");
