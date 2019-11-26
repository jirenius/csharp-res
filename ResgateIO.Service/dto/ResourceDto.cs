using Newtonsoft.Json;

namespace ResgateIO.Service
{
    internal class ResourceDto
    {
        [JsonProperty(PropertyName = "resource", NullValueHandling = NullValueHandling.Include)]
        public Ref Resource;

        public ResourceDto(Ref resource)
        {
            Resource = resource;
        }
    }
}