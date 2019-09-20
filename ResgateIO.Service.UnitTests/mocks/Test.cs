using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ResgateIO.Service.UnitTests
{
    public static class Test
    {
        public const string CID = "testcid";
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
        public const int TokenId = 12;
        public const string TokenRole = "foo";
        public const int ParamNumber = 42;
        public const string ParamText = "bar";
        public static readonly TokenDto Token = new TokenDto { Id = TokenId, Role = TokenRole };
        public static readonly ParamsDto Params = new ParamsDto { Number = ParamNumber, Text = ParamText };
        public static readonly byte[] EmptyRequest = Encoding.UTF8.GetBytes("{}");
        public static readonly RequestDto Request = new RequestDto { CID = CID, Token = Token };
        public static readonly RequestDto RequestWithoutToken = new RequestDto { CID = CID };
        public static readonly RequestDto RequestWithParams = new RequestDto { CID = CID, Token = Token, Params = Params };
        public static readonly RequestDto AuthRequest = new RequestDto { CID = CID, Header = Header, Host = Host, RemoteAddr = RemoteAddr, URI = URI, Token = Token, Params = Params };
        public static readonly RequestDto AuthRequestWithoutTokenAndParams = new RequestDto { CID = CID, Header = Header, Host = Host, RemoteAddr = RemoteAddr, URI = URI };
        public const string ErrorMessage = "Custom error";
        public static readonly object ErrorData = new { foo = "bar" };
        public static readonly ResError CustomError = new ResError("test.custom", ErrorMessage, ErrorData);

        public static readonly object Model = new ModelDto { Id = 42, Foo = "bar" };
        public static readonly object Collection = new object[] { 42, "foo", null };
        public static readonly object Result = new { foo = "bar" };
        public static readonly Ref NewRef = new Ref("test.model.new");
        public static readonly int IntValue = 42;

        /// <summary>
        /// Asserts that two objects will be serialized into deep equal JSON structures.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static void AssertJsonEqual(object expected, object actual)
        {
            string expectedJson = JsonConvert.SerializeObject(expected);
            string actualJson = JsonConvert.SerializeObject(actual);
            Assert.True(
                JToken.DeepEquals(JToken.Parse(expectedJson), JToken.Parse(actualJson)),
                String.Format("Json mismatch:\nExpected:\n\t{0}\nActual:\n\t{1}", expectedJson, actualJson)
            );
        }
    }
}
