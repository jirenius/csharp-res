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
    internal class ValueGetRequest : ResourceDecorator, IRequest, IGetRequest, IModelRequest, ICollectionRequest
    {
        private const string invalidError = "Method call invalid within get request handler";

        public object ValueResult { get; private set; }
        public ResError ErrorResult { get; private set; }

        private ILogger Log { get { return Service.Log; } }
        private bool replied = false;

        public ValueGetRequest(IResourceContext resource)
            : base(resource)
        {
        }

        public override T Value<T>()
        {
            throw new InvalidOperationException("Value called within get request handler");
        }

        public override Task<T> ValueAsync<T>()
        {
            throw new InvalidOperationException("ValueAsync called within get request handler");
        }

        public override T RequireValue<T>()
        {
            throw new InvalidOperationException("RequireValue called within get request handler");
        }

        public override Task<T> RequireValueAsync<T>()
        {
            throw new InvalidOperationException("RequireValueAsync called within get request handler");
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

        public RequestType Type { get { return RequestType.Get; } }

        public string Method => throw new InvalidOperationException(invalidError);

        public string CID => throw new InvalidOperationException(invalidError);

        public JToken Token => throw new InvalidOperationException(invalidError);

        public JToken Params => throw new InvalidOperationException(invalidError);

        public Dictionary<string, string[]> Header => throw new InvalidOperationException(invalidError);

        public string Host => throw new InvalidOperationException(invalidError);

        public string RemoteAddr => throw new InvalidOperationException(invalidError);

        public string URI => throw new InvalidOperationException(invalidError);

        internal async Task ExecuteHandler()
        {
            try
            {
                await Handler.Handle(this);
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

        public void RawResponse(byte[] data)
        {
            throw new InvalidOperationException(invalidError);
        }

        public void Access(bool get, string call)
        {
            throw new InvalidOperationException(invalidError);
        }

        public void AccessDenied()
        {
            throw new InvalidOperationException(invalidError);
        }

        public void AccessGranted()
        {
            throw new InvalidOperationException(invalidError);
        }

        public T ParseToken<T>()
        {
            throw new InvalidOperationException(invalidError);
        }

        public void Ok()
        {
            throw new InvalidOperationException(invalidError);
        }

        public void Ok(object result)
        {
            throw new InvalidOperationException(invalidError);
        }

        public void Resource(string resourceID)
        {
            throw new InvalidOperationException(invalidError);
        }

        public void MethodNotFound()
        {
            throw new InvalidOperationException(invalidError);
        }

        public void InvalidParams()
        {
            throw new InvalidOperationException(invalidError);
        }

        public void InvalidParams(string message)
        {
            throw new InvalidOperationException(invalidError);
        }

        public void InvalidParams(string message, object data)
        {
            throw new InvalidOperationException(invalidError);
        }

        public T ParseParams<T>()
        {
            throw new InvalidOperationException(invalidError);
        }

        public void TokenEvent(object token)
        {
            throw new InvalidOperationException(invalidError);
        }

        public void New(Ref rid)
        {
            throw new InvalidOperationException(invalidError);
        }
    }
}
