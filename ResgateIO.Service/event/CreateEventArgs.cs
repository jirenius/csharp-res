using System;

namespace ResgateIO.Service
{
    /// <summary>
    /// Provides information on a create event.
    /// </summary>
    public class CreateEventArgs: EventArgs
    {
        /// <summary>
        /// The created resource data object.
        /// </summary>
        public object Data { get; }

        /// <summary>
        /// Initializes a new instance of the CreateEvent class.
        /// </summary>
        public CreateEventArgs(object data)
        {
            Data = data;
        }
    }
}