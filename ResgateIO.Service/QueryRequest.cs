using NATS.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResgateIO.Service
{
    internal class QueryRequest: ResourceDecorator, IQueryRequest
    {
        private Msg msg;

        private string query;

        public bool Replied { get; private set; }

        public override string Query { get { return query; } }

        internal readonly List<EventDto> Events;

        public QueryRequest(IResourceContext resource, Msg msg)
            : base(resource)
        {
            this.msg = msg;
            Replied = false;
            Events = new List<EventDto>();
        }

        /// <summary>
        /// Sets the query string for the query request.
        /// </summary>
        /// <param name="query">Query string</param>
        public void SetQuery(string query)
        {
            this.query = query;
        }

        /// <summary>
        /// Sends an error response to a query request.
        /// </summary>
        public void Error(ResError error)
        {
            try
            {
                RawResponse(JsonUtils.Serialize(new ErrorDto(error)));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error serializing error query response: {0}" + ex.Message);
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

        public void ChangeEvent(Dictionary<string, object> properties)
        {
        }

        public void AddEvent(object value, int idx)
        {
        }

        public void RemoveEvent(int idx)
        {
        }

        /// <summary>
        /// Attempts to set the timeout duration of the query request.
        /// The call has no effect if the requester has already timed out the request.
        /// </summary>
        /// <param name="milliseconds">Timeout duration in milliseconds.</param>
        public void Timeout(int milliseconds)
        {
            if (milliseconds < 0)
            {
                throw new InvalidOperationException("Negative timeout duration");
            }

            var str = "timeout:\"" + milliseconds.ToString() + "\"";
            Console.WriteLine("<-- {0}: {1}", msg.Subject, str);
            Service.RawSend(msg.Reply, Encoding.UTF8.GetBytes(str));
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
            Console.WriteLine("<=Q {0}: {1}", ResourceName, Encoding.UTF8.GetString(data));
            try
            {
                Service.RawSend(msg.Reply, data);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error sending query reply {0}: {1}", ResourceName, ex.Message);
            }
        }
    }
}
