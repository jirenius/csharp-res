namespace ResgateIO.Service
{
    public interface IApplyDeleteHandler: IResourceHandler
    {
        /// <summary>
        /// Method called to apply a resource delete event.
        /// </summary>
        /// <param name="resource">Resource to delete.</param>
        /// <returns>The deleted resource data object.</returns>
        object ApplyDelete(Resource resource);
    }
}