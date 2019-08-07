using System;
using System.Collections.Generic;

namespace ResgateIO.Service
{
    public class Resource
    {
        public ResService Service { get; }
        public string ResourceName { get; }
        public ResourceType ResourceType { get; }
        public IResourceHandler Handler { get; }
        public Dictionary<string, string> PathParams { get; }

        /// <summary>
        /// Query part of the resource ID without the question mark separator.
        /// </summary>
        public string Query { get; }

        public Resource(ResService service, string rname, IResourceHandler handler, Dictionary<string, string> pathParams, string query)
        {
            Service = service;
            ResourceName = rname;
            Handler = handler;
            PathParams = pathParams;
            Query = query;

            if (Handler is IModelHandler)
            {
                ResourceType = ResourceType.Model;
            }
            else if (Handler is ICollectionHandler)
            {
                ResourceType = ResourceType.Collection;
            } else
            {
                ResourceType = ResourceType.Unknown;
            }
        }

        public Resource(ResService service)
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
        public T Value<T>()
        {
            // Assert Value is not called from within Get handler.
            //if (inGet)
            //{
            //    throw new InvalidOperationException("Value called from inside resource Get handler");
            //}
            throw new NotImplementedException();
        }

        /// <summary>
        /// Gets the resource data object as provided from the Get resource handler.
        /// If the get handler fails, or no get handler is defined, or the get handler responds
        /// with a different type than T, it throws an exception.
        /// May not be called from within a resource Get handler.
        /// </summary>
        /// <typeparam name="T">Type of resource data object.</typeparam>
        /// <returns>Resource data object.</returns>
        public T RequireValue<T>()
        {
            throw new NotImplementedException();
        }

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
        public void Event(string eventName, object payload)
        {
            switch (eventName)
            {
                case "change":
                    throw new InvalidOperationException("Use ChangeEvent to send change events");
                case "delete":
                    throw new InvalidOperationException("Use DeleteEvent to send delete events");
                case "add":
                    throw new InvalidOperationException("Use AddEvent to send add events");
                case "remove":
                    throw new InvalidOperationException("Use RemoveEvent to send remove events");
                case "patch":
                    throw new InvalidOperationException("Reserved event name: \"patch\"");
                case "reaccess":
                    throw new InvalidOperationException("Use ReaccessEvent to send reaccess events");
                case "unsubscribe":
                    throw new InvalidOperationException("Reserved event name: \"unsubscribe\"");
                case "query":
                    throw new InvalidOperationException("Reserved event name: \"query\"");
            }

            if (!isValidPart(eventName))
            {
                throw new InvalidOperationException("Invalid event name: " + eventName);
            }

            sendEvent(eventName, payload);
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
            if (ResourceType == ResourceType.Collection)
            {
                throw new InvalidOperationException("Change event not allowed on resource of ResourceType.Collection.");
            }
            if (properties == null || properties.Count == 0)
            {
                return;
            }
            if (Handler is IApplyChangeHandler changeHandler)
            {
                Dictionary<string, object> rev = changeHandler.ApplyChange(this, properties);
                if (rev == null || rev.Count == 0)
                {
                    return;
                }
            }
            sendEvent("change", new ChangeEventDto(properties));
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
            if (ResourceType == ResourceType.Model)
            {
                throw new InvalidOperationException("Add event not allowed on resource of ResourceType.Model.");
            }
            if (idx < 0)
            {
                throw new InvalidOperationException("Add event idx less than zero.");
            }
            if (Handler is IApplyAddHandler addHandler) {
                addHandler.ApplyAdd(this, value, idx);
            }
            sendEvent("add", new AddEventDto(value, idx));
        }

        /// <summary>
        /// Sends an remove event, removing the value at index idx.
        /// Throws an exception if the resource is not of ResourceType.Collection.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#collection-remove-event
        /// </remarks>
        /// <param name="idx">Index position where the value has been added.</param>
        public void RemoveEvent(int idx)
        {
            if (ResourceType == ResourceType.Model)
            {
                throw new InvalidOperationException("Remove event not allowed on resource of ResourceType.Model.");
            }
            if (idx < 0)
            {
                throw new InvalidOperationException("Remove event idx less than zero.");
            }
            if (Handler is IApplyRemoveHandler removeHandler)
            {
                removeHandler.ApplyRemove(this, idx);
            }
            sendEvent("remove", new RemoveEventDto(idx));
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
        public void QueryEvent(QueryCallBack callback)
        {
            //var conn = Service.Connection;
            //string qsubj = conn.NewInbox();
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sends a create event to signal the resource has been created.
        /// </summary>
        /// <param name="data">Resource data object.</param>
        public void CreateEvent(object data)
        {
            if (Handler is IApplyCreateHandler createHandler)
            {
                createHandler.ApplyCreate(this, data);
            }
            sendEvent("create", null);
        }

        /// <summary>
        /// Sends a delete event to signal the resource has been deleted.
        /// </summary>
        public void DeleteEvent()
        {
            if (Handler is IApplyDeleteHandler deleteHandler)
            {
                deleteHandler.ApplyDelete(this);
            }
            sendEvent("delete", null);
        }

        private void sendEvent(string eventName, object payload)
        {
            Service.Send("event." + ResourceName + "." + eventName, payload);
        }

        private bool isValidPart(string eventName)
        {
            foreach (char c in eventName)
            {
                if (c == '?') {
                    return false;
                }
                if (c < 33 || c > 126 || c == '?' || c == '*' || c == '>' || c == '.')
                {
                    return false;
                }
            }
            return true;
        }
    }
}