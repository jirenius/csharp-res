using System.Collections.Generic;

namespace ResgateIO.Service
{
    /// <summary>
    /// Defines methods to handle requests and events on a resource.
    /// </summary>
    public interface IResourceHandler
    {
        /// <summary>
        /// Gets the resource type associated with the resource handler.
        /// </summary>
        ResourceType Type { get; }

        /// <summary>
        /// Gets the enabled handler.
        /// </summary>
        HandlerTypes EnabledHandlers { get; }
                    
        /// <summary>
        /// Method called on a get request.
        /// </summary>
        /// <param name="request">Get request context.</param>
        void Get(IGetRequest request);

        /// <summary>
        /// Method called on an access request.
        /// </summary>
        /// <param name="request">Access request context.</param>
        void Access(IAccessRequest request);

        /// <summary>
        /// Method called on an auth request.
        /// </summary>
        /// <param name="request">Auth request context.</param>
        void Auth(IAuthRequest request);

        /// <summary>
        /// Method called on a call request.
        /// </summary>
        /// <param name="request">Call request context.</param>
        void Call(ICallRequest request);

        /// <summary>
        /// Method called on a new call request.
        /// </summary>
        /// <param name="request">New call request context.</param>
        void New(INewRequest request);

        /// <summary>
        /// Method called to apply a model change event.
        /// </summary>
        /// <param name="resource">Resource to apply the change to.</param>
        /// <param name="changes">Property values to apply to model.</param>
        /// <returns>A dictionary with the values to apply to revert the changes.</returns>
        Dictionary<string, object> ApplyChange(IResourceContext resource, IDictionary<string, object> changes);
        
        /// <summary>
        /// Method called to apply a collection add event.
        /// </summary>
        /// <param name="resource">Resource to add the value to.</param>
        /// <param name="value">Value to add.</param>
        /// <param name="idx">Index position where to add the value.</param>
        void ApplyAdd(IResourceContext resource, object value, int idx);

        /// <summary>
        /// Method called to apply a collection remove event.
        /// </summary>
        /// <param name="resource">Resource to remove the value from.</param>
        /// <param name="idx">Index position of the value to remove.</param>
        /// <returns>The removed value.</returns>
        object ApplyRemove(IResourceContext resource, int idx);

        /// <summary>
        /// Method called to apply a resource create event.
        /// </summary>
        /// <param name="resource">Resource to create.</param>
        /// <param name="data">The resource data object.</param>
        void ApplyCreate(IResourceContext resource, object data);
        
        /// <summary>
        /// Method called to apply a resource delete event.
        /// </summary>
        /// <param name="resource">Resource to delete.</param>
        /// <returns>The deleted resource data object.</returns>
        object ApplyDelete(IResourceContext resource);
    }
}
