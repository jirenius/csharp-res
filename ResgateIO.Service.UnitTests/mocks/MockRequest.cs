using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NATS.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ResgateIO.Service
{
    public class MockRequest : IResourceContext, IAccessRequest, IGetRequest, ICallRequest, IAuthRequest, IModelRequest, ICollectionRequest
    {
        public class Call
        {
            public string Method { get; }
            public object[] Args { get; }

            public Call(string method, object[] args)
            {
                Method = method;
                Args = args;
            }
            public Call(string method) : this(method, new object[0]) { }
        }
        public List<Call> Calls = new List<Call>();
        
        public ResService Service { get; set; }

        public string ResourceName { get; set; }

        public IDictionary<string, string> PathParams { get; set; }

        public string Query { get; set; }

        public string Group { get; set; }

        public IDictionary Items { get; set; }

        public IResourceHandler Handler { get; set; }

        public string CID { get; set; }

        public JToken Token { get; set; }

        public bool ForValue { get; set; }

        public string Method { get; set; }

        public JToken Params { get; set; }

        public Dictionary<string, string[]> Header { get; set; }

        public string Host { get; set; }

        public string RemoteAddr { get; set; }

        public string URI { get; set; }

        public void Access(bool get, string call)
        {
            Calls.Add(new Call("Access", new object[] { get, call }));
        }

        public void AccessDenied()
        {
            Calls.Add(new Call("AccessDenied"));
        }

        public void AccessGranted()
        {
            Calls.Add(new Call("AccessGranted"));
        }

        public void AddEvent(object value, int idx)
        {
            Calls.Add(new Call("AddEvent", new object[] { value, idx }));
        }

        public void ChangeEvent(Dictionary<string, object> properties)
        {
            Calls.Add(new Call("ChangeEvent", new object[] { properties }));
        }

        public void Collection(object collection)
        {
            Calls.Add(new Call("Collection", new object[] { collection }));
        }

        public void Collection(object collection, string query)
        {
            Calls.Add(new Call("Collection", new object[] { collection, query }));
        }

        public void CreateEvent(object data)
        {
            Calls.Add(new Call("CreateEvent", new object[] { data }));
        }

        public void DeleteEvent()
        {
            Calls.Add(new Call("DeleteEvent"));
        }

        public void Error(ResError error)
        {
            Calls.Add(new Call("Error", new object[] { error }));
        }

        public void Event(string eventName)
        {
            Calls.Add(new Call("Event", new object[] { eventName }));
        }

        public void Event(string eventName, object payload)
        {
            Calls.Add(new Call("Event", new object[] { eventName, payload }));
        }

        public void InvalidParams()
        {
            Calls.Add(new Call("InvalidParams"));
        }

        public void InvalidParams(string message)
        {
            Calls.Add(new Call("InvalidParams", new object[] { message }));
        }

        public void InvalidParams(string message, object data)
        {
            Calls.Add(new Call("InvalidParams", new object[] { message, data }));
        }

        public void MethodNotFound()
        {
            Calls.Add(new Call("MethodNotFound"));
        }

        public void Model(object model)
        {
            Calls.Add(new Call("Model", new object[] { model }));
        }

        public void Model(object model, string query)
        {
            Calls.Add(new Call("Model", new object[] { model, query }));
        }

        public void NotFound()
        {
            Calls.Add(new Call("NotFound"));
        }

        public void Ok()
        {
            Calls.Add(new Call("Ok"));
        }

        public void Ok(object result)
        {
            Calls.Add(new Call("Ok", new object[] { result }));
        }

        public T ParseParams<T>()
        {
            Calls.Add(new Call("ParseParams"));
            return default(T);
        }

        public T ParseToken<T>()
        {
            Calls.Add(new Call("ParseToken"));
            return default(T);
        }

        public string PathParam(string key)
        {
            Calls.Add(new Call("Ok", new object[] { key }));
            return "mock";
        }

        public void QueryEvent(QueryCallback callback)
        {
            Calls.Add(new Call("QueryEvent"));
            callback(null);
        }

        public void ReaccessEvent()
        {
            Calls.Add(new Call("ReaccessEvent"));
        }

        public void RemoveEvent(int idx)
        {
            Calls.Add(new Call("RemoveEvent", new object[] { idx }));
        }

        public T RequireValue<T>() where T : class
        {
            Calls.Add(new Call("RequireValue"));
            return default(T);
        }

        public void Timeout(int milliseconds)
        {
            Calls.Add(new Call("Timeout", new object[] { milliseconds }));
        }

        public void Timeout(TimeSpan duration)
        {
            Calls.Add(new Call("Timeout", new object[] { duration }));
        }

        public void TokenEvent(object token)
        {
            Calls.Add(new Call("TokenEvent", new object[] { token }));
        }

        public T Value<T>() where T : class
        {
            Calls.Add(new Call("Value"));
            return default(T);
        }
    }
}
