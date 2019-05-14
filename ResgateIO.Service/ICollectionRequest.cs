using System.Collections.Generic;

namespace ResgateIO.Service
{
    public interface ICollectionRequest : IResourceRequest
    {
        /// <summary>
        /// Sends a successful collection response for the get request.
        /// </summary>
        /// <remarks>
        /// The collection must be serializable into a JSON array with items that
        /// must be serializable into JSON primitives or resource references.
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-protocol.md#collections
        /// </remarks>
        /// <param name="collection">Collection data.</param>
        void Collection(object collection);

        /// <summary>
        /// Sends a successful query collection response for the get request.
        /// The collection must be serializable into a JSON array.
        /// </summary>
        /// <remarks>Only valid for a query collection resource.</remarks>
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
    }
}