using System;

namespace ResgateIO.Service
{
    /// <summary>
    /// Provides information on an add event.
    /// </summary>
    public class AddEventArgs: EventArgs
    {
        /// <summary>
        /// Value that was added.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Index position where the value is added.
        /// </summary>
        public int Idx { get; }

        /// <summary>
        /// Initializes a new instance of the AddEvent class.
        /// </summary>
        public AddEventArgs(object value, int idx)
        {
            Value = value;
            Idx = idx;
        }
    }
}