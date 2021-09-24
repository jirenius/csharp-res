using Newtonsoft.Json;

namespace ResgateIO.Service
{
    /// <summary>
    /// Represents a resource reference.
    /// </summary>
    /// <see>https://resgate.io/docs/specification/res-protocol/#resource-references</see>
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

        public bool IsValid()
        {
            bool start = true;
            foreach (char c in ResourceID)
            {
                if (c == '?')
                {
                    return !start;
                }

                if (c < 33 || c > 126 || c == '*' || c == '>')
                {
                    return false;
                }

                if (c == '.')
                {
                    if (start)
                    {
                        return false;
                    }
                    start = true;
                }
                else
                {
                    start = false;
                }
            }
            return !start;
        }
    }
}
