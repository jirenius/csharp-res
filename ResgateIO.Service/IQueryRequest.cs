namespace ResgateIO.Service
{
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