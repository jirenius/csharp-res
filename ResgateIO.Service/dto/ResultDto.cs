using Newtonsoft.Json;

namespace ResgateIO.Service
{
    internal class ResultDto
    {
        [JsonProperty(PropertyName = "result", NullValueHandling = NullValueHandling.Include)]
        public object Result;

        public ResultDto(object result)
        {
            Result = result;
        }
    }
}