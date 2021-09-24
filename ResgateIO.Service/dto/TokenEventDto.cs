using Newtonsoft.Json;

namespace ResgateIO.Service
{
    internal class TokenEventDto
    {
        [JsonProperty(PropertyName = "token", NullValueHandling = NullValueHandling.Include)]
        public object Token;

        [JsonProperty(PropertyName = "tid", NullValueHandling = NullValueHandling.Ignore)]
        public string Tid;

        public TokenEventDto(object token)
        {
            Token = token;
            Tid = null;
        }

        public TokenEventDto(object token, string tid)
        {
            Token = token;
            Tid = tid;
        }
    }
}