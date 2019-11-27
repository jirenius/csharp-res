using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using NATS.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ResgateIO.Service
{
    internal abstract class ResourceDecorator: IResourceContext
    {
        private IResourceContext resource;

        public ResourceDecorator(
            IResourceContext resource)
        {
            this.resource = resource;
        }

        public ResService Service { get { return resource.Service; } }
        public string ResourceName { get { return resource.ResourceName; } }
        public IDictionary<string, string> PathParams { get { return resource.PathParams; } }
        public string PathParam(string key) { return resource.PathParam(key); }
        public string Query { get { return resource.Query; } }
        public string Group { get { return resource.Group; } }
        public IDictionary Items { get { return resource.Items; } }
        public IAsyncHandler Handler { get { return resource.Handler; } }
        public virtual T Value<T>() where T : class { return resource.Value<T>(); }
        public virtual Task<T> ValueAsync<T>() where T : class { return resource.ValueAsync<T>(); }
        public virtual T RequireValue<T>() where T : class { return resource.RequireValue<T>(); }
        public virtual Task<T> RequireValueAsync<T>() where T : class { return resource.RequireValueAsync<T>(); }
        public void Event(string eventName) { resource.Event(eventName); }
        public Task EventAsync(string eventName) { return resource.EventAsync(eventName); }
        public void Event(string eventName, object payload) { resource.Event(eventName, payload); }
        public Task EventAsync(string eventName, object payload) { return resource.EventAsync(eventName, payload); }
        public virtual void ChangeEvent(Dictionary<string, object> properties) { resource.ChangeEvent(properties); }
        public virtual Task ChangeEventAsync(Dictionary<string, object> properties) { return resource.ChangeEventAsync(properties); }
        public virtual void AddEvent(object value, int idx) { resource.AddEvent(value, idx); }
        public virtual Task AddEventAsync(object value, int idx) { return resource.AddEventAsync(value, idx); }
        public virtual void RemoveEvent(int idx) { resource.RemoveEvent(idx); }
        public virtual Task RemoveEventAsync(int idx) { return resource.RemoveEventAsync(idx); }
        public void CreateEvent(object data) { resource.CreateEvent(data); }
        public Task CreateEventAsync(object data) { return resource.CreateEventAsync(data); }
        public void DeleteEvent() { resource.DeleteEvent(); }
        public Task DeleteEventAsync() { return resource.DeleteEventAsync(); }
        public void ReaccessEvent() { resource.ReaccessEvent(); }
        public void ResetEvent() { resource.ResetEvent(); }
        public void QueryEvent(Func<IQueryRequest, Task> callback) { resource.QueryEvent(callback); }
        public void QueryEvent(Action<IQueryRequest> callback) { resource.QueryEvent(callback); }
        public IResourceContext CloneWithQuery(string query) { return resource.CloneWithQuery(query); }
    }
}
