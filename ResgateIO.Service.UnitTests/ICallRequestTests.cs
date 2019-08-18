using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ResgateIO.Service.UnitTests
{
    public class ICallRequestTests : TestsBase
    {
        public ICallRequestTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Properties_WithValidMethodInRequest_ReturnsCorrectValue()
        {
            Service.AddHandler("model", new DynamicHandler().SetCall(r =>
            {
                Assert.Equal("method", r.Method);
                Assert.Equal(Test.CID, r.CID);
                Assert.True(JToken.DeepEquals(r.RawToken, Test.Token), "RawToken is not equal to sent token");
                Assert.True(JToken.DeepEquals(r.RawParams, Test.Params), "RawParams is not equal to sent params");
                r.Ok(Test.Result);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.RequestWithParams);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.Result);
        }
    }
}
