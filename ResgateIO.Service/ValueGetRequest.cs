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
            Error(ResError.NotFound);
        }

        /// <summary>
        /// Sends a system.invalidQuery response with a default error message.
        /// </summary>
        public void InvalidQuery()
        {
            Error(ResError.InvalidQuery);
        }

        /// <summary>
        /// Sends a system.invalidQuery response with a custom error message.
        /// </summary>
        /// <param name="message">Error message.</param>
        public void InvalidQuery(string message)
        {
            Error(new ResError(ResError.CodeInvalidQuery, message));
        }

        /// <summary>
        /// Sends a system.invalidQuery response with a custom error message and data.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="data">Additional data. Must be JSON serializable.</param>
        public void InvalidQuery(string message, object data)
        {
            Error(new ResError(ResError.CodeInvalidQuery, message, data));
        }

        public void Model(object model)
        {
            Model(model, null);
        }

        public void Model(object model, string query)
        {
            reply();
            ValueResult = model;
        }

        public void Collection(object collection)
        {
            Collection(collection, null);
        }

        public void Collection(object collection, string query)
        {
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
                if (Handler.EnabledHandlers.HasFlag(HandlerTypes.Get))
                {
                    Handler.Get(this);
                }
            }
            catch (ResException ex)
            {
                if (replied)
                {
                    Service.OnError("Error in value get request for {0}: {1} - {2}", ResourceName, ex.Code, ex.Message);
                }
                else
                {
                    ErrorResult = new ResError(ex);
                }
            }
            catch (Exception ex)
            {
                // Log error and rethrow as only ResExceptions are considered valid behaviour
                Service.OnError("Error in value get request for {0}: {1}", ResourceName, ex.Message);
                throw ex;
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
