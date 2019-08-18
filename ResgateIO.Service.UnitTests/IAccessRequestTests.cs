using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ResgateIO.Service.UnitTests
{
    public class IAccessRequestTests : TestsBase
    {
        public IAccessRequestTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Properties_WithValidRequest_ReturnsCorrectValue()
        {
            Service.AddHandler("model", new DynamicHandler().SetAccess(r =>
            {
                Assert.Equal(Test.CID, r.CID);
                Assert.True(JToken.DeepEquals(r.RawToken, Test.Token), "RawToken is not equal to sent token");
                r.Error(Test.CustomError);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("access.test.model", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(Test.CustomError);
        }

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
            string inbox = Conn.NATSRequest("access.test.model", Test.Request);
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
            string inbox = Conn.NATSRequest("access.test.model", Test.Request);
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
            string inbox = Conn.NATSRequest("access.test.model", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(new { get = true, call = "*" });
        }

        [Fact]
        public void Error_SendsErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetAccess(r => r.Error(Test.CustomError)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("access.test.model", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(Test.CustomError);
        }

        [Fact]
        public void NotFound_SendsNotFoundErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetAccess(r => r.NotFound()));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("access.test.model", Test.Request);
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
            string inbox = Conn.NATSRequest("access.test.model", Test.Request);
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
            string inbox = Conn.NATSRequest("access.test.model", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertPayload(Encoding.UTF8.GetBytes("timeout:\"4000\""));
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.NotFound);
        }

        [Fact]
        public void Timeout_WithNegativeDuration_ThrowsException()
        {
            Service.AddHandler("model", new DynamicHandler().SetAccess(r =>
            {
                Assert.Throws<ArgumentException>(() => r.Timeout(-1));
                r.NotFound();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("access.test.model", Test.EmptyRequest);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInternalError);
        }

        [Fact]
        public void AccessRequest_ThrownException_SendsInternalErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler()
                .SetAccess(r => throw new Exception(Test.ErrorMessage)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("access.test.model", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInternalError, Test.ErrorMessage);
        }

        [Fact]
        public void AccessRequest_ThrownResException_SendsErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler()
                .SetAccess(r => throw new ResException(Test.CustomError)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("access.test.model", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(Test.CustomError);
        }

        [Fact]
        public void AccessRequest_MultipleRequests_RespondedInOrder()
        {
            const int requestCount = 100;
            Service.AddHandler("model", new DynamicHandler().SetAccess(r => r.AccessGranted()));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");

            string[] inboxes = new string[requestCount];
            for (int i = 0; i < requestCount; i++)
            {
                inboxes[i] = Conn.NATSRequest("access.test.model", Test.Request);
            }
            for (int i = 0; i < requestCount; i++)
            {
                Conn.GetMsg()
                    .AssertSubject(inboxes[i])
                    .AssertResult();
            }
        }

        [Fact]
        public void AccessRequest_MultipleAccessGrantedCalls_SendsSingleAccessGrantedResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetAccess(r =>
            {
                r.AccessGranted();
                r.AccessGranted();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("access.test.model", Test.Request);
            Conn.GetMsg().AssertSubject(inbox);
            string inbox2 = Conn.NATSRequest("access.test.model", Test.Request);
            Conn.GetMsg().AssertSubject(inbox2);
        }
    }
}
