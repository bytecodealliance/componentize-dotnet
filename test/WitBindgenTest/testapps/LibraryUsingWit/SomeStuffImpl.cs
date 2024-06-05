// I don't think this namespace should be so different to the one in MyFuncsWorldImpl,
// but currently that's what the codegen requires
using ProducerWorld.wit.exports.test.multipleWorlds;

public class SomeStuffImpl : ISomeStuff
{
    public static int GetNumber()
    {
        return 456;
    }
}
