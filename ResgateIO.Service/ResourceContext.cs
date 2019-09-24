using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ResgateIO.Service
{
    /// <summary>
    /// Provides context information and methods for emitting events for a resource.
    /// </summary>
    public class ResourceContext: IResourceContext
    {
        /// <summary>
        /// Service instance.
        /// </summary>
        public ResService Service { get; }

        /// <summary>
        /// Resource name.
        /// </summary>
        public string ResourceName { get; }

        /// <summary>
        /// Parameters that are derived from the resource name.
        /// </summary>
        public IDictionary<string, string> PathParams { get; }

        /// <summary>
        /// Query part of the resource ID without the question mark separator.
        /// </summary>
        public string Query { get; }

        /// <summary>
        /// Context scoped key/value collection used to store and share data between handlers.
        /// </summary>
        public IDictionary Items { get; }

        /// <summary>
        /// Resource handler.
        /// </summary>
        public IResourceHandler Handler { get; }

        /// <summary>
        /// Group ID for the context.
        /// </summary>
        public string Group { get; }

        private EventHandler eventHandler;

        /// <summary>
        /// Initializes a new instance of the ResourceContext class.
        /// </summary>
        /// <param name="service">Service to which the resource context belong.</param>
        /// <param name="resourceName">Resource name without the query part.</param>
        /// <param name="handler">Resource handler.</param>
        /// <param name="pathParams">Path parameters derived from the resource name.</param>
        /// <param name="query">Query part of the resource name.</param>
        public ResourceContext(ResService service, string resourceName, IResourceHandler handler, EventHandler eventHandler, IDictionary<string, string> pathParams, string query, string group)
        {
            Service = service;
            ResourceName = resourceName;
            Handler = handler;
            this.eventHandler = eventHandler;
            PathParams = pathParams;
            Query = query == null ? "" : query;
            Group = group;
            Items = new Hashtable();
        }

        /// <summary>
        /// Initializes a new instance of the ResourceContext class without any resource specific context.
        /// </summary>
        /// <param name="service">Service to which the resource belong.</param>
        public ResourceContext(ResService service)
        {
            Service = service;
        }

        /// <summary>
        /// Returns the parameter derived from the resource name for the key placeholder.
        /// </summary>
        /// <param name="key">Name of the placeholder key in the path.</param>
        /// <returns>Path parameter value.</returns>
        public string PathParam(string key)
        {
            if (PathParams == null)
            {
                return null;
            }
            return PathParams[key];
        }

        /// <summary>
        /// Gets the resource data object as provided from the Get resource handler.
        /// If the get handler fails, or no get handler is defined, it return with null.
        /// If the get handler responds with a different type than T, it throws an exception.
        /// May not be called from within a resource Get handler.
        /// </summary>
        /// <typeparam name="T">Type of resource data object.</typeparam>
        /// <returns>Resource data object.</returns>
        public T Value<T>() where T : class
        {
            var valueGetRequest = new ValueGetRequest(this);
            valueGetRequest.ExecuteHandler();

            if (valueGetRequest.ErrorResult != null)
            {
                return default(T);
            }

            return (T)valueGetRequest.ValueResult;
        }

        /// <summary>
        /// Gets the resource data object as provided from the Get resource handler.
        /// If the get handler fails, or no get handler is defined, or the get handler responds
        /// with a different type than T, it throws an exception.
        /// May not be called from within a resource Get handler.
        /// </summary>
        /// <typeparam name="T">Type of resource data object.</typeparam>
        /// <returns>Resource data object.</returns>
        public T RequireValue<T>() where T : class
        {
            var valueGetRequest = new ValueGetRequest(this);
            valueGetRequest.ExecuteHandler();

            if (valueGetRequest.ErrorResult != null)
            {
                throw new ResException(valueGetRequest.ErrorResult);
            }

            return (T)valueGetRequest.ValueResult;
        }

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
        public void Event(string eventName)
        {
            Event(eventName, null);
        }

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
        public void Event(string eventName, object payload)
        {
            switch (eventName)
            {
                case "change":
                    throw new ArgumentException("Use ChangeEvent to send change events");
                case "delete":
                    throw new ArgumentException("Use DeleteEvent to send delete events");
                case "add":
                    throw new ArgumentException("Use AddEvent to send add events");
                case "remove":
                    throw new ArgumentException("Use RemoveEvent to send remove events");
                case "patch":
                    throw new ArgumentException("Reserved event name: \"patch\"");
                case "reaccess":
                    throw new ArgumentException("Use ReaccessEvent to send reaccess events");
                case "unsubscribe":
                    throw new ArgumentException("Reserved event name: \"unsubscribe\"");
                case "query":
                    throw new ArgumentException("Reserved event name: \"query\"");
            }

            if (!ResService.IsValidPart(eventName))
            {
                throw new ArgumentException("Invalid event name: " + eventName);
            }

            sendEvent(eventName, payload);

            eventHandler?.Invoke(this, new CustomEventArgs(eventName, payload));
        }

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
        public void ChangeEvent(Dictionary<string, object> properties)
        {
            Dictionary<string, object> rev = null;
            if (Handler.Type == ResourceType.Collection)
            {
                throw new InvalidOperationException("Change event not allowed on resource of ResourceType.Collection.");
            }
            if (properties == null || properties.Count == 0)
            {
                return;
            }
            if (Handler.EnabledHandlers.HasFlag(HandlerTypes.ApplyChange))
            {
                rev = Handler.ApplyChange(this, properties);
                if (rev == null || rev.Count == 0)
                {
                    return;
                }
                // Delete keys not present in the revert Dictionary
                properties = properties.Where(x => rev.ContainsKey(x.Key)).ToDictionary(x => x.Key, x => x.Value);
                if (properties.Count == 0)
                {
                    return;
                }
            }
            sendEvent("change", new ChangeEventDto(properties));

            eventHandler?.Invoke(this, new ChangeEventArgs(properties, rev));
        }

        /// <summary>
        /// Sends an add event, adding the value at index idx.
        /// Throws an exception if the resource is not of ResourceType.Collection.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#collection-add-event
        /// </remarks>
        /// <param name="value">Value that has been added.</param>
        /// <param name="idx">Index position where the value has been added.</param>
        public void AddEvent(object value, int idx)
        {
            if (Handler.Type == ResourceType.Model)
            {
                throw new InvalidOperationException("Add event not allowed on resource of ResourceType.Model.");
            }
            if (idx < 0)
            {
                throw new ArgumentException("Add event idx less than zero.");
            }
            if (Handler.EnabledHandlers.HasFlag(HandlerTypes.ApplyAdd))
            {
                Handler.ApplyAdd(this, value, idx);
            }
            sendEvent("add", new AddEventDto(value, idx));

            eventHandler?.Invoke(this, new AddEventArgs(value, idx));
        }

        /// <summary>
        /// Sends an remove event, removing the value at index idx.
        /// Throws an exception if the resource is not of ResourceType.Collection.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#collection-remove-event
        /// </remarks>
        /// <param name="idx">Index position where the value has been removed.</param>
        public void RemoveEvent(int idx)
        {
            object removed = null;
            if (Handler.Type == ResourceType.Model)
            {
                throw new InvalidOperationException("Remove event not allowed on resource of ResourceType.Model.");
            }
            if (idx < 0)
            {
                throw new ArgumentException("Remove event idx less than zero.");
            }
            if (Handler.EnabledHandlers.HasFlag(HandlerTypes.ApplyRemove))
            {
                removed = Handler.ApplyRemove(this, idx);
            }
            sendEvent("remove", new RemoveEventDto(idx));

            eventHandler?.Invoke(this, new RemoveEventArgs(removed, idx));
        }

        /// <summary>
        /// Sends a reaccess event to signal that the resource's access permissions has changed.
        /// It will invalidate any previous access response sent for the resource.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#reaccess-event
        /// </remarks>
        public void ReaccessEvent()
        {
            sendEvent("reaccess", null);
        }

        /// <summary>
        /// Sends a query event to signal that the query resource's underlying data has been modified.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#query-event
        /// </remarks>
        /// <param name="callback">Query request callback delegate.</param>
        public void QueryEvent(QueryCallback callback)
        {
            Service.AddQueryEvent(new QueryEvent(this, callback));
        }

        /// <summary>
        /// Sends a create event to signal the resource has been created.
        /// </summary>
        /// <param name="data">Resource data object.</param>
        public void CreateEvent(object data)
        {
            if (Handler.EnabledHandlers.HasFlag(HandlerTypes.ApplyCreate))
            {
                Handler.ApplyCreate(this, data);
            }
            sendEvent("create", null);

            eventHandler?.Invoke(this, new CreateEventArgs(data));
        }

        /// <summary>
        /// Sends a delete event to signal the resource has been deleted.
        /// </summary>
        public void DeleteEvent()
        {
            object data = null;
            if (Handler.EnabledHandlers.HasFlag(HandlerTypes.ApplyDelete))
            {
                data = Handler.ApplyDelete(this);
            }
            sendEvent("delete", null);

            eventHandler?.Invoke(this, new DeleteEventArgs(data));
        }

        private void sendEvent(string eventName, object payload)
        {
            Service.Send("event." + ResourceName + "." + eventName, payload);
        }
    }
}