using HostappWorld.wit.Imports.example.calculator;

var left = 123;
var right = 456;
var result = IOperationsImports.Add(left, right);
Console.WriteLine($"{left} + {right} = {result}");

Console.WriteLine(IOperationsImports.ToUpper("Hello, World!"));
