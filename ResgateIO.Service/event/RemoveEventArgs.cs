using System;

namespace ResgateIO.Service
{
    /// <summary>
    /// Provides information on a remove event.
    /// </summary>
    public class RemoveEventArgs: EventArgs
    {
        /// <summary>
        /// Value that was removed.
        /// </summary>
        public object Value { get; }

        /// <summary>
        /// Index position where the value was removed from.
        /// </summary>
        public int Idx { get; }

        /// <summary>
        /// Initializes a new instance of the RemoveEvent class.
        /// </summary>
        public RemoveEventArgs(object value, int idx)
        {
            Value = value;
            Idx = idx;
        }
    }
}