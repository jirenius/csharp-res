using Newtonsoft.Json;

namespace ResgateIO.Service
{
#pragma warning disable 0649 // These fields are assigned by JSON deserialization

    internal class QueryRequestDto
    {
        [JsonProperty(PropertyName = "query")]
        public string Query;
    }

#pragma warning restore 0649
}
