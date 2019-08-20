using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xunit;

namespace ResgateIO.Service.UnitTests
{
    public class MockMsg
    {
        public readonly string Subject;
        public readonly byte[] Data;

        public MockMsg(string subject, byte[] data)
        {
            Subject = subject;
            Data = data;
        }

        /// <summary>
        /// Deserializes the data.
        /// </summary>
        /// <typeparam name="T">Type to deserialize the data into.</typeparam>
        /// <returns>Deserialized data object.</returns>
        public T Payload<T>() where T : class
        {
            if (Data == null || Data.Length == 0)
            {
                return default(T);
            }

            using (var stream = new MemoryStream(Data))
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                return JsonSerializer.Create().Deserialize(reader, typeof(T)) as T;
        }

        public MockMsg AssertSubject(string subject)
        {
            Assert.Equal(subject, Subject);
            return this;
        }

        public MockMsg AssertPayload(object payload)
        {
            Assert.NotEmpty(Data);
            string payloadJson = JsonConvert.SerializeObject(payload);
            string dataJson = Encoding.UTF8.GetString(Data);
            Assert.True(
                JToken.DeepEquals(JToken.Parse(dataJson), JToken.Parse(payloadJson)),
                String.Format("Payload mismatch:\nExpected:\n\t{0}\nActual:\n\t{1}", payloadJson, dataJson)
            );
            return this;
        }

        public MockMsg AssertPayload(byte[] payload)
        {
            Assert.Equal(payload, Data);
            return this;
        }

        public MockMsg AssertNoPayload()
        {
            Assert.Empty(Data);
            return this;
        }

        public MockMsg AssertPath(string path)
        {
            Assert.True(TryGetPath(path, out JToken o), "payload does not contain path: " + path);
            return this;
        }

        public MockMsg AssertNoPath(string path)
        {
            Assert.False(TryGetPath(path, out JToken o), "payload contains path: " + path);
            return this;
        }

        public MockMsg AssertPathPayload(string path, object payload)
        {
            Assert.True(TryGetPath(path, out JToken o), "payload does not contain path: " + path);
           
            string payloadJson = JsonConvert.SerializeObject(payload);
            Assert.True(
                JToken.DeepEquals(o, JToken.Parse(payloadJson)),
                String.Format("Expected:\n{0}\nActual:\n{1}", payloadJson, JsonConvert.SerializeObject(o))
            );
            return this;
        }

        public MockMsg AssertResult()
        {
            AssertNoPath("error");
            AssertPath("result");
            return this;
        }

        public MockMsg AssertResult(object result)
        {
            AssertNoPath("error");
            AssertPathPayload("result", result);
            return this;
        }

        public MockMsg AssertError()
        {
            AssertNoPath("result");
            AssertPath("error");
            return this;
        }

        public MockMsg AssertError(string code)
        {
            AssertNoPath("result");
            AssertPathPayload("error.code", code);
            return this;
        }

        public MockMsg AssertError(string code, string message)
        {
            AssertNoPath("result");
            AssertPathPayload("error.code", code);
            AssertPathPayload("error.message", message);
            return this;
        }

        public MockMsg AssertError(string code, string message, object data)
        {
            AssertNoPath("result");
            AssertPathPayload("error.code", code);
            AssertPathPayload("error.message", message);
            AssertPathPayload("error.data", data);
            return this;
        }

        public MockMsg AssertError(ResError err)
        {
            AssertNoPath("result");
            AssertPathPayload("error", err);
            return this;
        }

        public bool TryGetPath(string path, out JToken token)
        {
            token = null;
            if (Data == null || Data.Length == 0)
            {
                return false;
            }

            try
            {
                JToken o = JObject.Parse(Encoding.UTF8.GetString(Data));
                string[] parts = path.Split('.');
                foreach (string part in parts)
                {
                    if (!(o is JObject obj && obj.ContainsKey(part)))
                    {
                        return false;
                    }
                    o = o[part];
                }
                token = o;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
