using Newtonsoft.Json;

namespace ResgateIO.Service.UnitTests
{
#pragma warning disable 0649 // These fields are assigned by JSON deserialization

    public class ParamsDto
    {
        [JsonProperty(PropertyName = "number")]
        public int Number;

        [JsonProperty(PropertyName = "text")]
        public string Text;
    }

#pragma warning restore 0649
}
