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
        public Dictionary<string, object> Changes { get; }

        /// <summary>
        /// Properties being changed, and their old values.
        /// The value will be ResAction.Delete for new properties.
        /// </summary>
        public Dictionary<string, object> Revert { get; }

        /// <summary>
        /// Initializes a new instance of the ChangeEvent class.
        /// </summary>
        public ChangeEventArgs(Dictionary<string, object> changes, Dictionary<string, object> revert)
        {
            Changes = changes;
            Revert = revert;
        }
    }
}