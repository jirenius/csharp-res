using System;

namespace ResgateIO.Service
{
    /// <summary>
    /// Provides context information and methods for responding to a query request.
    /// </summary>
    public interface IQueryRequest : IResourceContext
    {
        /// <summary>
        /// Sends an error response to the request.
        /// </summary>
        void Error(ResError error);

        /// <summary>
        /// Sends a system.notFound response.
        /// </summary>
        void NotFound();

        /// <summary>
        /// Attempts to set the timeout duration of the query request.
        /// The call has no effect if the requester has already timed out the query request,
        /// or if a response has already been sent.
        /// </summary>
        /// <param name="milliseconds">Timeout duration in milliseconds.</param>
        void Timeout(int milliseconds);

        /// <summary>
        /// Attempts to set the timeout duration of the query request.
        /// The call has no effect if the requester has already timed out the query request,
        /// or if a response has already been sent.
        /// </summary>
        /// <param name="milliseconds">Timeout duration.</param>
        void Timeout(TimeSpan duration);
    }
}