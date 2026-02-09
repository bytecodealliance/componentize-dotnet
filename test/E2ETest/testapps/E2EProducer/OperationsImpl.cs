namespace ProducerWorld.wit.Exports.test.producerConsumer;

public class OperationsExportsImpl : IOperationsExports
{
    public static int Add(int left, int right)
    {
        return left + right;
    }

    public static float AddFloat(float left, float right)
    {
        return left + right;
    }
}
