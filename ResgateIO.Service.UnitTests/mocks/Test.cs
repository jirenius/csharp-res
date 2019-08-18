using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;

namespace ResgateIO.Service.UnitTests
{
    public static class Test
    {
        public const string CID = "testcid";
        public static readonly JToken Token = JToken.Parse("{\"id\":12,\"role\":\"foo\"}");
        public static readonly JToken Params = JToken.Parse("{\"arg1\":12,\"arg2\":\"foo\"}");
        public const string Host = "local";
        public const string RemoteAddr = "127.0.0.1";
        public const string URI = "/ws";
        public static readonly Dictionary<string, string[]> Header = new Dictionary<string, string[]>{
            { "Accept-Encoding",          new string[]{"gzip, deflate, br"} },
            { "Accept-Language",          new string[]{"*"} },
            { "Cache-Control",            new string[]{"no-cache"} },
            { "Connection",               new string[]{"Upgrade"} },
            { "Origin",                   new string[]{"http://localhost"} },
            { "Pragma",                   new string[]{"no-cache"} },
            { "Sec-Websocket-Extensions", new string[]{"permessage-deflate; client_max_window_bits"} },
            { "Sec-Websocket-Key",        new string[]{"dGhlIHNhbXBsZSBub25jZQ=="} },
            { "Sec-Websocket-Version",    new string[]{"13"} },
            { "Upgrade",                  new string[]{"websocket"} },
            { "User-Agent",               new string[]{".NETTest/1.0 (Test)"} },
        };

        public const int TimeoutDuration = 200; // milliseconds
        public static readonly byte[] EmptyRequest = Encoding.UTF8.GetBytes("{}");
        public static readonly RequestDto Request = new RequestDto { CID = CID, RawToken = Token };
        public static readonly RequestDto RequestWithParams = new RequestDto { CID = CID, RawToken = Token, RawParams = Params };
        public static readonly RequestDto AuthRequest = new RequestDto { CID = CID, Header = Header, Host = Host, RemoteAddr = RemoteAddr, URI = URI };
        public const string ErrorMessage = "Custom error";
        public static readonly ResError CustomError = new ResError("test.custom", ErrorMessage, new { foo = "bar" });

        public static readonly object Model = new { id = 42, foo = "bar" };
        public static readonly object Collection = new object[] { 42, "foo", null };
        public static readonly object Result = new { foo = "bar" };
    }
}
