using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Text;
using Xunit;

namespace ResgateIO.Service.UnitTests
{
    public class DataValueTests
    {

        [Theory]
        [InlineData(null, "{\"data\":null}")]
        [InlineData("test", "{\"data\":\"test\"}")]
        [InlineData(42, "{\"data\":42}")]
        [InlineData(1.2, "{\"data\":1.2}")]
        [InlineData(true, "{\"data\":true}")]
        [InlineData(new string[] { "foo", "bar" }, "{\"data\":[\"foo\",\"bar\"]}")]
        public void SerializeDataValue_SerializesToCorrectJson(object data, string json)
        {
            DataValue v = new DataValue(data);
            Test.AssertJsonEqual(JToken.Parse(json), JToken.Parse(JsonConvert.SerializeObject(v)));
        }
    }
}
