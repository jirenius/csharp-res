using System;
using System.Collections.Generic;
using System.Text;

namespace ResgateIO.Service
{
    /// <summary>
    /// Will eventually provide details on the change of serving state.
    /// </summary>
    public class ServeEventArgs: EventArgs
    {
    }

    /// <summary>
    /// Provides details on an error that occurred within the service.
    /// </summary>
    public class ErrorEventArgs: EventArgs
    {
        /// <summary>
        /// Error message.
        /// </summary>
        public string Message { get; }

        public ErrorEventArgs(string message)
        {
            Message = message;
        }
    }
}
