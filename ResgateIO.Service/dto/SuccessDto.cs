using Newtonsoft.Json;

namespace ResgateIO.Service
{
    internal class SuccessDto
    {
        [JsonProperty(PropertyName = "result", NullValueHandling = NullValueHandling.Include)]
        public object Result;

        public SuccessDto(object result)
        {
            Result = result;
        }
    }
}