using System;

namespace ResgateIO.Service
{
    /// <summary>
    /// Provides information on a custom event.
    /// </summary>
    public class CustomEventArgs: EventArgs
    {
        /// <summary>
        /// Name of the custom event.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The payload of the custom event.
        /// </summary>
        public object Payload { get; }

        /// <summary>
        /// Initializes a new instance of the CustomEvent class.
        /// </summary>
        public CustomEventArgs(string name, object payload)
        {
            Name = name;
            Payload = payload;
        }
    }
}