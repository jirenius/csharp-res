namespace ResgateIO.Service
{
    /// <summary>
    /// Defines a class that provides methods for logging messages.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Logs service state, such as connects and reconnects to NATS.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        void Info(string format, params object[] args);

        /// <summary>
        /// Logs debug information such as startup and cleanup steps.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        void Debug(string format, params object[] args);

        /// <summary>
        /// Logs errors in the service, or incoming messages not complying with the RES protocol.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        void Error(string format, params object[] args);

        /// <summary>
        /// Logs all network traffic going to and from the service.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An object array that contains zero or more objects to format.</param>
        void Trace(string format, params object[] args);
    }
}
