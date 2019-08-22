using System;
using System.Collections.Generic;
using System.Reflection;

namespace ResgateIO.Service
{
    /// <summary>
    /// Provides a handler whose handler methods can be set dynamically.
    /// </summary>
    public class DynamicHandler: IResourceHandler
    {
        private ResourceType resourceType = ResourceType.Unknown;
        private HandlerTypes enabledHandlers = HandlerTypes.None;

        private Action<IAccessRequest> access = null;
        private Action<IGetRequest> get = null;
        private Action<ICallRequest> call = null;
        private Action<IAuthRequest> auth = null;
        private Func<IResourceContext, IDictionary<string, object>, Dictionary<string, object>> applyChange = null;
        private Action<IResourceContext, object, int> applyAdd = null;
        private Func<IResourceContext, int, object> applyRemove = null;
        private Action<IResourceContext, object> applyCreate = null;
        private Func<IResourceContext, object> applyDelete = null;
        private Dictionary<string, Action<ICallRequest>> callMethods = null;
        private Dictionary<string, Action<IAuthRequest>> authMethods = null;

        /// <summary>
        /// Initializes a new instance of the DynamicHandler class.
        /// </summary>
        public DynamicHandler()
        {
        }


        /// <summary>
        /// Gets the resource type associated with the resource handler.
        /// </summary>
        public virtual ResourceType Type { get { return resourceType; } }

        /// <summary>
        /// Gets the enabled handler based on the handler methods being overridden.
        /// </summary>
        public virtual HandlerTypes EnabledHandlers { get { return enabledHandlers; } }

        /// <summary>
        /// Sets the resource type.
        /// This will overwrite any type set by the SetModelGet or SetCollectionGet methods.
        /// </summary>
        /// <param name="type">Resource type.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetType(ResourceType type)
        {
            resourceType = type;
            return this;
        }

        /// <summary>
        /// Sets the access handler method, and sets the EnableHandlers bit flag
        /// if the handler is not null, otherwise it unsets the flag.
        /// </summary>
        /// <param name="accessHandler">Access handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetAccess(Action<IAccessRequest> accessHandler)
        {
            toggleHandlers(HandlerTypes.Access, accessHandler != null);
            access = accessHandler;
            return this;
        }

        /// <summary>
        /// Sets the get handler method, and sets the EnableHandlers bit flag
        /// if the handler is not null, otherwise it unsets the flag.
        /// </summary>
        /// <param name="getHandler">Get handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetGet(Action<IGetRequest> getHandler)
        {
            toggleHandlers(HandlerTypes.Get, getHandler != null);
            get = getHandler;
            return this;
        }

        /// <summary>
        /// Sets a model get handler method, sets the EnableHandlers bit flag,
        /// and sets Type to ResourceType.Model, if getHandler is not null.
        /// Otherwise it unsets the EnableHandlers bit flag, and sets Type
        /// to ResourceType.Unknown.
        /// </summary>
        /// <param name="getHandler">Model get handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetModelGet(Action<IModelRequest> getHandler)
        {
            toggleHandlers(HandlerTypes.Get, getHandler != null);
            if (getHandler != null)
            {
                get = r => getHandler((IModelRequest)r);
                resourceType = ResourceType.Model;
            }
            else
            {
                get = null;
                resourceType = ResourceType.Unknown;
            }
            return this;
        }

        /// <summary>
        /// Sets a collection get handler method, sets the EnableHandlers bit flag,
        /// and sets Type to ResourceType.Collection, if getHandler is not null.
        /// Otherwise it unsets the EnableHandlers bit flag, and sets Type
        /// to ResourceType.Unknown.
        /// </summary>
        /// <param name="getHandler">Collection get handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetCollectionGet(Action<ICollectionRequest> getHandler)
        {
            toggleHandlers(HandlerTypes.Get, getHandler != null);
            if (getHandler != null)
            {
                get = r => getHandler((ICollectionRequest)r);
                resourceType = ResourceType.Collection;
            }
            else
            {
                get = null;
                resourceType = ResourceType.Unknown;
            }
            return this;
        }

        /// <summary>
        /// Sets the call handler method, and sets the EnableHandlers bit flag
        /// if the handler is not null, otherwise it unsets the flag.
        /// </summary>
        /// <param name="callHandler">Call handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetCall(Action<ICallRequest> callHandler)
        {
            toggleHandlers(HandlerTypes.Call, callHandler != null || callMethods != null);
            call = callHandler;
            return this;
        }

        /// <summary>
        /// Sets the call handler for a specific method, and sets the EnableHandlers bit flag appropriately.
        /// </summary>
        /// <param name="method">Name of the method. Must only contain alpha-numeric characters.</param>
        /// <param name="callHandler">Call method handler. Null removes any previously registered handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetCallMethod(string method, Action<ICallRequest> callHandler)
        {
            if (!ResService.IsValidPart(method))
            {
                throw new ArgumentException("Invalid method name: " + method);
            }
            if (callHandler == null)
            {
                if (callMethods != null)
                {
                    callMethods.Remove(method);
                    if (callMethods.Count == 0)
                    {
                        callMethods = null;
                    }
                }
            }
            else
            {
                if (callMethods == null)
                {
                    callMethods = new Dictionary<string, Action<ICallRequest>>();
                }
                callMethods[method] = callHandler;
            }
            toggleHandlers(HandlerTypes.Call, call != null || callMethods != null);
            return this;
        }

        /// <summary>
        /// Sets the auth handler method, and sets the EnableHandlers bit flag
        /// if the handler is not null, otherwise it unsets the flag.
        /// </summary>
        /// <param name="authHandler">Auth handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetAuth(Action<IAuthRequest> authHandler)
        {
            toggleHandlers(HandlerTypes.Auth, authHandler != null || authMethods != null);
            auth = authHandler;
            return this;
        }

        /// <summary>
        /// Sets the auth handler for a specific method, and sets the EnableHandlers bit flag appropriately
        /// </summary>
        /// <param name="method">Name of the method. Must only contain alpha-numeric characters.</param>
        /// <param name="authHandler">Auth method handler. Null removes any previously registered handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetAuthMethod(string method, Action<IAuthRequest> authHandler)
        {
            if (!ResService.IsValidPart(method))
            {
                throw new ArgumentException("Invalid method name: " + method);
            }
            if (authHandler == null)
            {
                if (authMethods != null)
                {
                    authMethods.Remove(method);
                    if (authMethods.Count == 0)
                    {
                        authMethods = null;
                    }
                }
            }
            else
            {
                if (authMethods == null)
                {
                    authMethods = new Dictionary<string, Action<IAuthRequest>>();
                }
                authMethods[method] = authHandler;
            }
            toggleHandlers(HandlerTypes.Auth, auth != null || authMethods != null);
            return this;
        }

        /// <summary>
        /// Sets the apply change handler method, and sets the EnableHandlers bit flag
        /// if the handler is not null, otherwise it unsets the flag.
        /// </summary>
        /// <param name="applyChangeHandler">Apply change handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApplyChange(Func<IResourceContext, IDictionary<string, object>, Dictionary<string, object>> applyChangeHandler)
        {
            toggleHandlers(HandlerTypes.ApplyChange, applyChangeHandler != null);
            applyChange = applyChangeHandler;
            return this;
        }

        /// <summary>
        /// Sets the apply add handler method, and sets the EnableHandlers bit flag
        /// if the handler is not null, otherwise it unsets the flag.
        /// </summary>
        /// <param name="applyAddHandler">Apply add handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApplyAdd(Action<IResourceContext, object, int> applyAddHandler)
        {
            toggleHandlers(HandlerTypes.ApplyAdd, applyAddHandler != null);
            applyAdd = applyAddHandler;
            return this;
        }

        /// <summary>
        /// Sets the apply remove handler method, and sets the EnableHandlers bit flag
        /// if the handler is not null, otherwise it unsets the flag.
        /// </summary>
        /// <param name="applyRemoveHandler">Apply remove handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApplyRemove(Func<IResourceContext, int, object> applyRemoveHandler)
        {
            toggleHandlers(HandlerTypes.ApplyRemove, applyRemoveHandler != null);
            applyRemove = applyRemoveHandler;
            return this;
        }

        /// <summary>
        /// Sets the apply create handler method, and sets the EnableHandlers bit flag
        /// if the handler is not null, otherwise it unsets the flag.
        /// </summary>
        /// <param name="applyCreateHandler">Apply create handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApplyCreate(Action<IResourceContext, object> applyCreateHandler)
        {
            toggleHandlers(HandlerTypes.ApplyCreate, applyCreateHandler != null);
            applyCreate = applyCreateHandler;
            return this;
        }

        /// <summary>
        /// Sets the apply delete handler method, and sets the EnableHandlers bit flag
        /// if the handler is not null, otherwise it unsets the flag.
        /// </summary>
        /// <param name="applyDeleteHandler">Apply delete handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApplyDelete(Func<IResourceContext, object> applyDeleteHandler)
        {
            toggleHandlers(HandlerTypes.ApplyDelete, applyDeleteHandler != null);
            applyDelete = applyDeleteHandler;
            return this;
        }

        /// <summary>
        /// Method called on a get request.
        /// </summary>
        /// <param name="request">Get request context.</param>
        public virtual void Get(IGetRequest request)
        {
            get?.Invoke(request);
        }

        /// <summary>
        /// Method called on an access request.
        /// </summary>
        /// <param name="request">Access request context.</param>
        public virtual void Access(IAccessRequest request)
        {
            access?.Invoke(request);
        }

        /// <summary>
        /// Method called on an auth request.
        /// </summary>
        /// <param name="request">Auth request context.</param>
        public virtual void Auth(IAuthRequest request)
        {
            if (authMethods != null)
            {
                if (authMethods.TryGetValue(request.Method, out Action<IAuthRequest> handler))
                {
                    handler(request);
                    return;
                }
                else if (auth == null)
                {
                    request.MethodNotFound();
                    return;
                }
            }
            auth?.Invoke(request);
        }

        /// <summary>
        /// Method called on a call request.
        /// </summary>
        /// <param name="request">Call request context.</param>
        public virtual void Call(ICallRequest request)
        {
            if (callMethods != null)
            {
                if (callMethods.TryGetValue(request.Method, out Action<ICallRequest> handler))
                {
                    handler(request);
                    return;
                }
                else if (call == null)
                {
                    request.MethodNotFound();
                    return;
                }
            }
            call?.Invoke(request);
        }

        /// <summary>
        /// Method called to apply a model change event.
        /// </summary>
        /// <param name="resource">Resource to apply the change to.</param>
        /// <param name="changes">Property values to apply to model.</param>
        /// <returns>A dictionary with the values to apply to revert the changes.</returns>
        public virtual Dictionary<string, object> ApplyChange(IResourceContext resource, IDictionary<string, object> changes)
        {
            return applyChange?.Invoke(resource, changes);
        }
        
        /// <summary>
        /// Method called to apply a collection add event.
        /// </summary>
        /// <param name="resource">Resource to add the value to.</param>
        /// <param name="value">Value to add.</param>
        /// <param name="idx">Index position where to add the value.</param>
        public virtual void ApplyAdd(IResourceContext resource, object value, int idx)
        {
            applyAdd?.Invoke(resource, value, idx);
        }

        /// <summary>
        /// Method called to apply a collection remove event.
        /// </summary>
        /// <param name="resource">Resource to remove the value from.</param>
        /// <param name="idx">Index position of the value to remove.</param>
        /// <returns>The removed value.</returns>
        public virtual object ApplyRemove(IResourceContext resource, int idx)
        {
            return applyRemove?.Invoke(resource, idx);
        }

        /// <summary>
        /// Method called to apply a resource create event.
        /// </summary>
        /// <param name="resource">Resource to create.</param>
        /// <param name="data">The resource data object.</param>
        public virtual void ApplyCreate(IResourceContext resource, object data)
        {
            applyCreate?.Invoke(resource, data);
        }
        
        /// <summary>
        /// Method called to apply a resource delete event.
        /// </summary>
        /// <param name="resource">Resource to delete.</param>
        /// <returns>The deleted resource data object.</returns>
        public virtual object ApplyDelete(IResourceContext resource)
        {
            return applyDelete?.Invoke(resource);
        }

        private void toggleHandlers(HandlerTypes type, bool setFlag)
        {
            if (setFlag)
            {
                enabledHandlers |= type;
            }
            else
            {
                enabledHandlers &= ~type;
            }
        }
    }
}
