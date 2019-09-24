using System;
using System.Collections.Generic;
using System.Reflection;

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
    ///     // Will be called on call requests for method "baz".
    ///     [CallMethod("baz")]
    ///     public void Bar(ICallRequest request)
    ///     {
    ///         request.Ok();
    ///     }
    ///     
    ///     // Will be ignored and not called on auth requests.
    ///     [AuthMethod(Ignore = true)]
    ///     public void Baz(IAuthRequest request) { }
    /// }
    /// </code>
    /// </example>
    public abstract class BaseHandler: IResourceHandler
    {
        private readonly ResourceType resourceType;
        private readonly HandlerTypes enabledHandlers;
        private Dictionary<string, Action<ICallRequest>> callMethods;
        private Dictionary<string, Action<IAuthRequest>> authMethods;

        private const string callMethodName = "Call";
        private const string authMethodName = "Auth";

        private class TupleList<T1, T2, T3> : List<Tuple<T1, T2, T3>>
        {
            public void Add(T1 item, T2 item2, T3 item3)
            {
                Add(new Tuple<T1, T2, T3>(item, item2, item3));
            }
        }

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
            resourceType = type;
            enabledHandlers = HandlerTypes.None;

            // Find overridden IResourceHandler methods
            var handlers = new TupleList<string, Type[], HandlerTypes>
            {
                { "Access", new Type[] { typeof(IAccessRequest) }, HandlerTypes.Access },
                { "Get", new Type[] { typeof(IGetRequest) }, HandlerTypes.Get },
                { "Call", new Type[] { typeof(ICallRequest) }, HandlerTypes.Call },
                { "Auth", new Type[] { typeof(IAuthRequest) }, HandlerTypes.Auth },
                { "New", new Type[] { typeof(INewRequest) }, HandlerTypes.New },
                { "ApplyChange", new Type[] { typeof(IResourceContext), typeof(IDictionary<string, object>) }, HandlerTypes.ApplyChange },
                { "ApplyAdd", new Type[] { typeof(IResourceContext), typeof(object), typeof(int) }, HandlerTypes.ApplyAdd },
                { "ApplyRemove", new Type[] { typeof(IResourceContext), typeof(int) }, HandlerTypes.ApplyRemove },
                { "ApplyCreate", new Type[] { typeof(IResourceContext), typeof(object) }, HandlerTypes.ApplyCreate },
                { "ApplyDelete", new Type[] { typeof(IResourceContext) }, HandlerTypes.ApplyDelete },
            };

            foreach (Tuple<string, Type[], HandlerTypes> tuple in handlers)
            {
                if (isMethodOverridden(tuple.Item1, tuple.Item2))
                {
                    enabledHandlers |= tuple.Item3;
                }
            }

            MethodInfo[] methods = this.GetType().GetTypeInfo().GetMethods(BindingFlags.Instance | BindingFlags.Public);
            if (findCallMethods(methods))
            {
                enabledHandlers |= HandlerTypes.Call;
            }
            if (findAuthMethods(methods))
            {
                enabledHandlers |= HandlerTypes.Auth;
            }
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
        /// Get request handler doing nothing.
        /// </summary>
        /// <param name="request">Get request context.</param>
        public virtual void Get(IGetRequest request)
        {
        }

        /// <summary>
        /// Access request handler doing nothing.
        /// </summary>
        /// <param name="request">Access request context.</param>
        public virtual void Access(IAccessRequest request)
        {
        }

        /// <summary>
        /// Call request handler.
        /// It will invoke any call method matching that of the request,
        /// or else respond with MethodNotFound.
        /// </summary>
        /// <param name="request">Call request context.</param>
        public virtual void Call(ICallRequest request)
        {
            if (callMethods != null && callMethods.TryGetValue(request.Method, out Action<ICallRequest> h))
            {
                h(request);
            }
            else
            {
                request.MethodNotFound();
            }
        }

        /// <summary>
        /// Auth request handler.
        /// It will invoke any auth method matching that of the request,
        /// or else respond with MethodNotFound.
        /// </summary>
        /// <param name="request">Auth request context.</param>
        public virtual void Auth(IAuthRequest request)
        {
            if (authMethods != null && authMethods.TryGetValue(request.Method, out Action<IAuthRequest> h))
            {
                h(request);
            }
            else
            {
                request.MethodNotFound();
            }
        }

        /// <summary>
        /// New call request handler doing nothing.
        /// </summary>
        /// <param name="request">New call request context.</param>
        public virtual void New(INewRequest request)
        {
        }

        /// <summary>
        /// Apply change event handler doing nothing.
        /// </summary>
        /// <param name="resource">Resource to apply the change to.</param>
        /// <param name="changes">Property values to apply to model.</param>
        /// <returns>A null dictionary.</returns>
        public virtual Dictionary<string, object> ApplyChange(IResourceContext resource, IDictionary<string, object> changes)
        {
            return null;
        }
        
        /// <summary>
        /// Apply change event handler doing nothing.
        /// </summary>
        /// <param name="resource">Resource to add the value to.</param>
        /// <param name="value">Value to add.</param>
        /// <param name="idx">Index position where to add the value.</param>
        public virtual void ApplyAdd(IResourceContext resource, object value, int idx)
        {
        }

        /// <summary>
        /// Apply remove event handler doing nothing.
        /// </summary>
        /// <param name="resource">Resource to remove the value from.</param>
        /// <param name="idx">Index position of the value to remove.</param>
        /// <returns>A null object.</returns>
        public virtual object ApplyRemove(IResourceContext resource, int idx)
        {
            return null;
        }

        /// <summary>
        /// Apply create event handler doing nothing.
        /// </summary>
        /// <param name="resource">Resource to create.</param>
        /// <param name="data">The resource data object.</param>
        public virtual void ApplyCreate(IResourceContext resource, object data)
        {
        }

        /// <summary>
        /// Apply delete event handler doing nothing.
        /// </summary>
        /// <param name="resource">Resource to delete.</param>
        /// <returns>The deleted resource data object.</returns>
        public virtual object ApplyDelete(IResourceContext resource)
        {
            return null;
        }

        private bool findCallMethods(MethodInfo[] methods)
        {
            foreach (MethodInfo method in methods)
            {
                CallMethodAttribute attr = method.GetCustomAttribute<CallMethodAttribute>();
                if (isResourceMethod(method, typeof(ICallRequest), attr != null, callMethodName)
                    && method.Name != callMethodName
                    && (attr == null || !attr.Ignore))
                {
                    string methodName = getMethodName(method, attr?.MethodName);
                    validateMethodName(callMethodName, callMethods != null && callMethods.ContainsKey(methodName), methodName);
                    callMethods = callMethods ?? new Dictionary<string, Action<ICallRequest>>();
                    callMethods.Add(methodName, (Action<ICallRequest>)method.CreateDelegate(typeof(Action<ICallRequest>), this));
                }
            }
            return callMethods != null;
        }

        private bool findAuthMethods(MethodInfo[] methods)
        {
            foreach (MethodInfo method in methods)
            {
                AuthMethodAttribute attr = method.GetCustomAttribute<AuthMethodAttribute>();
                if (isResourceMethod(method, typeof(IAuthRequest), attr != null, authMethodName)
                    && method.Name != authMethodName
                    && (attr == null || !attr.Ignore))
                {
                    string methodName = getMethodName(method, attr?.MethodName);
                    validateMethodName(authMethodName, authMethods != null && authMethods.ContainsKey(methodName), methodName);
                    authMethods = authMethods ?? new Dictionary<string, Action<IAuthRequest>>();
                    authMethods.Add(methodName, (Action<IAuthRequest>)method.CreateDelegate(typeof(Action<IAuthRequest>), this));
                }
            }
            return authMethods != null;
        }

        private bool isMethodOverridden(string methodName, Type[] types)
        {
            MethodInfo m = this.GetType().GetTypeInfo().GetMethod(methodName, types);
            return m.GetBaseDefinition().DeclaringType != m.DeclaringType;
        }

        private bool isResourceMethod(MethodInfo m, Type t, bool hasAttr, string reserved)
        {
            ParameterInfo[] args = m.GetParameters();
            bool isMatch = m.ReturnType == typeof(void)
                && args.Length == 1
                && args[0].ParameterType == t
                && !args[0].IsOut;

            if (hasAttr && !isMatch)
            {
                throw new InvalidOperationException(String.Format("Method {0} does not match signature required by {1}Method attribute.", m.Name, reserved));
            }
            if (hasAttr && m.Name == reserved)
            {
                throw new InvalidOperationException(String.Format("{0}Method attribute not allowed on {0} method.", reserved));
            }
            return isMatch;
        }

        private string getMethodName(MethodInfo m, string name)
        {
            return name ?? Char.ToLower(m.Name[0]) + m.Name.Substring(1);
        }

        private void validateMethodName(string reserved, bool redeclared, string name)
        {
            if (!ResService.IsValidPart(name))
            {
                throw new InvalidOperationException("Invalid method name: " + name);
            }
            if (redeclared)
            {
                throw new InvalidOperationException(String.Format("{0} resource method {1} declared multiple times.", reserved, name));
            }
        }
    }
}
