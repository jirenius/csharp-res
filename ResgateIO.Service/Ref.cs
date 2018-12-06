using Newtonsoft.Json;

namespace ResgateIO.Service
{
    // Resource Reference
    public class Ref
    {
        [JsonProperty(PropertyName = "rid")]
        public string ResourceID;

        public Ref(string resourceID)
        {
            ResourceID = resourceID;
        }
    }
}
