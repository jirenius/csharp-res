using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ResgateIO.Service.UnitTests
{
    public class RefTests
    {
        [Theory]
        // Valid RID
        [InlineData("test", true)]
        [InlineData("test.model", true)]
        [InlineData("test.model._hej_", true)]
        [InlineData("test.model.<strange", true)]
        [InlineData("test.model.23", true)]
        [InlineData("test.model.23?", true)]
        [InlineData("test.model.23?foo=bar", true)]
        [InlineData("test.model.23?foo=test.bar", true)]
        [InlineData("test.model.23?foo=*&?", true)]
        // Invalid RID
        [InlineData("", false)]
        [InlineData(".test", false)]
        [InlineData("test.", false)]
        [InlineData(".test.model", false)]
        [InlineData("test..model", false)]
        [InlineData("test.model.", false)]
        [InlineData("test\tmodel", false)]
        [InlineData("test\nmodel", false)]
        [InlineData("test\rmodel", false)]
        [InlineData("test model", false)]
        [InlineData("test\ufffdmodel", false)]
        [InlineData("täst.model", false)]
        [InlineData("test.*.model", false)]
        [InlineData("test.>.model", false)]
        [InlineData("test.model.>", false)]
        [InlineData("?foo=test.bar", false)]
        [InlineData(".test.model?foo=test.bar", false)]
        [InlineData("test..model?foo=test.bar", false)]
        [InlineData("test.model.?foo=test.bar", false)]
        [InlineData("test\tmodel?foo=test.bar", false)]
        [InlineData("test\nmodel?foo=test.bar", false)]
        [InlineData("test\rmodel?foo=test.bar", false)]
        [InlineData("test model?foo=test.bar", false)]
        [InlineData("test\ufffdmodel?foo=test.bar", false)]
        [InlineData("täst.model?foo=test.bar", false)]
        [InlineData("test.*.model?foo=test.bar", false)]
        [InlineData("test.>.model?foo=test.bar", false)]
        [InlineData("test.model.>?foo=test.bar", false)]
        public void IsValid_ReturnsCorrectValue(string rid, bool valid)
        {
            Ref r = new Ref(rid);
            Assert.Equal(valid, r.IsValid());
        }

        [Theory]
        [InlineData("test", "{\"rid\":\"test\"}")]
        [InlineData("test.model", "{\"rid\":\"test.model\"}")]
        [InlineData("test.model._hej_", "{\"rid\":\"test.model._hej_\"}")]
        [InlineData("test.model.<strange", "{\"rid\":\"test.model.<strange\"}")]
        [InlineData("test.model.23", "{\"rid\":\"test.model.23\"}")]
        [InlineData("test.model.23?", "{\"rid\":\"test.model.23?\"}")]
        [InlineData("test.model.23?foo=bar", "{\"rid\":\"test.model.23?foo=bar\"}")]
        [InlineData("test.model.23?foo=test.bar", "{\"rid\":\"test.model.23?foo=test.bar\"}")]
        [InlineData("test.model.23?foo=*&?", "{\"rid\":\"test.model.23?foo=*&?\"}")]
        public void SerializeRef_SerializesToCorrectJson(string rid, string json)
        {
            Ref r = new Ref(rid);
            Test.AssertJsonEqual(JToken.Parse(json), JToken.Parse(JsonConvert.SerializeObject(r)));
        }

        [Theory]
        // Valid RID
        [InlineData("{\"rid\":\"test\"}", "test")]
        [InlineData("{\"rid\":\"test.model\"}", "test.model")]
        [InlineData("{\"rid\":\"test.model._hej_\"}", "test.model._hej_")]
        [InlineData("{\"rid\":\"test.model.<strange\"}", "test.model.<strange")]
        [InlineData("{\"rid\":\"test.model.23\"}", "test.model.23")]
        [InlineData("{\"rid\":\"test.model.23?\"}", "test.model.23?")]
        [InlineData("{\"rid\":\"test.model.23?foo=bar\"}", "test.model.23?foo=bar")]
        [InlineData("{\"rid\":\"test.model.23?foo=test.bar\"}", "test.model.23?foo=test.bar")]
        [InlineData("{\"rid\":\"test.model.23?foo=*&?\"}", "test.model.23?foo=*&?")]
        // Valid JSON but marshals to empty or null
        [InlineData("{\"rid\":\"\"}", "")]
        [InlineData("{\"foo\":\"bar\"}", null)]
        [InlineData("{}", null)]
        [InlineData("{\"rid\": null}", null)]
        public void DeserializeRef_ValidJsonObject_DeserializesToCorrectResourceID(string json, string resourceID)
        {
            Ref r = JsonConvert.DeserializeObject<Ref>(json);
            Assert.Equal(resourceID, r.ResourceID);
        }
    }
}
