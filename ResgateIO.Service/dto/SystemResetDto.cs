using Newtonsoft.Json;

namespace ResgateIO.Service
{
    internal class SystemResetDto
    {
        [JsonProperty(PropertyName = "resources")]
        public string Resources;

        [JsonProperty(PropertyName = "access")]
        public string Access;

        public SystemResetDto(string resources, string access)
        {
            Resources = resources;
            Access = access;
        }
    }
}