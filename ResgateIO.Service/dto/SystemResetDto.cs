using Newtonsoft.Json;

namespace ResgateIO.Service
{
    internal class SystemResetDto
    {
        [JsonProperty(PropertyName = "resources", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Resources;

        [JsonProperty(PropertyName = "access", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Access;

        public SystemResetDto(string[] resources, string[] access)
        {
            Resources = resources;
            Access = access;
        }
    }
}