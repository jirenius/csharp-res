using Newtonsoft.Json;
using System.Collections.Generic;

namespace ResgateIO.Service
{
    internal class QueryResponseDto
    {
        [JsonProperty(PropertyName = "events")]
        public IList<EventDto> Events;

        public QueryResponseDto(IList<EventDto> events)
        {
            Events = events;
        }
    }
}
