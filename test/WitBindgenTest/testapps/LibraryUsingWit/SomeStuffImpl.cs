// I don't think this namespace should be so different to the one in MyFuncsWorldImpl,
// but currently that's what the codegen requires
using ProducerWorld.wit.Exports.test.multipleWorlds;

public class SomeStuffExportsImpl : ISomeStuffExports
{
    public static int GetNumber()
    {
        return 456;
    }
}
