﻿using System;

namespace ResgateIO.Service
{
    /// <summary>
    /// Provides context information and methods for responding to a collection get request.
    /// </summary>
    public interface ICollectionRequest : IResourceContext
    {
        /// <summary>
        /// Sends a successful collection response for the get request.
        /// </summary>
        /// <remarks>
        /// The collection must be serializable into a JSON array with items
        /// serializable into JSON primitives or resource references.
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-protocol.md#collections
        /// </remarks>
        /// <param name="collection">Collection data.</param>
        void Collection(object collection);

        /// <summary>
        /// Sends a successful collection query response for the get request.
        /// </summary>
        /// <remarks>
        /// The collection must be serializable into a JSON array with items
        /// serializable into JSON primitives or resource references.
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-protocol.md#collections
        /// </remarks>
        /// <param name="collection">Collection data.</param>
        /// <param name="query">Normalized query</param>
        void Collection(object collection, string query);

        /// <summary>
        /// Sends an error response to the request.
        /// </summary>
        void Error(ResError error);

        /// <summary>
        /// Sends a system.notFound response.
        /// </summary>
        void NotFound();

        /// <summary>
        /// Sends a system.invalidQuery response with a default error message.
        /// </summary>
        void InvalidQuery();

        /// <summary>
        /// Sends a system.invalidQuery response with a custom error message.
        /// </summary>
        void InvalidQuery(string message);

        /// <summary>
        /// Sends a system.invalidQuery response with a custom error message and data.
        /// </summary>
        void InvalidQuery(string message, object data);

        /// <summary>
        /// Attempts to set the timeout duration of the request.
        /// The call has no effect if the requester has already timed out the request,
        /// or if a reply has already been sent.
        /// </summary>
        /// <param name="milliseconds">Timeout duration in milliseconds.</param>
        void Timeout(int milliseconds);

        /// <summary>
        /// Attempts to set the timeout duration of the request.
        /// The call has no effect if the requester has already timed out the request,
        /// or if a reply has already been sent.
        /// </summary>
        /// <param name="milliseconds">Timeout duration.</param>
        void Timeout(TimeSpan duration);
    }
}