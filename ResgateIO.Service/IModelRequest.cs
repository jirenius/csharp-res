using System.Collections.Generic;

namespace ResgateIO.Service
{
    public interface IModelRequest : IResourceRequest
    {
        /// <summary>
        /// Sends a successful model response for the get request.
        /// <remarks>
        /// The model must be serializable into a JSON object with values that
        /// must be serializable into JSON primitives or resource references.
        /// See the protocol specification for more information:
        ///    https://github.com/jirenius/resgate/blob/master/docs/res-protocol.md#models
        /// </remarks>
        /// The model must be serializable into a JSON object.
        /// </summary>
        /// <param name="model">Model data.</param>
        void Model(object model);

        /// <summary>
        /// Sends a successful query model response for the get request.
        /// The model must be serializable into a JSON object.
        /// </summary>
        /// <remarks>Only valid for a query model resource.</remarks>
        /// <param name="model">Model data</param>
        /// <param name="query">Normalized query</param>
        void Model(object model, string query);

        /// <summary>
        /// Sends an error response to the request.
        /// </summary>
        void Error(ResError error);

        /// <summary>
        /// Sends a system.notFound response.
        /// </summary>
        void NotFound();
    }
}