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
        /// <remarks>
        /// Will be null unless its value is set through <see cref="SetRevert"/>.
        /// </remarks>
        public object Value { get; private set; }

        /// <summary>
        /// Index position where the value was removed from.
        /// </summary>
        public int Idx { get; }

        /// <summary>
        /// Initializes a new instance of the RemoveEvent class.
        /// </summary>
        public RemoveEventArgs(int idx)
        {
            Idx = idx;
            Value = null;
        }

        /// <summary>
        /// Value that was removed,
        /// that can be used to revert the effects event.
        /// </summary>
        /// <param name="value">Removed value.</param>
        /// <returns>This instance.</returns>
        public RemoveEventArgs SetRevert(object value)
        {
            Value = value;
            return this;
        }
    }
}