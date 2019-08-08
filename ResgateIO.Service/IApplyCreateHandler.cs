namespace ResgateIO.Service
{
    public interface IApplyCreateHandler: IResourceHandler
    {
        /// <summary>
        /// Method called to apply a resource create event.
        /// </summary>
        /// <param name="resource">Resource to create.</param>
        /// <param name="data">The resource data object.</param>
        void ApplyCreate(ResourceContext resource, object data);
    }
}