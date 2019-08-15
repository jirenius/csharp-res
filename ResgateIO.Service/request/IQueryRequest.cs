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
    }
}