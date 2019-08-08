using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NATS.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ResgateIO.Service
{
    internal class ValueGetRequest: IGetRequest
    {
        public object ValueResult { get; private set; }
        public ResError ErrorResult { get; private set; }

        private bool replied = false;        
        private ResourceContext resource;

        public ValueGetRequest(
            ResourceContext resource)
        {
            this.resource = resource;
        }

        // Expose IResourceContext properties and methods
        public ResService Service { get { return resource.Service; } }
        public string ResourceName { get { return resource.ResourceName; } }
        public ResourceType ResourceType { get { return resource.ResourceType; } }
        public IDictionary<string, string> PathParams { get { return resource.PathParams; } }
        public string PathParam(string key) { return resource.PathParam(key); }
        public string Query { get { return resource.Query; } }
        public IDictionary Items { get { return resource.Items; }  }
        public T Value<T>() { throw new InvalidOperationException("Value called within get request handler"); }
        public T RequireValue<T>() { throw new InvalidOperationException("RequireValue called within get request handler"); }
        public void Event(string eventName, object payload) { resource.Event(eventName, payload); }
        public void ChangeEvent(Dictionary<string, object> properties) { resource.ChangeEvent(properties); }
        public void AddEvent(object value, int idx) { resource.AddEvent(value, idx); }
        public void RemoveEvent(int idx) { resource.RemoveEvent(idx); }
        public void ReaccessEvent() { resource.ReaccessEvent(); }
        public void QueryEvent(QueryCallBack callback) { resource.QueryEvent(callback); }
        public void CreateEvent(object data) { resource.CreateEvent(data); }
        public void DeleteEvent() { resource.DeleteEvent(); }

        public void Error(ResError error)
        {
            reply();
            ErrorResult = error;
        }

        public void NotFound()
        {
            Error(new ResError(ResError.CodeNotFound, "Not found"));
        }

        public void Model(object model)
        {
            Model(model, null);
        }

        public void Model(object model, string query)
        {
            if (!String.IsNullOrEmpty(query) && String.IsNullOrEmpty(Query))
            {
                throw new ArgumentException("Query model response on non-query request");
            }
            reply();
            ValueResult = model;
        }

        public void Collection(object collection)
        {
            Collection(collection, null);
        }

        public void Collection(object collection, string query)
        {
            if (!String.IsNullOrEmpty(query) && String.IsNullOrEmpty(Query))
            {
                throw new ArgumentException("Query collection response on non-query request");
            }
            reply();
            ValueResult = collection;
        }

        public void Timeout(int milliseconds)
        {
            // Implement when an internal timeout for requests is implemented
        }

        public void Timeout(TimeSpan duration)
        {
            Timeout((int)duration.TotalMilliseconds);
        }

        public bool ForValue { get { return true; } }

        internal void ExecuteHandler()
        {
            try
            {
                if (resource.Handler is IModelHandler modelHandler)
                {
                    modelHandler.Get((IModelRequest)this);
                }
                else
                {
                    if (!(resource.Handler is ICollectionHandler collectionHandler))
                    {
                        return;
                    }
                    collectionHandler.Get((ICollectionRequest)this);
                }

                if (!replied)
                {
                    ErrorResult = new ResError(ResError.CodeInternalError, "Missing response on get request for " + ResourceName);
                }
            }
            catch (ResException ex)
            {
                if (replied)
                {
                    Console.WriteLine("Error in value get request for {0}: {1} - {2}", ResourceName, ex.Code, ex.Message);
                }
                else
                {
                    ErrorResult = new ResError(ex);
                    return;
                }

            }
            catch (Exception ex)
            {
                if (replied)
                {
                    Console.WriteLine("Error in value get request for {0}: {1}", ResourceName, ex.Message);
                }
                else
                {
                    ErrorResult = new ResError(ex);
                }
                
            }
        }

        private void reply()
        {
            if (replied)
            {
                throw new InvalidOperationException("Response already sent on request");
            }
            replied = true;
        }
    }
}
