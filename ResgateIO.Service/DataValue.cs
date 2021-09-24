using Newtonsoft.Json;

namespace ResgateIO.Service
{
    /// <summary>
    /// Represents a data value that can be used for arbitrary data.
    /// </summary>
    /// <see>https://resgate.io/docs/specification/res-protocol/#data-values</see>
    public class DataValue
    {
        [JsonProperty(PropertyName = "data")]
        public object Data;

        /// <summary>
        /// Initializes a new instance of the DataValue class.
        /// </summary>
        /// <param name="data"></param>
        public DataValue(object data)
        {
            Data = data;
        }
    }
}
