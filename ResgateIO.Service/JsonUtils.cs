using Newtonsoft.Json;
using System.IO;
using System.Text;

namespace ResgateIO.Service
{
    internal static class JsonUtils
    {
        public static T Deserialize<T>(byte[] data) where T : class
        {
            using (var stream = new MemoryStream(data))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                return JsonSerializer.Create().Deserialize(reader, typeof(T)) as T;
        }

        public static byte[] Serialize(object item)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(item));
        }
    }
}
