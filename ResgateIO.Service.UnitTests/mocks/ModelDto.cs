using Newtonsoft.Json;

namespace ResgateIO.Service.UnitTests
{
#pragma warning disable 0649 // These fields are assigned by JSON deserialization

    public class ModelDto
    {
        [JsonProperty(PropertyName = "id")]
        public int Id;

        [JsonProperty(PropertyName = "foo")]
        public string Foo;
    }

#pragma warning restore 0649
}
