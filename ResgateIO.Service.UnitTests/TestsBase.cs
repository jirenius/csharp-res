using System;
using Xunit;
using Xunit.Abstractions;

namespace ResgateIO.Service.UnitTests
{
    /// <summary>
    /// Initializes a mock connection, a test logger, and a service with the name "test".
    /// </summary>
    public abstract class TestsBase : IDisposable
    {
        public readonly ITestOutputHelper Output;
        public readonly ResService Service;
        public readonly MockConnection Conn;

#pragma warning disable IDE1006 // Naming Styles
        public ResService service => Service;
#pragma warning restore IDE1006 // Naming Styles

        public TestsBase(ITestOutputHelper output) : this(output, "test") { }

        public TestsBase(ITestOutputHelper output, string serviceName)
        {
            Output = output;
            Service = new ResService(serviceName).SetLogger(new TestLogger(Output));
            Conn = new MockConnection();
        }

        public void Dispose()
        {
            Service.Shutdown();
            Conn.Dispose();
        }
    }
}
