using Newtonsoft.Json;
using System;

namespace ResgateIO.Service
{
    internal class CollectionDto
    {
        [JsonProperty(PropertyName = "collection")]
        public object Collection;

        [JsonProperty(PropertyName = "query", NullValueHandling = NullValueHandling.Ignore)]
        public string Query;

        public CollectionDto(object collection, string query)
        {
            Collection = collection;
            Query = String.IsNullOrEmpty(query) ? null : query;
        }
    }
}