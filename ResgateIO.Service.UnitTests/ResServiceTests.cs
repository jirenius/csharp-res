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
        public void Serve_OwnedResourcesSet__SendsSystemReset()
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

    }
}
