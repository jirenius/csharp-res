using System.Collections.Generic;

namespace ResgateIO.Service
{
    public interface IApplyChangeHandler: IResourceHandler
    {
        /// <summary>
        /// Method called to apply a model change event.
        /// </summary>
        /// <param name="resource">Resource to apply the change to.</param>
        /// <param name="changes">Property values to apply to model.</param>
        /// <returns>A dictionary with the values to apply to revert the changes.</returns>
        Dictionary<string, object> ApplyChange(ResourceContext resource, Dictionary<string, object> changes);
    }
}