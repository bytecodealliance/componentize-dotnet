// I don't think this namespace should be so different to the one in MyFuncsWorldImpl,
// but currently that's what the codegen requires
using MyWitResultsWorld;



public class MyWitResultsWorldImpl : IMyWitResultsWorld
{
    public static Result<float, string> StringError(float a)
    {
        return Result<float, string>.Ok(a);
    }
}
