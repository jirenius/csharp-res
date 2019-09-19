using System;

namespace ResgateIO.Service
{
    /// <summary>
    /// Provides information on a delete event.
    /// </summary>
    public class DeleteEventArgs: EventArgs
    {
        /// <summary>
        /// The deleted resource data object.
        /// </summary>
        public object Data { get; set;  }

        /// <summary>
        /// Initializes a new instance of the DeleteEvent class.
        /// </summary>
        public DeleteEventArgs(object data)
        {
            Data = data;
        }
    }
}