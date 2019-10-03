using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
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
        public virtual string Query { get { return resource.Query; } }
        public string Group { get { return resource.Group; } }
        public IDictionary Items { get { return resource.Items; } }
        public IResourceHandler Handler { get { return resource.Handler; } }
        public virtual T Value<T>() where T : class { return resource.Value<T>(); }
        public virtual T RequireValue<T>() where T : class { return resource.RequireValue<T>(); }
        public void Event(string eventName) { resource.Event(eventName); }
        public void Event(string eventName, object payload) { resource.Event(eventName, payload); }
        public virtual void ChangeEvent(Dictionary<string, object> properties) { resource.ChangeEvent(properties); }
        public virtual void AddEvent(object value, int idx) { resource.AddEvent(value, idx); }
        public virtual void RemoveEvent(int idx) { resource.RemoveEvent(idx); }
        public void ReaccessEvent() { resource.ReaccessEvent(); }
        public void ResetEvent() { resource.ResetEvent(); }
        public void QueryEvent(QueryCallback callback) { resource.QueryEvent(callback); }
        public void CreateEvent(object data) { resource.CreateEvent(data); }
        public void DeleteEvent() { resource.DeleteEvent(); }
    }
}
