using System;

namespace ResgateIO.Service
{
    /// <summary>
    /// Simple logger class that logs messages to the console.
    /// </summary>
    public class ConsoleLogger: ILogger
    {
        private readonly bool info;
        private readonly bool debug;
        private readonly bool error;
        private readonly bool trace;

        /// <summary>
        /// Initializes a new instance of the ConsoleLogger class, set to log all message except Debug.
        /// </summary>
        public ConsoleLogger() : this(LogLevels.Default) { }

        /// <summary>
        /// Initializes a new instance of the ConsoleLogger class.
        /// </summary>
        /// <param name="lvls">Flags determining what levels should be logged.</param>
        public ConsoleLogger(LogLevels lvls)
        {
            this.info = lvls.HasFlag(LogLevels.Info);
            this.debug = lvls.HasFlag(LogLevels.Debug);
            this.error = lvls.HasFlag(LogLevels.Error);
            this.trace = lvls.HasFlag(LogLevels.Trace);
        }

        /// <summary>
        /// Logs info message.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Info(string format, params object[] args)
        {
            if (info)
            {
                Console.WriteLine(timestamp() + " [INF] " + format, args);
            }
        }

        /// <summary>
        /// Logs debug message.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Debug(string format, params object[] args)
        {
            if (debug)
            {
                Console.WriteLine(timestamp() + " [DBG] " + format, args);
            }
        }

        /// <summary>
        /// Logs error message.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Error(string format, params object[] args)
        {
            if (error)
            {
                Console.WriteLine(timestamp() + " [ERR] " + format, args);
            }
        }

        /// <summary>
        /// Logs trace message.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        public void Trace(string format, params object[] args)
        {
            if (trace)
            {
                Console.WriteLine(timestamp() + " [TRC] " + format, args);
            }
        }

        private string timestamp()
        {
            return DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
        }
    }
}
