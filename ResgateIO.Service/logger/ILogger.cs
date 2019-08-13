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
        /// <param name="message">Message to log.</param>
        void Info(string message);

        /// <summary>
        /// Logs errors in the service, or incoming messages not complying with the RES protocol.
        /// </summary>
        /// <param name="message">Message to log.</param>
        void Error(string message);

        /// <summary>
        /// Logs all network traffic going to and from the service.
        /// </summary>
        /// <param name="message">Message to log.</param>
        void Trace(string message);
    }
}
