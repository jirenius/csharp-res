using Newtonsoft.Json.Linq;
using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ResgateIO.Service.UnitTests
{
    public class IAccessRequestTests : TestsBase
    {
        public IAccessRequestTests(ITestOutputHelper output) : base(output) { }

        [Theory]
        [InlineData(true, null, "{\"get\":true}")]
        [InlineData(true, "", "{\"get\":true}")]
        [InlineData(true, "foo", "{\"get\":true,\"call\":\"foo\"}")]
        [InlineData(true, "foo,bar", "{\"get\":true,\"call\":\"foo,bar\"}")]
        [InlineData(true, "*", "{\"get\":true,\"call\":\"*\"}")]
        [InlineData(false, null, "{\"get\":false}")]
        [InlineData(false, "", "{\"get\":false}")]
        [InlineData(false, "foo", "{\"get\":false,\"call\":\"foo\"}")]
        [InlineData(false, "foo,bar", "{\"get\":false,\"call\":\"foo,bar\"}")]
        [InlineData(false, "*", "{\"get\":false,\"call\":\"*\"}")]
        public void Access_SendsAccessResponse(bool getAccess, string callAccess, string expectedJson)
        {
            Service.AddHandler("model", new DynamicHandler().SetAccess(r => r.Access(getAccess, callAccess)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("access.test.model", null);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(JObject.Parse(expectedJson));
        }

        [Fact]
        public void AccessGranted_SendsAccessDeniedResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetAccess(r => r.AccessDenied()));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("access.test.model", null);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(new { get = false });
        }

        [Fact]
        public void AccessGranted_SendsAccessGrantedResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetAccess(r => r.AccessGranted()));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("access.test.model", null);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(new { get = true, call = "*" });
        }

        [Fact]
        public void Error_SendsErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetAccess(r => r.Error(ResError.NotFound)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("access.test.model", null);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.NotFound);
        }

        [Fact]
        public void NotFound_SendsNotFoundErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetAccess(r => r.NotFound()));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("access.test.model", null);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.NotFound);
        }

        [Fact]
        public void Timeout_WithMilliseconds_SendsPreresponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetAccess(r =>
            {
                r.Timeout(3000);
                r.NotFound();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("access.test.model", null);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertPayload(Encoding.UTF8.GetBytes("timeout:\"3000\""));
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.NotFound);
        }

        [Fact]
        public void Timeout_WithTimespan_SendsPreresponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetAccess(r =>
            {
                r.Timeout(new TimeSpan(0, 0, 4));
                r.NotFound();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("access.test.model", null);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertPayload(Encoding.UTF8.GetBytes("timeout:\"4000\""));
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.NotFound);
        }
    }
}
