using System;
using Xunit.Abstractions;

namespace ResgateIO.Service.UnitTests
{
    public class TestLogger : ILogger
    {
        private readonly ITestOutputHelper log;

        public TestLogger(ITestOutputHelper output)
        {
            log = output;
        }

        public void Info(string message)
        {
            log.WriteLine("[INFO ] " + message);
        }

        public void Error(string message)
        {
            log.WriteLine("[ERROR] " + message);
        }

        public void Trace(string message)
        {
            log.WriteLine("[TRACE] " + message);
        }
    }
}
