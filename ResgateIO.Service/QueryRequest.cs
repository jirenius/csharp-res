using NATS.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResgateIO.Service
{
    internal class QueryRequest: ResourceDecorator, IQueryRequest
    {
        private Msg msg;

        public bool Replied { get; private set; }

        internal readonly List<EventDto> Events;

        private ILogger Log { get { return Service.Log; } }

        public QueryRequest(IResourceContext resource, Msg msg)
            : base(resource)
        {
            this.msg = msg;
            Replied = false;
            Events = new List<EventDto>();
        }

        /// <summary>
        /// Sends a model response for the query request.
        /// </summary>
        /// <remarks>
        /// Only valid for a model query resource.
        /// The model must be serializable into a JSON object with values
        /// serializable into JSON primitives or resource references.
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-protocol.md#models
        /// </remarks>
        /// <param name="model">Model data.</param>
        public void Model(object model)
        {
            Result(new ModelDto(model, null));
        }

        /// <summary>
        /// Sends a collection response for the query request.
        /// </summary>
        /// <remarks>
        /// Only valid for a collection query resource.
        /// The collection must be serializable into a JSON array with items
        /// serializable into JSON primitives or resource references.
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-protocol.md#collections
        /// </remarks>
        /// <param name="collection">Collection data.</param>
        public void Collection(object collection)
        {
            Result(new CollectionDto(collection, null));
        }

        /// <summary>
        /// Sends an error response to a query request.
        /// </summary>
        public void Error(ResError error)
        {
            try
            {
                RawResponse(Service.JsonSerialize(new ErrorDto(error)));
            }
            catch (Exception ex)
            {
                Service.OnError("Error serializing error query response: {0}", ex.Message);
                RawResponse(ResService.ResponseInternalError);
            }
        }

        /// <summary>
        /// Sends a system.notFound response.
        /// </summary>
        public void NotFound()
        {
            RawResponse(ResService.ResponseNotFound);
        }

        /// <summary>
        /// Sends a system.invalidQuery response with a default error message.
        /// </summary>
        public void InvalidQuery()
        {
            RawResponse(ResService.ResponseInvalidQuery);
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

        /// <summary>
        /// Adds a change event to the query response.
        /// If properties is null or empty, no event is added.
        /// </summary>
        /// <param name="properties">Properties that has been changed with their new values.</param>
        public override void ChangeEvent(Dictionary<string, object> properties)
        {
            if (properties == null || properties.Count == 0)
            {
                return;
            }
            Events.Add(new EventDto("change", new ChangeEventDto(properties)));
        }

        /// <summary>
        /// Adds an add event to the query response.
        /// </summary>
        /// <param name="value">Value that has been added.</param>
        /// <param name="idx">Index position where the value has been added.</param>
        public override void AddEvent(object value, int idx)
        {
            if (idx < 0)
            {
                throw new InvalidOperationException("Add event idx less than zero.");
            }
            Events.Add(new EventDto("add", new AddEventDto(value, idx)));
        }

        /// <summary>
        /// Adds a remove event to the query response.
        /// </summary>
        /// <param name="idx">Index position where the value has been removed.</param>
        public override void RemoveEvent(int idx)
        {
            if (idx < 0)
            {
                throw new InvalidOperationException("Remove event idx less than zero.");
            }
            Events.Add(new EventDto("remove", new RemoveEventDto(idx)));
        }

        /// <summary>
        /// Attempts to set the timeout duration of the query request.
        /// The call has no effect if the requester has already timed out the request,
        /// or if a response has already been sent.
        /// </summary>
        /// <param name="milliseconds">Timeout duration in milliseconds.</param>
        public void Timeout(int milliseconds)
        {
            if (milliseconds < 0)
            {
                throw new InvalidOperationException("Negative timeout duration");
            }

            var str = "timeout:\"" + milliseconds.ToString() + "\"";
            Log.Trace("<-- {0}: {1}", msg.Subject, str);
            Service.RawSend(msg.Reply, Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Attempts to set the timeout duration of the query request.
        /// The call has no effect if the requester has already timed out the request,
        /// or if a response has already been sent.
        /// </summary>
        /// <param name="milliseconds">Timeout duration.</param>
        public void Timeout(TimeSpan duration)
        {
            Timeout((int)duration.TotalMilliseconds);
        }

        /// <summary>
        /// Sends a raw RES protocol response to a query request.
        /// Throws an exception if a response has already been sent.
        /// </summary>
        /// <remarks>
        /// Only use this method if you are familiar with the RES protocol,
        /// and you know what you are doing.
        /// </remarks>
        /// <param name="data">JSON encoded RES response data</param>
        internal void RawResponse(byte[] data)
        {
            if (Replied)
            {
                throw new InvalidOperationException("Response already sent on query request");
            }
            Replied = true;
            Log.Trace("<=Q {0}: {1}", ResourceName, Encoding.UTF8.GetString(data));
            try
            {
                Service.RawSend(msg.Reply, data);
            }
            catch(Exception ex)
            {
                Service.OnError("Error sending query reply {0}: {1}", ResourceName, ex.Message);
            }
        }

        internal void Result(object result)
        {
            try
            {
                RawResponse(Service.JsonSerialize(new ResultDto(result)));
            }
            catch (Exception ex)
            {
                Service.OnError("Error serializing result query response: {0}", ex.Message);
                RawResponse(ResService.ResponseInternalError);
            }
        }
    }
}
