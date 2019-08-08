using System.Collections;
using System.Collections.Generic;

namespace ResgateIO.Service
{
    public delegate void QueryCallBack(IQueryRequest request);

    public interface IResourceContext
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
        /// Resource type.
        /// </summary>
        ResourceType ResourceType { get; }

        /// <summary>
        /// Parameters that are derived from the resource name.
        /// </summary>
        IDictionary<string, string> PathParams { get; }

        /// <summary>
        /// Returns the parameter derived from the resource name for the key placeholder.
        /// </summary>
        /// <param name="key">Name of the placeholder key.</param>
        /// <returns>Path parameter value.</returns>
        string PathParam(string key);

        /// <summary>
        /// Query part of the resource ID without the question mark separator.
        /// </summary>
        string Query { get; }

        /// <summary>
        /// Context scoped key/value collection used to store and share data between handlers.
        /// </summary>
        IDictionary Items { get; }

        /// <summary>
        /// Gets the resource data object as provided from the Get resource handler.
        /// If the get handler fails, or no get handler is defined, it return with null.
        /// If the get handler responds with a different type than T, it throws an exception.
        /// </summary>
        /// <typeparam name="T">Type of resource data object.</typeparam>
        /// <returns>Resource data object.</returns>
        T Value<T>();

        /// <summary>
        /// Gets the resource data object as provided from the Get resource handler.
        /// If the get handler fails, or no get handler is defined, or the get handler responds
        /// with a different type than T, it throws an exception.
        /// </summary>
        /// <typeparam name="T">Type of resource data object.</typeparam>
        /// <returns>Resource data object.</returns>
        T RequireValue<T>();

        /// <summary>
        /// Sends a custom event on the resource.
        /// Throws an exception if the event is one of the pre-defined or reserved events,
        /// "change", "delete", "add", "remove", "patch", "reaccess", "unsubscribe", or "query".
        /// For pre-defined events, the matching method, ChangeEvent, AddEvent,
        /// RemoveEvent, or ReaccessEvent should be used instead.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#custom-event
        /// </remarks>
        /// <param name="eventName">Name of the event</param>
        /// <param name="payload">JSON serializable payload. May be null.</param>
        void Event(string eventName, object payload);

        /// <summary>
        /// Sends a change event.
        /// If properties is null or empty, no event is sent.
        /// Throws an exception if the resource is not of ResourceType.Model.
        /// </summary>
        /// <remarks>
        /// The values must be serializable into JSON primitives, resource references,
        /// or a delete action objects.
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#model-change-event
        /// </remarks>
        /// <param name="properties">Properties that has been changed with their new values.</param>
        void ChangeEvent(Dictionary<string, object> properties);

        /// <summary>
        /// Sends an add event, adding the value at index idx.
        /// Throws an exception if the resource is not of ResourceType.Collection.
        /// </summary>
        /// <remarks>
        /// The value must be serializable into a JSON primitive or resource reference.
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#collection-add-event
        /// </remarks>
        /// <param name="value">Value that has been added.</param>
        /// <param name="idx">Index position where the value has been added.</param>
        void AddEvent(object value, int idx);

        /// <summary>
        /// Sends a remove event, removing the value at index idx.
        /// Throws an exception if the resource is not of ResourceType.Collection.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#collection-remove-event
        /// </remarks>
        /// <param name="idx">Index position where the value has been added.</param>
        void RemoveEvent(int idx);

        /// <summary>
        /// Sends a reaccess event to signal that the resource's access permissions has changed.
        /// It will invalidate any previous access response sent for the resource.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#reaccess-event
        /// </remarks>
        void ReaccessEvent();

        // QueryEvent sends a query event to signal that the query resource's underlying data has been modified.
        // See the protocol specification for more information:
        //    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#query-event

        /// <summary>
        /// Sends a query event to signal that the query resource's underlying data has been modified.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#query-event
        /// </remarks>
        /// <param name="callback">Query request callback delegate.</param>
        void QueryEvent(QueryCallBack callback);

        /// <summary>
        /// Sends a create event to signal the resource has been created.
        /// </summary>
        /// <param name="data">Resource data object.</param>
        void CreateEvent(object data);

        /// <summary>
        /// Sends a delete event to signal the resource has been deleted.
        /// </summary>
        void DeleteEvent();
    }
}