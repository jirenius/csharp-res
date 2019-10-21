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
        /// <remarks>
        /// Will be null unless its value is set through <see cref="SetRevert"/>.
        /// </remarks>
        public object Data { get; private set; }

        /// <summary>
        /// Initializes a new instance of the DeleteEvent class.
        /// </summary>
        public DeleteEventArgs()
        {
            Data = null;
        }

        /// <summary>
        /// Sets the deleted resource data object,
        /// that can be used to revert the effects event.
        /// </summary>
        /// <param name="value">Resource data object.</param>
        /// <returns>This instance.</returns>
        public DeleteEventArgs SetRevert(object data)
        {
            Data = data;
            return this;
        }
    }
}