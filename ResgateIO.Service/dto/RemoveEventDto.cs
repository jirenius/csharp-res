using Newtonsoft.Json;

namespace ResgateIO.Service
{
    internal class RemoveEventDto
    {
        [JsonProperty(PropertyName = "idx")]
        public int Idx;

        public RemoveEventDto(int idx)
        {
            Idx = idx;
        }
    }
}