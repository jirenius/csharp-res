using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace ResgateIO.Service
{
    /// <summary>
    /// Provides a base class for resource handler classes.
    /// </summary>
    /// <remarks>
    /// The BaseHandler constructor will check if any of its IResourceHandler methods are overridden,
    /// and enable each corresponding bit flag in EnabledHandlers.
    /// Additionally, it will search for any public instance method in the derived class that matches
    /// the signature of a Call or Auth handler. Matching methods will be invoked on call and auth
    /// requests if the name of the class method, with first letter lowercase, matches the method
    /// of the call or auth request.
    /// A different method name can set using the CallMethod and AuthMethod attributes.
    /// If any matching call or auth methods are found, each corresponding bit flag in EnabledHandlers
    /// will be set.
    /// </remarks>
    /// <example> 
    /// This sample shows how to define call and auth methods.
    /// <code>
    /// class TestHandler : BaseHandler
    /// {
    ///     // Will be called on call requests for method "foo".
    ///     public void Foo(ICallRequest request)
    ///     {
    ///         request.Ok();
    ///     }
    ///     
    ///     // Will be called async on call requests for method "baz".
    ///     [CallMethod("baz")]
    ///     public async Task Bar(ICallRequest request)
    ///     {
    ///         await Task.Delay(100);
    ///         request.Ok();
    ///     }
    ///     
    ///     // Will be ignored and not called on auth requests.
    ///     [AuthMethod(Ignore = true)]
    ///     public void Baz(IAuthRequest request) { }
    /// }
    /// </code>
    /// </example>
    public abstract class BaseHandler: IAsyncHandler
    {
        private readonly DynamicHandler handler = new DynamicHandler();
        private readonly Task completedTask = Task.FromResult(false);

        /// <summary>
        /// Initializes a new instance of the BaseHandler class.
        /// </summary>
        public BaseHandler() : this(ResourceType.Unknown)
        {
        }

        /// <summary>
        /// Initializes a new instance of the BaseHandler class, with resource type being specified.
        /// </summary>
        /// <param name="type">Resource type.</param>
        public BaseHandler(ResourceType type)
        {
            handler.SetType(type);

            MethodInfo[] methods = this.GetType().GetTypeInfo().GetMethods(BindingFlags.Instance | BindingFlags.Public);
            foreach (MethodInfo method in methods)
            {
                matchHandler(method);
            }
        }

        private void matchHandler(MethodInfo m)
        {
            ParameterInfo[] args = m.GetParameters();
            
            if (args.Length == 1)
            {
                if (tryMatchRequestHandler(m, args[0]))
                {
                    return;
                }
                var t = args[0].ParameterType;
                if (t == typeof(ICallRequest))
                {
                    if (tryMatchCallRequestHandler(m))
                    {
                        return;
                    }
                }
                else if(t == typeof(IAuthRequest))
                {
                    if (tryMatchAuthRequestHandler(m))
                    {
                        return;
                    }
                }
            }
            else if (args.Length == 2 && args[0].ParameterType == typeof(IResourceContext))
            {
                if (tryMatchApplyHandler(m, args[1]))
                {
                    return;
                }
            }

            validateAttributes(m, null);
        }

        /// <summary>
        /// Gets the resource type associated with the resource handler.
        /// </summary>
        public virtual ResourceType Type { get { return handler.Type; } }

        /// <summary>
        /// Gets the enabled handler based on the handler methods being overridden.
        /// </summary>
        public virtual HandlerTypes EnabledHandlers { get { return handler.EnabledHandlers; } }

        /// <summary>
        /// Apply change event handler doing nothing.
        /// </summary>
        /// <param name="resource">Resource to apply the change to.</param>
        /// <param name="changes">Property values to apply to model.</param>
        /// <returns>A null Task.</returns>
        public virtual Task<Dictionary<string, object>> ApplyChange(IResourceContext resource, IDictionary<string, object> changes)
        {
            return null;
        }

        /// <summary>
        /// Apply change event handler doing nothing.
        /// </summary>
        /// <param name="resource">Resource to add the value to.</param>
        /// <param name="value">Value to add.</param>
        /// <param name="idx">Index position where to add the value.</param>
        /// <returns>A null Task.</returns>
        public virtual Task ApplyAdd(IResourceContext resource, object value, int idx)
        {
            return null;
        }

        /// <summary>
        /// Apply remove event handler doing nothing.
        /// </summary>
        /// <param name="resource">Resource to remove the value from.</param>
        /// <param name="idx">Index position of the value to remove.</param>
        /// <returns>A null Task.</returns>
        public virtual Task<object> ApplyRemove(IResourceContext resource, int idx)
        {
            return null;
        }

        /// <summary>
        /// Apply create event handler doing nothing.
        /// </summary>
        /// <param name="resource">Resource to create.</param>
        /// <param name="data">The resource data object.</param>
        /// <returns>A null Task.</returns>
        public virtual Task ApplyCreate(IResourceContext resource, object data)
        {
            return null;
        }

        /// <summary>
        /// Apply delete event handler doing nothing.
        /// </summary>
        /// <param name="resource">Resource to delete.</param>
        /// <returns>A null Task.</returns>
        public virtual Task<object> ApplyDelete(IResourceContext resource)
        {
            return null;
        }

        public async Task Handle(IRequest r)
        {
            await handler.Handle(r);
        }

        public async Task Apply(IResourceContext r, EventArgs ev)
        {
            await handler.Apply(r, ev);
        }

        private bool tryMatchRequestHandler(MethodInfo m, ParameterInfo p)
        {
            var t = p.ParameterType;
            if (t != typeof(IAccessRequest) &&
                t != typeof(IGetRequest) &&
                t != typeof(ICallRequest) &&
                t != typeof(IAuthRequest) &&
                t != typeof(IModelRequest) &&
                t != typeof(ICollectionRequest) &&
                t != typeof(INewRequest))
            {
                return false;
            }

            var attr = m.GetCustomAttribute<RequestHandlerAttribute>();
            if (attr == null && (
                t == typeof(ICallRequest) ||
                t == typeof(IAuthRequest)))
            {
                return false;
            }

            validateAttributes(m, typeof(RequestHandlerAttribute));
            if (attr != null && attr.Ignore)
            {
                return true;
            }
            
            if (t == typeof(IAccessRequest))
            {
                handler.Access(createRequestHandler<IAccessRequest>(m));
            }
            else if (t == typeof(IGetRequest))
            {
                handler.Get(createRequestHandler<IGetRequest>(m));
            }
            else if (t == typeof(ICallRequest))
            {
                handler.Call(createRequestHandler<ICallRequest>(m));
            }
            else if (t == typeof(IAuthRequest))
            {
                handler.Auth(createRequestHandler<IAuthRequest>(m));
            }
            else if (t == typeof(IModelRequest))
            {
                handler.ModelGet(createRequestHandler<IModelRequest>(m));
            }
            else if (t == typeof(ICollectionRequest))
            {
                handler.CollectionGet(createRequestHandler<ICollectionRequest>(m));
            }
            else // if (t == typeof(INewRequest))
            {
                handler.New(createRequestHandler<INewRequest>(m));
            }

            return true;
        }

        private bool tryMatchCallRequestHandler(MethodInfo m)
        {
            var attr = m.GetCustomAttribute<CallMethodAttribute>();
            validateAttributes(m, typeof(CallMethodAttribute));
            if (attr != null && attr.Ignore)
            {
                return true;
            }
            handler.CallMethod(getMethodName(m, attr?.MethodName), createRequestHandler<ICallRequest>(m));
            return true;
        }

        private bool tryMatchAuthRequestHandler(MethodInfo m)
        {
            var attr = m.GetCustomAttribute<AuthMethodAttribute>();
            validateAttributes(m, typeof(AuthMethodAttribute));
            if (attr != null && attr.Ignore)
            {
                return true;
            }
            handler.AuthMethod(getMethodName(m, attr?.MethodName), createRequestHandler<IAuthRequest>(m));
            return true;
        }

        private bool tryMatchApplyHandler(MethodInfo m, ParameterInfo p)
        {
            var t = p.ParameterType;
            if (t != typeof(ChangeEventArgs) &&
                t != typeof(AddEventArgs) &&
                t != typeof(RemoveEventArgs) &&
                t != typeof(CreateEventArgs) &&
                t != typeof(DeleteEventArgs) &&
                t != typeof(CustomEventArgs))
            {
                return false;
            }

            validateAttributes(m, typeof(ApplyHandlerAttribute));

            var attr = m.GetCustomAttribute<ApplyHandlerAttribute>();
            if (attr != null && attr.Ignore)
            {
                return true;
            }

            if (t == typeof(ChangeEventArgs))
            {
                handler.ApplyChange(createApplyHandler<ChangeEventArgs>(m));
            }
            else if (t == typeof(AddEventArgs))
            {
                handler.ApplyAdd(createApplyHandler<AddEventArgs>(m));
            }
            else if (t == typeof(RemoveEventArgs))
            {
                handler.ApplyRemove(createApplyHandler<RemoveEventArgs>(m));
            }
            else if (t == typeof(CreateEventArgs))
            {
                handler.ApplyCreate(createApplyHandler<CreateEventArgs>(m));
            }
            else if (t == typeof(DeleteEventArgs))
            {
                handler.ApplyDelete(createApplyHandler<DeleteEventArgs>(m));
            }
            else // if (t == typeof(CustomEventArgs))
            {
                handler.ApplyCustom(createApplyHandler<CustomEventArgs>(m));
            }

            return true;
        }

        private string getMethodName(MethodInfo m, string name)
        {
            return name ?? Char.ToLower(m.Name[0]) + m.Name.Substring(1);
        }

        private Func<T, Task> createRequestHandler<T>(MethodInfo m)
        {
            if (m.ReturnType == typeof(Task))
            {
                return (Func<T, Task>)m.CreateDelegate(typeof(Func<T, Task>), this);
            }
            else if (m.ReturnType == typeof(void))
            {
                var d = (Action<T>)m.CreateDelegate(typeof(Action<T>), this);
                return r =>
                {
                    d(r);
                    return completedTask;
                };
            }
            throw new InvalidOperationException("Request handler must either return void or Task.");
        }

        private Func<IResourceContext, T, Task> createApplyHandler<T>(MethodInfo m)
        {
            if (m.ReturnType == typeof(Task))
            {
                return (Func<IResourceContext, T, Task>)m.CreateDelegate(typeof(Func<IResourceContext, T, Task>), this);
            }
            else if (m.ReturnType == typeof(void))
            {
                var d = (Action<IResourceContext, T>)m.CreateDelegate(typeof(Action<IResourceContext, T>), this);
                return (r, ev) =>
                {
                    d(r, ev);
                    return completedTask;
                };
            }
            throw new InvalidOperationException("Apply handler must either return void or Task.");
        }

        private void validateAttributes(MethodInfo m, Type excluded)
        {
            foreach (var attr in m.GetCustomAttributes())
            {
                var t = attr.GetType();
                if (excluded != null && t == excluded)
                {
                    continue;
                }

                if (t == typeof(RequestHandlerAttribute) ||
                    t == typeof(CallMethodAttribute) ||
                    t == typeof(AuthMethodAttribute) ||
                    t == typeof(ApplyHandlerAttribute))
                {
                    throw new InvalidOperationException(String.Format("Mismatching signature of method {0} with {1}.", m.Name, t.Name));
                }
            }
        }
    }
}
