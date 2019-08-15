using Newtonsoft.Json;

namespace ResgateIO.Service
{
    internal class QueryEventDto
    {
        [JsonProperty(PropertyName = "subject")]
        public string Subject;

        public QueryEventDto(string subject)
        {
            Subject = subject;
        }
    }
}