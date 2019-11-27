using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ResgateIO.Service
{
    /// <summary>
    /// Provides context information and methods for emitting events for a resource.
    /// </summary>
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
        /// Group ID for the context.
        /// </summary>
        string Group { get; }

        /// <summary>
        /// Context scoped key/value collection used to store and share data between handlers.
        /// </summary>
        IDictionary Items { get; }

        /// <summary>
        /// Resource handler.
        /// </summary>
        IAsyncHandler Handler { get; }

        /// <summary>
        /// Gets the resource data object as provided from <see cref="Handler"/> for <see cref="Type"/> being <see cref="RequestType.Get"/>.
        /// If the handler fails, or no resource is provided by the handler, it returns with null.
        /// If the get handler responds with a different type than T, it throws an exception.
        /// 
        /// May not be called from on <see cref="Type"/> being <see cref="RequestType.Get"/>.
        /// The call will block the current thread while awaiting the result from the <see cref="Handler"/>.
        /// </summary>
        /// <typeparam name="T">Type of resource data object.</typeparam>
        /// <returns>Resource data object.</returns>
        T Value<T>() where T : class;

        /// <summary>
        /// Gets the resource data object as provided from <see cref="Handler"/> for <see cref="Type"/> being <see cref="RequestType.Get"/>.
        /// If the handler fails, or no resource is provided by the handler, it returns with null.
        /// If the get handler responds with a different type than T, it throws an exception.
        /// 
        /// May not be called from on <see cref="Type"/> being <see cref="RequestType.Get"/>.
        /// </summary>
        /// <typeparam name="T">Type of resource data object.</typeparam>
        /// <returns>Resource data object.</returns>
        Task<T> ValueAsync<T>() where T : class;

        /// <summary>
        /// Gets the resource data object as provided from <see cref="Handler"/> for <see cref="Type"/> being <see cref="RequestType.Get"/>.
        /// If the handler fails, or the get handler responds with a different type than T, it throws an exception.
        /// 
        /// May not be called from on <see cref="Type"/> being <see cref="RequestType.Get"/>.
        /// The call will block the current thread while awaiting the result from the <see cref="Handler"/>.
        /// </summary>
        /// <typeparam name="T">Type of resource data object.</typeparam>
        /// <returns>Resource data object.</returns>
        T RequireValue<T>() where T : class;

        /// <summary>
        /// Gets the resource data object as provided from <see cref="Handler"/> for <see cref="Type"/> being <see cref="RequestType.Get"/>.
        /// If the handler fails, or the get handler responds with a different type than T, it throws an exception.
        /// 
        /// May not be called from on <see cref="Type"/> being <see cref="RequestType.Get"/>.
        /// </summary>
        /// <typeparam name="T">Type of resource data object.</typeparam>
        /// <returns>Resource data object.</returns>
        Task<T> RequireValueAsync<T>() where T : class;

        /// <summary>
        /// Sends a custom event on the resource without payload.
        /// Throws an exception if the event is one of the pre-defined or reserved events,
        /// "change", "delete", "add", "remove", "patch", "reaccess", "unsubscribe", or "query".
        /// For pre-defined events, the matching method, ChangeEvent, AddEvent,
        /// RemoveEvent, or ReaccessEvent should be used instead.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#custom-event
        /// </remarks>
        /// <param name="eventName">Name of the event.</param>
        void Event(string eventName);

        /// <summary>
        /// Sends a custom event on the resource without payload.
        /// Throws an exception if the event is one of the pre-defined or reserved events,
        /// "change", "delete", "add", "remove", "patch", "reaccess", "unsubscribe", or "query".
        /// For pre-defined events, the matching method, ChangeEvent, AddEvent,
        /// RemoveEvent, or ReaccessEvent should be used instead.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#custom-event
        /// </remarks>
        /// <param name="eventName">Name of the event.</param>
        Task EventAsync(string eventName);

        /// <summary>
        /// Sends a custom event on the resource with payload.
        /// Throws an exception if the event is one of the pre-defined or reserved events,
        /// "change", "delete", "add", "remove", "patch", "reaccess", "unsubscribe", or "query".
        /// For pre-defined events, the matching method, ChangeEvent, AddEvent,
        /// RemoveEvent, or ReaccessEvent should be used instead.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#custom-event
        /// </remarks>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="payload">JSON serializable payload. May be null.</param>
        void Event(string eventName, object payload);

        /// <summary>
        /// Sends a custom event on the resource with payload.
        /// Throws an exception if the event is one of the pre-defined or reserved events,
        /// "change", "delete", "add", "remove", "patch", "reaccess", "unsubscribe", or "query".
        /// For pre-defined events, the matching method, ChangeEvent, AddEvent,
        /// RemoveEvent, or ReaccessEvent should be used instead.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#custom-event
        /// </remarks>
        /// <param name="eventName">Name of the event.</param>
        /// <param name="payload">JSON serializable payload. May be null.</param>
        Task EventAsync(string eventName, object payload);

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
        /// <param name="properties">Properties to change with their new values.</param>
        void ChangeEvent(Dictionary<string, object> properties);

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
        /// <param name="properties">Properties to change with their new values.</param>
        Task ChangeEventAsync(Dictionary<string, object> properties);

        /// <summary>
        /// Sends an add event, adding the value at index idx.
        /// Throws an exception if the resource is not of ResourceType.Collection.
        /// </summary>
        /// <remarks>
        /// The value must be serializable into a JSON primitive or resource reference.
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#collection-add-event
        /// </remarks>
        /// <param name="value">Value to add.</param>
        /// <param name="idx">Index position of the value to add.</param>
        void AddEvent(object value, int idx);

        /// <summary>
        /// Sends an add event, adding the value at index idx.
        /// Throws an exception if the resource is not of ResourceType.Collection.
        /// </summary>
        /// <remarks>
        /// The value must be serializable into a JSON primitive or resource reference.
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#collection-add-event
        /// </remarks>
        /// <param name="value">Value to add.</param>
        /// <param name="idx">Index position of the value to add.</param>
        Task AddEventAsync(object value, int idx);

        /// <summary>
        /// Sends a remove event, removing the value at index idx.
        /// Throws an exception if the resource is not of ResourceType.Collection.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#collection-remove-event
        /// </remarks>
        /// <param name="idx">Index position of the value to remove.</param>
        void RemoveEvent(int idx);

        /// <summary>
        /// Sends a remove event, removing the value at index idx.
        /// Throws an exception if the resource is not of ResourceType.Collection.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#collection-remove-event
        /// </remarks>
        /// <param name="idx">Index position of the value to remove.</param>
        Task RemoveEventAsync(int idx);

        /// <summary>
        /// Sends a create event to signal the resource has been created.
        /// </summary>
        /// <param name="data">Resource data object.</param>
        void CreateEvent(object data);

        /// <summary>
        /// Sends a create event to signal the resource has been created.
        /// </summary>
        /// <param name="data">Resource data object.</param>
        Task CreateEventAsync(object data);

        /// <summary>
        /// Sends a delete event to signal the resource has been deleted.
        /// </summary>
        void DeleteEvent();

        /// <summary>
        /// Sends a delete event to signal the resource has been deleted.
        /// </summary>
        Task DeleteEventAsync();

        /// <summary>
        /// Sends a reaccess event to signal that the resource's access permissions has changed.
        /// It will invalidate any previous access response sent for the resource.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#reaccess-event
        /// </remarks>
        void ReaccessEvent();

        /// <summary>
        /// Sends a reset event for the resource.
        /// </summary>
        /// <remarks>
        /// Reset should be sent whenever the resource might have been modified,
        /// but where the service will not send any other event (including query events)
        /// to describe these modifications.
        /// </remarks>
        void ResetEvent();

        /// <summary>
        /// Sends a query event to signal that the query resource's underlying data has been modified.
        /// The callback will be called for each available query.
        /// The last time the callback is called, the IQueryRequest value will be null, to allow
        /// disposal of any resources related to the query event.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#query-event
        /// </remarks>
        /// <param name="callback">Query request callback delegate.</param>
        void QueryEvent(Func<IQueryRequest, Task> callback);

        /// <summary>
        /// Sends a query event to signal that the query resource's underlying data has been modified.
        /// The callback will be called for each available query.
        /// The last time the callback is called, the IQueryRequest value will be null, to allow disposal of any resources related to the query event.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#query-event
        /// </remarks>
        /// <param name="callback">Query request callback delegate.</param>
        void QueryEvent(Action<IQueryRequest> callback);

        /// <summary>
        /// Creates a new object that is a copy of the current IResourceContext,
        /// with the exception of the Query string and the Item context.
        /// </summary>
        /// <param name="query">Query string to use for the clone.</param>
        /// <returns>A new object that is a copy of the IResourceContext instance.</returns>
        IResourceContext CloneWithQuery(string query);
    }
}