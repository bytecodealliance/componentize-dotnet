using CommandWorld.wit.exports.wasi.cli.v0_2_0;
using MyFuncsWorld;

public class RunImpl : IRun
{
    public static void Run()
    {
        Console.WriteLine("Oci is awesome!"); 
    }
}

public class MyFuncsWorldImpl : IMyFuncsWorld
{
    public static int GetNumber()
    {
        return 123;
    }
}
