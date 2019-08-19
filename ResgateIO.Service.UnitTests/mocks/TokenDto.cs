using Newtonsoft.Json;

namespace ResgateIO.Service.UnitTests
{
#pragma warning disable 0649 // These fields are assigned by JSON deserialization

    public class TokenDto
    {
        [JsonProperty(PropertyName = "id")]
        public int Id;

        [JsonProperty(PropertyName = "role")]
        public string Role;
    }

#pragma warning restore 0649
}
