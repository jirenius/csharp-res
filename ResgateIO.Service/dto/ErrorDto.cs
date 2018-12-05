using Newtonsoft.Json;

namespace ResgateIO.Service
{
    internal class ErrorDto
    {
        [JsonProperty(PropertyName = "error")]
        public ResError Error;

        public ErrorDto(ResError error)
        {
            Error = error;
        }
    }
}