using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using NATS.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ResgateIO.Service
{
    internal class ValueGetRequest: ResourceDecorator, IGetRequest, IModelRequest, ICollectionRequest
    {
        public object ValueResult { get; private set; }
        public ResError ErrorResult { get; private set; }

        private ILogger Log { get { return Service.Log; } }
        private bool replied = false;

        public ValueGetRequest(IResourceContext resource)
            :base(resource)
        {
        }

        public override T Value<T>()
        {
            throw new InvalidOperationException("Value called within get request handler");
        }

        public override T RequireValue<T>()
        {
            throw new InvalidOperationException("RequireValue called within get request handler");
        }

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
                if (Handler is IModelHandler modelHandler)
                {
                    modelHandler.Get(this);
                }
                else if (Handler is ICollectionHandler collectionHandler)
                {
                    collectionHandler.Get(this);
                }
                else if (Handler is IGetHandler getHandler)
                {
                    getHandler.Get(this);
                }
            }
            catch (ResException ex)
            {
                if (replied)
                {
                    Log.Error(String.Format("Error in value get request for {0}: {1} - {2}", ResourceName, ex.Code, ex.Message));
                }
                else
                {
                    ErrorResult = new ResError(ex);
                }
            }
            catch (Exception ex)
            {
                if (!replied)
                {
                    ErrorResult = new ResError(ex);
                }
                // Log error as only ResExceptions are considered valid behaviour
                Log.Error(String.Format("Error in value get request for {0}: {1}", ResourceName, ex.Message));
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
