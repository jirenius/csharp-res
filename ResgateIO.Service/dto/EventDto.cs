using Newtonsoft.Json;

namespace ResgateIO.Service
{
    internal class EventDto
    {
        [JsonProperty(PropertyName = "event")]
        public string Event;

        [JsonProperty(PropertyName = "data")]
        public object Data;

        public EventDto(string eventName, object data)
        {
            Event = eventName;
            Data = data;
        }
    }
}