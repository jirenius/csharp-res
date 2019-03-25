using Newtonsoft.Json;
using System.Collections.Generic;

namespace ResgateIO.Service
{
    internal class ChangeEventDto
    {
        [JsonProperty(PropertyName = "values")]
        public Dictionary<string, object> Values;

        public ChangeEventDto(Dictionary<string, object> values)
        {
            Values = values;
        }
    }
}