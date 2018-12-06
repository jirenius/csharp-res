using Newtonsoft.Json;

namespace ResgateIO.Service
{
    internal class AddEventDto
    {
        [JsonProperty(PropertyName = "value")]
        public object Value;

        [JsonProperty(PropertyName = "idx")]
        public int Idx;

        public AddEventDto(object value, int idx)
        {
            Value = value;
            Idx = idx;
        }
    }
}