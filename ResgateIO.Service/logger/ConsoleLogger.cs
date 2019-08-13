using System;

namespace ResgateIO.Service
{
    /// <summary>
    /// Simple logger class that logs messages to the console.
    /// </summary>
    public class ConsoleLogger: ILogger
    {
        private readonly bool info = true;
        private readonly bool error = true;
        private readonly bool trace = true;

        /// <summary>
        /// Initializes a new instance of the ConsoleLogger class, set to log all message.
        /// </summary>
        public ConsoleLogger()
        {
        }

        /// <summary>
        /// Initializes a new instance of the ConsoleLogger class.
        /// </summary>
        /// <param name="info">Flag determining if info messages should be logged.</param>
        /// <param name="error">Flag determining if error messages should be logged.</param>
        /// <param name="trace">Flag determining if trace messages should be logged.</param>
        public ConsoleLogger(bool info, bool error, bool trace)
        {
            this.info = info;
            this.error = error;
            this.trace = trace;
        }

        /// <summary>
        /// Logs info message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Info(string message)
        {
            if (info)
            {
                Console.WriteLine("[INFO ] " + message);
            }
        }

        /// <summary>
        /// Logs error message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Error(string message)
        {
            if (error)
            {
                Console.WriteLine("[ERROR] " + message);
            }
        }

        /// <summary>
        /// Logs trace message.
        /// </summary>
        /// <param name="message">Message to log.</param>
        public void Trace(string message)
        {
            if (trace)
            {
                Console.WriteLine("[TRACE] " + message);
            }
        }
    }
}
