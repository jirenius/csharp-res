using System.Collections.Generic;

namespace ResgateIO.Service
{
    public class Resource
    {
        public ResService Service { get; }
        public string ResourceName { get; }
        public IResourceHandler Handler { get; }
        public Dictionary<string, string> PathParams { get; }

        /// <summary>
        /// Query part of the resource ID without the question mark separator.
        /// </summary>
        public string Query { get; }

        public Resource(ResService service, string rname, IResourceHandler handler, Dictionary<string, string> pathParams, string query)
        {
            Service = service;
            ResourceName = rname;
            Handler = handler;
            PathParams = pathParams;
            Query = query;
        }

        /// <summary>
        /// Sends a raw data to NATS server on a given subject.
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="data"></param>
        internal void RawSend(string subject, byte[] data)
        {
            Service.Connection.Publish(subject, data);
        }
    }
}