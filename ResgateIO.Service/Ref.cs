using Newtonsoft.Json;

namespace ResgateIO.Service
{
    /// <summary>
    /// Represents a resource reference.
    /// </summary>
    /// <see>https://resgate.io/docs/specification/res-protocol/#values</see>
    public class Ref
    {
        [JsonProperty(PropertyName = "rid")]
        public string ResourceID;

        /// <summary>
        /// Initializes a new instance of the Ref class.
        /// </summary>
        /// <param name="resourceID"></param>
        public Ref(string resourceID)
        {
            ResourceID = resourceID;
        }
    }
}
