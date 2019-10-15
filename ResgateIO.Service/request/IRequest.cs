using System;
using System.Collections.Generic;
using System.Text;
using NATS.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ResgateIO.Service
{
    /// <summary>
    /// Provides context information and methods for responding to a request.
    /// </summary>
    public interface IRequest: IResourceContext, IAccessRequest, IGetRequest, ICallRequest, IAuthRequest, IModelRequest, ICollectionRequest, INewRequest
    {
        /// <summary>
        /// Type of request.
        /// </summary>
        RequestType Type { get; }

        /// <summary>
        /// Sends a raw RES protocol response to a request.
        /// Throws an exception if a response has already been sent.
        /// </summary>
        /// <remarks>
        /// Only use this method if you are familiar with the RES protocol,
        /// and you know what you are doing.
        /// </remarks>
        /// <param name="data">JSON encoded RES response data. Text encoding must be UTF8 without BOM.</param>
        void RawResponse(byte[] data);
    }
}
