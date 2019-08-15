using Xunit;
using Xunit.Abstractions;

namespace ResgateIO.Service.UnitTests
{
    public class CreateServiceTests : TestsBase
    {
        public CreateServiceTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Serve()
        {
            Service.Serve(Conn);
        }

        [Fact]
        public void Shutdown()
        {
            Service.Serve(Conn);
            Service.Shutdown();
            Assert.True(Conn.Closed, "connection should be closed");
        }

        [Fact]
        public void ServeWithoutLogger()
        {
            Service.SetLogger(null);
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

    }
}
