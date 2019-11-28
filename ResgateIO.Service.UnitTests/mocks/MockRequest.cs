using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace ResgateIO.Service
{
#pragma warning disable 618
    public class MockRequest : IRequest, IAccessRequest, IGetRequest, ICallRequest, IAuthRequest, IModelRequest, ICollectionRequest, INewRequest
#pragma warning restore 618
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

        private static readonly Task completedTask = Task.FromResult(false);

        public List<Call> Calls = new List<Call>();
        
        public ResService Service { get; set; }

        public RequestType Type { get; set; }

        public string ResourceName { get; set; }

        public IDictionary<string, string> PathParams { get; set; }

        public string Query { get; set; }

        public string Group { get; set; }

        public IDictionary Items { get; set; }

        public IAsyncHandler Handler { get; set; }

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

        public Task AddEventAsync(object value, int idx)
        {
            Calls.Add(new Call("AddEventAsync", new object[] { value, idx }));
            return completedTask;
        }

        public void ChangeEvent(Dictionary<string, object> properties)
        {
            Calls.Add(new Call("ChangeEvent", new object[] { properties }));
        }

        public Task ChangeEventAsync(Dictionary<string, object> properties)
        {
            Calls.Add(new Call("ChangeEvent", new object[] { properties }));
            return completedTask;
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

        public Task CreateEventAsync(object data)
        {
            Calls.Add(new Call("CreateEvent", new object[] { data }));
            return completedTask;
        }

        public void DeleteEvent()
        {
            Calls.Add(new Call("DeleteEvent"));
        }

        public Task DeleteEventAsync()
        {
            Calls.Add(new Call("DeleteEvent"));
            return completedTask;
        }

        public void Error(ResError error)
        {
            Calls.Add(new Call("Error", new object[] { error }));
        }

        public void Event(string eventName)
        {
            Calls.Add(new Call("Event", new object[] { eventName }));
        }

        public Task EventAsync(string eventName)
        {
            Calls.Add(new Call("EventAsync", new object[] { eventName }));
            return completedTask;
        }

        public void Event(string eventName, object payload)
        {
            Calls.Add(new Call("Event", new object[] { eventName, payload }));
        }

        public Task EventAsync(string eventName, object payload)
        {
            Calls.Add(new Call("Event", new object[] { eventName, payload }));
            return completedTask;
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

        public void InvalidQuery()
        {
            Calls.Add(new Call("InvalidQuery"));
        }

        public void InvalidQuery(string message)
        {
            Calls.Add(new Call("InvalidQuery", new object[] { message }));
        }

        public void InvalidQuery(string message, object data)
        {
            Calls.Add(new Call("InvalidQuery", new object[] { message, data }));
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
        
        public void Resource(string resourceID)
        {
            Calls.Add(new Call("Resource", new object[] { resourceID }));
        }

        public void New(Ref rid)
        {
            Calls.Add(new Call("New", new object[] { rid }));
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

        public void QueryEvent(Func<IQueryRequest, Task> callback)
        {
            Calls.Add(new Call("QueryEvent(Func<IQueryRequest, Task>)"));
            callback(null);
        }

        public void QueryEvent(Action<IQueryRequest> callback)
        {
            Calls.Add(new Call("QueryEvent(Action<IQueryRequest>)"));
            callback(null);
        }

        public void ReaccessEvent()
        {
            Calls.Add(new Call("ReaccessEvent"));
        }

        public void ResetEvent()
        {
            Calls.Add(new Call("ReaccessEvent"));
        }

        public void RemoveEvent(int idx)
        {
            Calls.Add(new Call("RemoveEvent", new object[] { idx }));
        }

        public Task RemoveEventAsync(int idx)
        {
            Calls.Add(new Call("RemoveEvent", new object[] { idx }));
            return completedTask;
        }

        public T RequireValue<T>() where T : class
        {
            Calls.Add(new Call("RequireValue"));
            return default(T);
        }

        public Task<T> RequireValueAsync<T>() where T : class
        {
            Calls.Add(new Call("RequireValueAsync"));
            return Task.FromResult(default(T));
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

        public Task<T> ValueAsync<T>() where T : class
        {
            Calls.Add(new Call("ValueAsync"));
            return Task.FromResult(default(T));
        }

        public void RawResponse(byte[] data)
        {
            Calls.Add(new Call("RawResponse", new object[] { data }));
        }

        public IResourceContext CloneWithQuery(string query)
        {
            Calls.Add(new Call("CloneWithQuery", new object[] { query }));
            return null;
        }
    }
}
