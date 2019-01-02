using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

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
        public void ChangeEvent(Dictionary<string, object> properties)
        {
            if (ResourceType != ResourceType.Model)
            {
                throw new InvalidOperationException("Change event only allowed on resource of ResourceType.Model.");
            }
            if (properties != null && properties.Count > 0)
            {
                sendEvent("change", properties);
            }
        }

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
        public void AddEvent(object value, int idx)
        {
            if (ResourceType != ResourceType.Collection)
            {
                throw new InvalidOperationException("Add event only allowed on resource of ResourceType.Model.");
            }
            sendEvent("add", new AddEventDto(value, idx));
        }

        /// <summary>
        /// Sends an remove event, removing the value at index idx.
        /// Throws an exception if the resource is not of ResourceType.Collection.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/jirenius/resgate/blob/master/docs/res-service-protocol.md#collection-remove-event
        /// </remarks>
        /// <param name="idx">Index position where the value has been added.</param>
        public void RemoveEvent(int idx)
        {
            if (ResourceType != ResourceType.Collection)
            {
                throw new InvalidOperationException("Remove event only allowed on resource of ResourceType.Model.");
            }
            sendEvent("remove", new RemoveEventDto(idx));
        }

        /// <summary>
        /// Sends a reaccess event to signal that the resource's access permissions has changed.
        /// It will invalidate any previous access response sent for the resource.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/jirenius/resgate/blob/master/docs/res-service-protocol.md#reaccess-event
        /// </remarks>
        public void ReaccessEvent()
        {
            sendEvent("reaccess", null);
        }

        private void sendEvent(string eventName, object payload)
        {
            Service.Send("event." + ResourceName + "." + eventName, payload);
        }
    }
}