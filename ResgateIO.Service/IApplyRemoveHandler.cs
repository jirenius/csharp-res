namespace ResgateIO.Service
{
    public interface IApplyRemoveHandler: IResourceHandler
    {
        /// <summary>
        /// Method called to apply a collection remove event.
        /// </summary>
        /// <param name="resource">Resource to remove the value from.</param>
        /// <param name="idx">Index position of the value to remove.</param>
        /// <returns>The removed value.</returns>
        object ApplyRemove(ResourceContext resource, int idx);
    }
}