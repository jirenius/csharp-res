using Newtonsoft.Json;
using System;

namespace ResgateIO.Service
{
    internal class ModelDto
    {
        [JsonProperty(PropertyName = "model")]
        public object Model;

        [JsonProperty(PropertyName = "query", NullValueHandling = NullValueHandling.Ignore)]
        public string Query;

        public ModelDto(object model, string query)
        {
            Model = model;
            Query = String.IsNullOrEmpty(query) ? null : query;
        }
    }
}