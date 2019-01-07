using Newtonsoft.Json;

namespace ResgateIO.Service
{
    internal class TokenEventDto
    {
        [JsonProperty(PropertyName = "token", NullValueHandling = NullValueHandling.Include)]
        public object Token;

        public TokenEventDto(object token)
        {
            Token = token;
        }
    }
}