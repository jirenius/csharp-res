using Newtonsoft.Json;

namespace ResgateIO.Service
{
    internal class AccessDto
    {
        [JsonProperty(PropertyName = "get")]
        public bool Get;

        [JsonProperty(PropertyName = "call", NullValueHandling = NullValueHandling.Ignore)]
        public string Call;

        public AccessDto(bool get, string call)
        {
            Get = get;
            Call = call;
        }
    }
}