using System.Runtime.InteropServices;
using ConsumerWorld.wit.imports.test.producerConsumer;

Console.WriteLine($"Hello, world on {RuntimeInformation.OSArchitecture}");

var result = OperationsInterop.Add(123, 456);
Console.WriteLine($"123 + 456 = {result}");
