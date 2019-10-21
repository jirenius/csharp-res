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


        /// <summary>
        /// Logs info message.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Info(string format, params object[] args)
        {
            log.WriteLine("[INF] " + format, args);
        }

        /// <summary>
        /// Logs debug message.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Debug(string format, params object[] args)
        {
            log.WriteLine("[DBG] " + format, args);
        }

        /// <summary>
        /// Logs error message.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Error(string format, params object[] args)
        {
            log.WriteLine("[ERR] " + format, args);
        }

        /// <summary>
        /// Logs trace message.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Trace(string format, params object[] args)
        {
            log.WriteLine("[TRC] " + format, args);
        }
    }
}
