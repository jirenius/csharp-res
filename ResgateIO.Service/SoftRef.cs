using Newtonsoft.Json;
using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace ResgateIO.Service
{
    /// <summary>
    /// Represents a soft resource reference.
    /// </summary>
    /// <see>https://resgate.io/docs/specification/res-protocol/#resource-references</see>
    [Serializable]
    public class SoftRef: Ref, ISerializable
    {
        /// <summary>
        /// Initializes a new instance of the SoftRef class.
        /// </summary>
        /// <param name="resourceID">Resource ID</param>
        public SoftRef(string resourceID): base(resourceID)
        {
        }

        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("rid", this.ResourceID);
            info.AddValue("soft", true);
        }
    }
}
