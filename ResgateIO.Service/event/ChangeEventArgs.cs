using System;
using System.Collections.Generic;

namespace ResgateIO.Service
{
    /// <summary>
    /// Provides information on a change event.
    /// </summary>
    public class ChangeEventArgs: EventArgs
    {
        /// <summary>
        /// Properties being changed, and their new values.
        /// The value will be ResAction.Delete for deleted properties.
        /// </summary>
        public Dictionary<string, object> ChangedProperties { get; }

        /// <summary>
        /// Properties being changed, and their old values.
        /// The value will be ResAction.Delete for new properties.        
        /// </summary>
        /// <remarks>
        /// Will be null unless its value is set through <see cref="SetRevert"/>.
        /// </remarks>
        public Dictionary<string, object> OldProperties { get; private set; }

        /// <summary>
        /// Initializes a new instance of the ChangeEvent class.
        /// </summary>
        public ChangeEventArgs(Dictionary<string, object> changes)
        {
            ChangedProperties = changes;
            OldProperties = null;
        }

        /// <summary>
        /// Set properties being changed, and their old values,
        /// that can be used to revert the effects event.
        /// The property value should be ResAction.Delete for new properties.
        /// </summary>
        /// <param name="oldProperties">Old property values.</param>
        /// <returns>This instance.</returns>
        public ChangeEventArgs SetRevert(Dictionary<string, object> oldProperties)
        {
            OldProperties = oldProperties;
            return this;
        }
    }
}