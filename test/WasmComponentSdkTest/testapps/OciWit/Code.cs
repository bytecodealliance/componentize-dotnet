using CommandWorld.wit.Exports.wasi.cli.v0_2_0;
using MyFuncsWorld;
using System.Text;

namespace ProxyWorld.wit.Exports.wasi.http.v0_2_0
{
    using ProxyWorld.wit.Imports.wasi.http.v0_2_0;
    public class IncomingHandlerExportsImpl : IIncomingHandlerExports
    {
        public static void Handle(ITypes.IncomingRequest request, ITypes.ResponseOutparam responseOut)
        {
            var content = Encoding.ASCII.GetBytes("Hello, from C#!");
            var headers = new List<(string, byte[])> {
            ("content-type", Encoding.ASCII.GetBytes("text/plain")),
            ("content-length", Encoding.ASCII.GetBytes(content.Count().ToString()))
        };
            var response = new ITypes.OutgoingResponse(ITypes.Fields.FromList(headers));
            var body = response.Body();
            ITypes.ResponseOutparam.Set(responseOut, Result<ITypes.OutgoingResponse, ITypes.ErrorCode>.Ok(response));
            using (var stream = body.Write())
            {
                stream.BlockingWriteAndFlush(content);
            }
            ITypes.OutgoingBody.Finish(body, null);
        }
    }
}
namespace CommandWorld.wit.Exports.wasi.cli.v0_2_0
{
    public class RunExportsImpl : IRunExports
    {
        public static void Run()
        {
            Console.WriteLine("Oci is awesome!");
        }
    }
}

namespace MyFuncsWorld
{
    public class MyFuncsWorldExportsImpl : IMyFuncsWorldExports
    {
        public static int GetNumber()
        {
            return 123;
        }
    }
}
