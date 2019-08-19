using System;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace ResgateIO.Service.UnitTests
{
    public class ResServiceTests : TestsBase
    {
        public ResServiceTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Serve_NoException()
        {
            Service.Serve(Conn);
        }

        [Fact]
        public void Serve_NoRegisteredHandlers_NoSystemReset()
        {
            Service.Serve(Conn);
            Conn.AssertNoMsg();
        }

        [Fact]
        public void Serve_OwnedResourcesSet_SendsSystemReset()
        {
            var resources = new string[] { "test.>" };
            var access = new string[] { "test.foo.>" };
            Service.SetOwnedResources(resources, access);
            Service.Serve(Conn);
            Conn.GetMsg()
                .AssertSubject("system.reset")
                .AssertPayload(new { resources, access });
        }

        [Fact]
        public void Serve_RegisteredGetHandler_SendsResourcesInSystemReset()
        {
            Service.AddHandler("model", new DynamicHandler().SetModelGet(r => r.NotFound()));
            Service.Serve(Conn);
            Conn.GetMsg()
                .AssertSubject("system.reset")
                .AssertPayload(new { resources = new string[] { "test.>" }, access = new string[] { } });
        }

        [Fact]
        public void Serve_RegisteredAccessHandler_SendsAccessInSystemReset()
        {
            Service.AddHandler("model", new DynamicHandler().SetAccess(r => r.AccessDenied()));
            Service.Serve(Conn);
            Conn.GetMsg()
                .AssertSubject("system.reset")
                .AssertPayload(new { resources = new string[] { }, access = new string[] { "test.>" } });
        }

        [Fact]
        public void Serve_RegisteredAccessAndGetHandler_SendsResourceAndAccessInSystemReset()
        {
            Service.AddHandler("model", new DynamicHandler()
                .SetAccess(r => r.AccessDenied())
                .SetGet(r => r.NotFound()));
            Service.Serve(Conn);
            Conn.GetMsg()
                .AssertSubject("system.reset")
                .AssertPayload(new { resources = new string[] { "test.>" }, access = new string[] { "test.>" } });
        }

        [Fact]
        public void Shutdown_ClosesConnection()
        {
            Service.Serve(Conn);
            Service.Shutdown();
            Assert.True(Conn.Closed, "connection should be closed");
        }

        [Fact]
        public void SetLogger_NullParameter_NoException()
        {
            Service.SetLogger(null);
            Service.Serve(Conn);
        }

        [Fact]
        public void TokenEvent_WithToken_SendsTokenEvent()
        {
            Service.Serve(Conn);
            Service.TokenEvent(Test.CID, Test.Token);
            Conn.GetMsg()
                .AssertSubject("conn." + Test.CID + ".token")
                .AssertPayload(new { token = Test.Token });
        }

        [Fact]
        public void TokenEvent_WithNullToken_SendsNullTokenEvent()
        {
            Service.Serve(Conn);
            Service.TokenEvent(Test.CID, null);
            Conn.GetMsg()
                .AssertSubject("conn." + Test.CID + ".token")
                .AssertPayload(new { token = (object)null });
        }

        [Fact]
        public void TokenEvent_WithInvalidCID_ThrowsException()
        {
            Service.Serve(Conn);
            Assert.Throws<ArgumentException>(() => Service.TokenEvent("invalid.*.cid", null));
        }

        [Fact]
        public void With_WithValidResourceID_CallsCallback()
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler().SetGet(r => r.NotFound()));
            Service.Serve(Conn);
            Service.With("test.model", r => ev.Set());
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
        }

        [Fact]
        public void With_WithoutMatchingPattern_ThrowsException()
        {
            Service.Serve(Conn);
            Assert.Throws<ArgumentException>(() =>
            {
                Service.With("test.model", r => { });
            });
        }
    }
}
