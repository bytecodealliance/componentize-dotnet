namespace ComputerWorld.wit.Exports.example.calculator;

public class OperationsExportsImpl : IOperationsExports
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
