using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace ResgateIO.Service
{
    /// <summary>
    /// Provides a handler whose handler methods can be set dynamically.
    /// </summary>
    public class DynamicHandler: IAsyncHandler
    {
        private static readonly Task completedTask = Task.FromResult(false);
        private ResourceType resourceType = ResourceType.Unknown;
        private HandlerTypes enabledHandlers = HandlerTypes.None;

        private Func<IAccessRequest, Task> accessHandler = null;
        private Func<IGetRequest, Task> getHandler = null;
        private Func<ICallRequest, Task> callHandler = null;
        private Func<IAuthRequest, Task> authHandler = null;
        private Func<INewRequest, Task> newHandler = null;
        private Func<IResourceContext, EventArgs, Task> applyHandler = null;
        private Func<IResourceContext, ChangeEventArgs, Task> applyChangeHandler = null;
        private Func<IResourceContext, AddEventArgs, Task> applyAddHandler = null;
        private Func<IResourceContext, RemoveEventArgs, Task> applyRemoveHandler = null;
        private Func<IResourceContext, CreateEventArgs, Task> applyCreateHandler = null;
        private Func<IResourceContext, DeleteEventArgs, Task> applyDeleteHandler = null;
        private Func<IResourceContext, CustomEventArgs, Task> applyCustomHandler = null;
        private Dictionary<string, Func<ICallRequest, Task>> callMethods = null;
        private Dictionary<string, Func<IAuthRequest, Task>> authMethods = null;

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
        /// This will overwrite any type set by the <see cref="SetModelGet(Func{IModelRequest, Task})"/>,
        /// <see cref="SetModelGet(Action{IModelRequest})"/>, <see cref="SetCollectionGet(Func{ICollectionRequest, Task})"/>,
        /// or <see cref="SetCollectionGet(Action{ICollectionRequest})"/> methods.
        /// </summary>
        /// <param name="type">Resource type.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetType(ResourceType type)
        {
            resourceType = type;
            return this;
        }

        /// <summary>
        /// Sets the access handler method, and sets the EnableHandlers bit flag.
        /// </summary>
        /// <param name="accessHandler">Access handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetAccess(Func<IAccessRequest, Task> accessHandler)
        {
            if (this.accessHandler != null)
            {
                throw new InvalidOperationException("Access handler already set.");
            }
            toggleHandlers(HandlerTypes.Access, true);
            this.accessHandler = accessHandler;
            return this;
        }

        /// <summary>
        /// Sets the access handler method, and sets the EnableHandlers bit flag.
        /// </summary>
        /// <param name="accessHandler">Access handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetAccess(Action<IAccessRequest> accessHandler)
        {
            return SetAccess(r =>
            {
                accessHandler(r);
                return completedTask;
            });
        }

        ///// <summary>
        ///// Unsets any previously set access handler method, and unsets the EnableHandlers bit flag.
        ///// </summary>
        ///// <returns>This instance.</returns>
        //public DynamicHandler UnsetAccess()
        //{
        //    toggleHandlers(HandlerTypes.Access, false);
        //    this.accessHandler = null;
        //    return this;
        //}

        /// <summary>
        /// Sets the get handler method, and sets the EnableHandlers bit flag.
        /// </summary>
        /// <param name="getHandler">Get handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetGet(Func<IGetRequest, Task> getHandler)
        {
            if (this.getHandler != null)
            {
                throw new InvalidOperationException("Get handler already set.");
            }
            toggleHandlers(HandlerTypes.Get, true);
            this.getHandler = getHandler;
            return this;
        }

        /// <summary>
        /// Sets the get handler method, and sets the EnableHandlers bit flag.
        /// </summary>
        /// <param name="getHandler">Get handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetGet(Action<IGetRequest> getHandler)
        {
            return SetGet(r =>
            {
                getHandler(r);
                return completedTask;
            });
        }

        ///// <summary>
        ///// Unsets any previously set get handler method, and unsets the EnableHandlers bit flag.
        ///// </summary>
        ///// <returns>This instance.</returns>
        //public DynamicHandler UnsetGet()
        //{
        //    toggleHandlers(HandlerTypes.Access, false);
        //    this.getHandler = null;
        //    return this;
        //}

        /// <summary>
        /// Sets a model get handler method, sets the EnableHandlers bit flag,
        /// and sets Type to ResourceType.Model.
        /// </summary>
        /// <param name="getHandler">Model get handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetModelGet(Func<IModelRequest, Task> getHandler)
        {
            SetGet(r => getHandler((IModelRequest)r));
            resourceType = ResourceType.Model;
            return this;
        }

        /// <summary>
        /// Sets a model get handler method, sets the EnableHandlers bit flag,
        /// and sets Type to ResourceType.Model.
        /// </summary>
        /// <param name="getHandler">Model get handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetModelGet(Action<IModelRequest> getHandler)
        {
            SetGet(r =>
            {
                getHandler((IModelRequest)r);
                return completedTask;
            });
            resourceType = ResourceType.Model;
            return this;
        }

        /// <summary>
        /// Sets a collection get handler method, sets the EnableHandlers bit flag,
        /// and sets Type to ResourceType.Collection.
        /// </summary>
        /// <param name="getHandler">Collection get handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetCollectionGet(Func<ICollectionRequest, Task> getHandler)
        {
            SetGet(r => getHandler((ICollectionRequest)r));
            resourceType = ResourceType.Collection;
            return this;
        }

        /// <summary>
        /// Sets a collection get handler method, sets the EnableHandlers bit flag,
        /// and sets Type to ResourceType.Collection.
        /// </summary>
        /// <param name="getHandler">Collection get handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetCollectionGet(Action<ICollectionRequest> getHandler)
        {
            SetGet(r =>
            {
                getHandler((ICollectionRequest)r);
                return completedTask;
            });
            resourceType = ResourceType.Collection;
            return this;
        }

        /// <summary>
        /// Sets the call handler method, and sets the EnableHandlers bit flag.
        /// </summary>
        /// <param name="callHandler">Call handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetCall(Func<ICallRequest, Task> callHandler)
        {
            if (this.callHandler != null)
            {
                throw new InvalidOperationException("Call handler already set.");
            }
            toggleHandlers(HandlerTypes.Call, true);
            this.callHandler = callHandler;
            return this;
        }

        /// <summary>
        /// Sets the call handler method, and sets the EnableHandlers bit flag.
        /// </summary>
        /// <param name="callHandler">Call handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetCall(Action<ICallRequest> callHandler)
        {
            return SetCall(r =>
            {
                callHandler(r);
                return completedTask;
            });
        }

        /// <summary>
        /// Sets the call handler for a specific method, and sets the EnableHandlers bit flag appropriately.
        /// </summary>
        /// <param name="method">Name of the method. Must only contain alpha-numeric characters.</param>
        /// <param name="callHandler">Call method handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetCallMethod(string method, Func<ICallRequest, Task> callHandler)
        {
            if (!ResService.IsValidPart(method))
            {
                throw new ArgumentException("Invalid method name: " + method);
            }
            if (method == "new")
            {
                throw new ArgumentException("Must use SetNew to register handler for new call requests");
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
                    callMethods = new Dictionary<string, Func<ICallRequest, Task>>();
                }
                else if (callMethods.ContainsKey(method))
                {
                    throw new InvalidOperationException(String.Format("Call method {0} already set.", method));
                }
                callMethods[method] = callHandler;
            }
            toggleHandlers(HandlerTypes.Call, this.callHandler != null || callMethods != null);
            return this;
        }

        /// <summary>
        /// Sets the call handler for a specific method, and sets the EnableHandlers bit flag appropriately.
        /// </summary>
        /// <param name="method">Name of the method. Must only contain alpha-numeric characters.</param>
        /// <param name="callHandler">Call method handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetCallMethod(string method, Action<ICallRequest> callHandler)
        {
            return SetCallMethod(method, r =>
            {
                callHandler(r);
                return completedTask;
            });
        }

        /// <summary>
        /// Sets the auth handler method, and sets the EnableHandlers bit flag.
        /// </summary>
        /// <param name="authHandler">Auth handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetAuth(Func<IAuthRequest, Task> authHandler)
        {
            if (this.authHandler != null)
            {
                throw new InvalidOperationException("Auth handler already set.");
            }
            toggleHandlers(HandlerTypes.Auth, true);
            this.authHandler = authHandler;
            return this;
        }

        /// <summary>
        /// Sets the auth handler method, and sets the EnableHandlers bit flag.
        /// </summary>
        /// <param name="authHandler">Auth handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetAuth(Action<IAuthRequest> authHandler)
        {
            return SetAuth(r =>
            {
                authHandler(r);
                return completedTask;
            });
        }

        /// <summary>
        /// Sets the auth handler for a specific method, and sets the EnableHandlers bit flag appropriately
        /// </summary>
        /// <param name="method">Name of the method. Must only contain alpha-numeric characters.</param>
        /// <param name="authHandler">Auth method handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetAuthMethod(string method, Func<IAuthRequest, Task> authHandler)
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
                    authMethods = new Dictionary<string, Func<IAuthRequest, Task>>();
                }
                else if (authMethods.ContainsKey(method))
                {
                    throw new InvalidOperationException(String.Format("Auth method {0} already set.", method));
                }
                authMethods[method] = authHandler;
            }
            toggleHandlers(HandlerTypes.Auth, this.authHandler != null || authMethods != null);
            return this;
        }

        /// <summary>
        /// Sets the auth handler for a specific method, and sets the EnableHandlers bit flag appropriately
        /// </summary>
        /// <param name="method">Name of the method. Must only contain alpha-numeric characters.</param>
        /// <param name="authHandler">Auth method handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetAuthMethod(string method, Action<IAuthRequest> authHandler)
        {
            return SetAuthMethod(method, r =>
            {
                authHandler(r);
                return completedTask;
            });
        }

        /// <summary>
        /// Sets the new call handler method, and sets the EnableHandlers bit flag.
        /// </summary>
        /// <param name="newHandler">New handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetNew(Func<INewRequest, Task> newHandler)
        {
            if (this.newHandler != null)
            {
                throw new InvalidOperationException("New handler already set.");
            }
            toggleHandlers(HandlerTypes.New, true);
            this.newHandler = newHandler;
            return this;
        }

        /// <summary>
        /// Sets the new call handler method, and sets the EnableHandlers bit flag.
        /// </summary>
        /// <param name="newHandler">New handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetNew(Action<INewRequest> newHandler)
        {
            return SetNew(r =>
            {
                newHandler(r);
                return completedTask;
            });
        }

        /// <summary>
        /// Sets the apply handler method for applying any event.
        /// Will be called after any more specific apply handler.
        /// </summary>
        /// <param name="applyChangeHandler">Apply change handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApply(Func<IResourceContext, EventArgs, Task> applyHandler)
        {
            if (this.applyHandler != null)
            {
                throw new InvalidOperationException("Apply handler already set.");
            }
            this.applyHandler = applyHandler;
            return this;
        }

        /// <summary>
        /// Sets the apply handler method for applying any event.
        /// Will be called after any more specific apply handler.
        /// </summary>
        /// <param name="applyChangeHandler">Apply change handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApply(Action<IResourceContext, EventArgs> applyHandler)
        {
            return SetApply((r, ev) =>
            {
                applyHandler(r, ev);
                return completedTask;
            });
        }

        /// <summary>
        /// Sets the apply change handler method.
        /// </summary>
        /// <param name="applyChangeHandler">Apply change handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApplyChange(Func<IResourceContext, ChangeEventArgs, Task> applyChangeHandler)
        {
            if (this.applyChangeHandler != null)
            {
                throw new InvalidOperationException("Apply change handler already set.");
            }
            this.applyChangeHandler = applyChangeHandler;
            return this;
        }

        /// <summary>
        /// Sets the apply change handler method.
        /// </summary>
        /// <param name="applyChangeHandler">Apply change handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApplyChange(Action<IResourceContext, ChangeEventArgs> applyChangeHandler)
        {
            return SetApplyChange((r, ev) =>
            {
                applyChangeHandler(r, ev);
                return completedTask;
            });
        }

        /// <summary>
        /// Sets the apply add handler method.
        /// </summary>
        /// <param name="applyAddHandler">Apply add handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApplyAdd(Func<IResourceContext, AddEventArgs, Task> applyAddHandler)
        {
            if (this.applyAddHandler != null)
            {
                throw new InvalidOperationException("Apply add handler already set.");
            }
            this.applyAddHandler = applyAddHandler;
            return this;
        }

        /// <summary>
        /// Sets the apply add handler method.
        /// </summary>
        /// <param name="applyAddHandler">Apply add handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApplyAdd(Action<IResourceContext, AddEventArgs> applyAddHandler)
        {
            return SetApplyAdd((r, ev) =>
            {
                applyAddHandler(r, ev);
                return completedTask;
            });
        }

        /// <summary>
        /// Sets the apply remove handler method.
        /// </summary>
        /// <param name="applyRemoveHandler">Apply remove handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApplyRemove(Func<IResourceContext, RemoveEventArgs, Task> applyRemoveHandler)
        {
            if (this.applyRemoveHandler != null)
            {
                throw new InvalidOperationException("Apply remove handler already set.");
            }
            this.applyRemoveHandler = applyRemoveHandler;
            return this;
        }

        /// <summary>
        /// Sets the apply remove handler method.
        /// </summary>
        /// <param name="applyRemoveHandler">Apply remove handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApplyRemove(Action<IResourceContext, RemoveEventArgs> applyRemoveHandler)
        {
            return SetApplyRemove((r, ev) =>
            {
                applyRemoveHandler(r, ev);
                return completedTask;
            });
        }

        /// <summary>
        /// Sets the apply create handler method.
        /// </summary>
        /// <param name="applyCreateHandler">Apply create handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApplyCreate(Func<IResourceContext, CreateEventArgs, Task> applyCreateHandler)
        {
            if (this.applyCreateHandler != null)
            {
                throw new InvalidOperationException("Apply create handler already set.");
            }
            this.applyCreateHandler = applyCreateHandler;
            return this;
        }

        /// <summary>
        /// Sets the apply create handler method.
        /// </summary>
        /// <param name="applyCreateHandler">Apply create handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApplyCreate(Action<IResourceContext, CreateEventArgs> applyCreateHandler)
        {
            return SetApplyCreate((r, ev) =>
            {
                applyCreateHandler(r, ev);
                return completedTask;
            });
        }

        /// <summary>
        /// Sets the apply delete handler method.
        /// </summary>
        /// <param name="applyDeleteHandler">Apply delete handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApplyDelete(Func<IResourceContext, DeleteEventArgs, Task> applyDeleteHandler)
        {
            if (this.applyDeleteHandler != null)
            {
                throw new InvalidOperationException("Apply delete handler already set.");
            }
            this.applyDeleteHandler = applyDeleteHandler;
            return this;
        }

        /// <summary>
        /// Sets the apply delete handler method.
        /// </summary>
        /// <param name="applyDeleteHandler">Apply delete handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApplyDelete(Action<IResourceContext, DeleteEventArgs> applyDeleteHandler)
        {
            return SetApplyDelete((r, ev) =>
            {
                applyDeleteHandler(r, ev);
                return completedTask;
            });
        }

        /// <summary>
        /// Sets the apply custom handler method.
        /// </summary>
        /// <param name="applyCustomHandler">Apply custom handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApplyCustom(Func<IResourceContext, CustomEventArgs, Task> applyCustomHandler)
        {
            if (this.applyCustomHandler != null)
            {
                throw new InvalidOperationException("Apply custom handler already set.");
            }
            this.applyCustomHandler = applyCustomHandler;
            return this;
        }

        /// <summary>
        /// Sets the apply custom handler method.
        /// </summary>
        /// <param name="applyCustomHandler">Apply custom handler.</param>
        /// <returns>This instance.</returns>
        public DynamicHandler SetApplyCustom(Action<IResourceContext, CustomEventArgs> applyCustomHandler)
        {
            return SetApplyCustom((r, ev) =>
            {
                applyCustomHandler(r, ev);
                return completedTask;
            });
        }

        public async Task Handle(IRequest r)
        {
            switch (r.Type)
            {
                case RequestType.Access:
                    await accessHandler.Invoke((IAccessRequest)r);
                    break;
                case RequestType.Get:
                    await getHandler.Invoke((IGetRequest)r);
                    break;
                case RequestType.Call:
                    await handleCall(r);
                    break;
                case RequestType.Auth:
                    await handleAuth(r);
                    break;
            }
        }

        public async Task Apply(IResourceContext r, EventArgs ev)
        {
            switch (ev)
            {
                case ChangeEventArgs change:
                    if (applyChangeHandler != null)
                    {
                        await applyChangeHandler(r, change);
                    }
                    break;
                case AddEventArgs add:
                    if (applyAddHandler != null)
                    {
                        await applyAddHandler(r, add);
                    }
                    break;
                case RemoveEventArgs remove:
                    if (applyRemoveHandler != null)
                    {
                        await applyRemoveHandler(r, remove);
                    }
                    break;
                case CreateEventArgs create:
                    if (applyCreateHandler != null)
                    {
                        await applyCreateHandler(r, create);
                    }
                    break;
                case DeleteEventArgs delete:
                    if (applyDeleteHandler != null)
                    {
                        await applyDeleteHandler(r, delete);
                    }
                    break;
                case CustomEventArgs custom:
                    if (applyCustomHandler != null)
                    {
                        await applyCustomHandler(r, custom);
                    }
                    break;
            }
            applyHandler?.Invoke(r, ev);
        }

        /// <summary>
        /// Method called on an auth request.
        /// </summary>
        /// <param name="request">Auth request context.</param>
        private async Task handleAuth(IAuthRequest request)
        {
            if (authMethods != null)
            {
                if (authMethods.TryGetValue(request.Method, out Func<IAuthRequest, Task> handler))
                {
                    await handler(request);
                    return;
                }
            }
            if (authHandler != null)
            {
                await authHandler.Invoke(request);
            }
            else
            {
                request.MethodNotFound();
            }
        }

        /// <summary>
        /// Method called on a call request.
        /// </summary>
        /// <param name="request">Call request context.</param>
        private async Task handleCall(ICallRequest request)
        {
            if (callMethods != null)
            {
                if (callMethods.TryGetValue(request.Method, out Func<ICallRequest, Task> handler))
                {
                    await handler(request);
                    return;
                }
            }
            if (callHandler != null)
            {
                await callHandler.Invoke(request);
            }
            else
            {
                request.MethodNotFound();
            }
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
