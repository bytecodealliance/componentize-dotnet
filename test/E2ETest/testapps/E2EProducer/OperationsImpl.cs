namespace ProducerWorld.wit.exports.test.producerConsumer;

public class OperationsImpl : IOperations
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
