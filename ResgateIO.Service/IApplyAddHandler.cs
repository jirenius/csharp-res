namespace ResgateIO.Service
{
    public interface IApplyAddHandler: IResourceHandler
    {
        /// <summary>
        /// Method called to apply a collection add event.
        /// </summary>
        /// <param name="resource">Resource to add the value to.</param>
        /// <param name="value">Value to add.</param>
        /// <param name="idx">Index position where to add the value.</param>
        void ApplyAdd(Resource resource, object value, int idx);
    }
}