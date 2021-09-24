using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ResgateIO.Service.UnitTests
{
    public class SoftRefTests
    {
        [Theory]
        [InlineData("test", "{\"rid\":\"test\",\"soft\":true}")]
        [InlineData("test.model", "{\"rid\":\"test.model\",\"soft\":true}")]
        [InlineData("test.model._hej_", "{\"rid\":\"test.model._hej_\",\"soft\":true}")]
        [InlineData("test.model.<strange", "{\"rid\":\"test.model.<strange\",\"soft\":true}")]
        [InlineData("test.model.23", "{\"rid\":\"test.model.23\",\"soft\":true}")]
        [InlineData("test.model.23?", "{\"rid\":\"test.model.23?\",\"soft\":true}")]
        [InlineData("test.model.23?foo=bar", "{\"rid\":\"test.model.23?foo=bar\",\"soft\":true}")]
        [InlineData("test.model.23?foo=test.bar", "{\"rid\":\"test.model.23?foo=test.bar\",\"soft\":true}")]
        [InlineData("test.model.23?foo=*&?", "{\"rid\":\"test.model.23?foo=*&?\",\"soft\":true}")]
        public void SerializeSoftRef_SerializesToCorrectJson(string rid, string json)
        {
            SoftRef r = new SoftRef(rid);
            Test.AssertJsonEqual(JToken.Parse(json), JToken.Parse(JsonConvert.SerializeObject(r)));
        }
    }
}
