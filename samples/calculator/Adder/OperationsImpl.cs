using System;
using ComputerWorld.wit.imports.wasi.http.v0_2_0; // May be generated if wasi:http present; harmless if unused
using static ComputerWorld.wit.imports.wasi.http.v0_2_0.ITypes;

namespace ComputerWorld.wit.exports.example.calculator;

// Implementation of operations interface generated from WIT
public class OperationsImpl : IOperations
{
    public static int Add(int left, int right) => left + right;

    public static string ToUpper(string input) => input.ToUpperInvariant();

    // Renamed to match WIT change: get-repo(owner, name)
    public static string GetRepo(string owner, string name)
    {
        // Performs a simple unauthenticated GET to the GitHub repository metadata endpoint.
        // Parameters map directly to the GitHub URL path segments.
        try
        {
            if (string.IsNullOrWhiteSpace(owner) || string.IsNullOrWhiteSpace(name))
                return "{\"error\":\"missing-owner-or-name\"}";
            var safeOwner = owner.Trim();
            var safeName = name.Trim();
            // Very basic validation to avoid path injection
            if (safeOwner.Contains('/') || safeName.Contains('/'))
                return "{\"error\":\"invalid-slash\"}";
            var url = $"https://api.github.com/repos/{safeOwner}/{safeName}";

            // Empty headers collection (GitHub typically requires a User-Agent, but per request no headers are passed).
            // Fields headers;
            // try
            // {
            //     headers = Fields.FromList(new System.Collections.Generic.List<(string, byte[])>());
            // }
            // catch (WitException<HeaderError> hex)
            // {
            //     return $"{{\"error\":\"header-build\",\"detail\":\"{hex.TypedValue.Tag}\"}}";
            // }

            var headers = new Fields();
            try {
                // GitHub API recommends providing a User-Agent; also add Accept for clarity.
                headers.Set("User-Agent", new System.Collections.Generic.List<byte[]> { System.Text.Encoding.UTF8.GetBytes("componentize-dotnet-sample/1.0") });
                headers.Set("Accept", new System.Collections.Generic.List<byte[]> { System.Text.Encoding.UTF8.GetBytes("application/vnd.github+json") });
            } catch (WitException<HeaderError>) { /* non-fatal if immutable/forbidden */ }
            var req = new OutgoingRequest(headers);
            req.SetMethod(Method.Get());

            var uri = new Uri(url);
            req.SetScheme(uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) ? Scheme.Https() : Scheme.Http());
            req.SetAuthority(uri.Authority);
            req.SetPathWithQuery(uri.PathAndQuery);

            RequestOptions? options = null;
            var future = OutgoingHandlerInterop.Handle(req, options);

            IncomingResponse? incoming = null;
            const int timeoutMs = 5000; // total budget
            var sw = System.Diagnostics.Stopwatch.StartNew();
            int delay = 10; // start with 10ms, exponential backoff up to 250ms
            while (sw.ElapsedMilliseconds < timeoutMs)
            {
                var resultOpt = future.Get();
                if (resultOpt != null)
                {
                    var outer = resultOpt.Value;
                    if (outer.IsOk)
                    {
                        var inner = outer.AsOk;
                        if (inner.IsOk)
                        {
                            incoming = inner.AsOk;
                        }
                        else
                        {
                            var err = inner.AsErr;
                            return $"{{\"error\":\"http-transport\",\"code\":\"{err.Tag}\"}}";
                        }
                    }
                    break;
                }
                System.Threading.Thread.Sleep(delay);
                delay = Math.Min(delay * 2, 250);
            }

            if (incoming == null)
            {
                return $"{{\"error\":\"timeout\",\"elapsedMs\":{sw.ElapsedMilliseconds}}}";
            }

            int status = incoming.Status();
            string body = string.Empty;
            try
            {
                var bodyRes = incoming.Consume();
                var stream = bodyRes.Stream();
                var bytes = new System.Collections.Generic.List<byte>();
                while (true)
                {
                    var chunk = stream.Read(1024);
                    if (chunk.Length == 0)
                        break;
                    bytes.AddRange(chunk);
                }
                body = System.Text.Encoding.UTF8.GetString(bytes.ToArray());
                stream.Dispose();
                bodyRes.Dispose();
            }
            finally
            {
                incoming.Dispose();
                future.Dispose();
            }

            if (status >= 200 && status < 300) return body;
            return $"{{\"error\":\"http-status\",\"status\":{status},\"body\":{EscapeForJson(body)} }}";
        }
        catch (Exception ex)
        {
            return $"{{\"error\":\"exception\",\"message\":\"{Escape(ex.Message)}\"}}";
        }
    }

    private static string Escape(string s) => s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    private static string EscapeForJson(string s)
    {
        return "\"" + Escape(s) + "\"";
    }
}
