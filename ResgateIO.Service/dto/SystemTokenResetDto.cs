using Newtonsoft.Json;

namespace ResgateIO.Service
{
    internal class SystemTokenResetDto
    {
        [JsonProperty(PropertyName = "tids", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Tids;

        [JsonProperty(PropertyName = "subject", NullValueHandling = NullValueHandling.Ignore)]
        public string Subject;

        public SystemTokenResetDto(string subject, string[] tids)
        {
            Subject = subject;
            Tids = tids;
        }
    }
}