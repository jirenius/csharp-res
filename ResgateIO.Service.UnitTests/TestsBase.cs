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

        public TestsBase(ITestOutputHelper output)
        {
            Output = output;
            Service = new ResService("test").SetLogger(new TestLogger(Output));
            Conn = new MockConnection();
        }

        public void Dispose()
        {
            Service.Shutdown();
            Conn.Dispose();
        }
    }
}
