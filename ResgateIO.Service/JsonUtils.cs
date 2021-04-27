using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Text;

namespace ResgateIO.Service
{
    internal static class JsonUtils
    {
        public static T Deserialize<T>(byte[] data) where T : class
        {
            return JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(data));
        }

        public static byte[] Serialize(object item)
        {
            DefaultContractResolver contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(item, new JsonSerializerSettings
            {
                ContractResolver = contractResolver,
                Formatting = Formatting.Indented
            }));
        }
    }
}
