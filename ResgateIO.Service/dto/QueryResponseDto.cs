using Newtonsoft.Json;
using System.Collections.Generic;

namespace ResgateIO.Service
{
#pragma warning disable 0649 // These fields are assigned by JSON deserialization

    internal class QueryResponseDto
    {
        [JsonProperty(PropertyName = "events")]
        public IList<EventDto> Events;

        public QueryResponseDto(IList<EventDto> events)
        {
            Events = events;
        }
    }

#pragma warning restore 0649
}
