using System.Collections.Generic;

namespace ResgateIO.Service
{
    public interface IResourceRequest
    {
        /// <summary>
        /// Service instance.
        /// </summary>
        ResService Service { get; }

        /// <summary>
        /// Resource name.
        /// </summary>
        string ResourceName { get; }

        /// <summary>
        /// Parameters that are derived from the resource name.
        /// </summary>
        Dictionary<string, string> PathParams { get; }

        /// <summary>
        /// Query part of the resource ID without the question mark separator.
        /// </summary>
        string Query { get; }

        /// <summary>
        /// Sends a change event.
        /// If properties is null or empty, no event is sent.
        /// Throws an exception if the resource is not of ResourceType.Model.
        /// </summary>
        /// <remarks>
        /// The values must be serializable into JSON primitives, resource references,
        /// or a delete action objects.
        /// See the protocol specification for more information:
        ///    https://github.com/jirenius/resgate/blob/master/docs/res-service-protocol.md#model-change-event
        /// </remarks>
        /// <param name="properties">Properties that has been changed with their new values.</param>
        void ChangeEvent(Dictionary<string, object> properties);

        /// <summary>
        /// Sends an add event, adding the value at index idx.
        /// Throws an exception if the resource is not of ResourceType.Collection.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/jirenius/resgate/blob/master/docs/res-service-protocol.md#collection-add-event
        /// </remarks>
        /// <param name="value">Value that has been added.</param>
        /// <param name="idx">Index position where the value has been added.</param>
        void AddEvent(object value, int idx);

        /// <summary>
        /// Sends an remove event, removing the value at index idx.
        /// Throws an exception if the resource is not of ResourceType.Collection.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/jirenius/resgate/blob/master/docs/res-service-protocol.md#collection-remove-event
        /// </remarks>
        /// <param name="idx">Index position where the value has been added.</param>
        void RemoveEvent(int idx);

        /// <summary>
        /// Sends a reaccess event to signal that the resource's access permissions has changed.
        /// It will invalidate any previous access response sent for the resource.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/jirenius/resgate/blob/master/docs/res-service-protocol.md#reaccess-event
        /// </remarks>
        void ReaccessEvent(int idx);
    }
}