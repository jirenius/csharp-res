using System;
using System.Collections.Generic;
using System.Reflection;

namespace ResgateIO.Service
{
    /// <summary>
    /// Provides a base class for resource handler classes.
    /// </summary>
    public abstract class BaseHandler: IResourceHandler
    {
        private readonly ResourceType resourceType;
        private readonly HandlerTypes enabledHandlers;

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
            HandlerTypes enabled = HandlerTypes.None;

            var handlers = new TupleList<string, Type[], HandlerTypes>
            {
                { "Access", new Type[] { typeof(IAccessRequest) }, HandlerTypes.Access },
                { "Get", new Type[] { typeof(IGetRequest) }, HandlerTypes.Get },
                { "Call", new Type[] { typeof(ICallRequest) }, HandlerTypes.Call },
                { "Auth", new Type[] { typeof(IAuthRequest) }, HandlerTypes.Auth },
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
                    enabled |= tuple.Item3;
                }
            }

            enabledHandlers = enabled;
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
        /// Method called on a get request.
        /// </summary>
        /// <param name="request">Get request context.</param>
        public virtual void Get(IGetRequest request)
        {
        }

        /// <summary>
        /// Method called on an access request.
        /// </summary>
        /// <param name="request">Access request context.</param>
        public virtual void Access(IAccessRequest request)
        {
        }

        /// <summary>
        /// Method called on an auth request.
        /// </summary>
        /// <param name="request">Auth request context.</param>
        public virtual void Auth(IAuthRequest request)
        {

        }

        /// <summary>
        /// Method called on a call request.
        /// </summary>
        /// <param name="request">Call request context.</param>
        public virtual void Call(ICallRequest request)
        {
        }

        /// <summary>
        /// Method called to apply a model change event.
        /// </summary>
        /// <param name="resource">Resource to apply the change to.</param>
        /// <param name="changes">Property values to apply to model.</param>
        /// <returns>A dictionary with the values to apply to revert the changes.</returns>
        public virtual Dictionary<string, object> ApplyChange(IResourceContext resource, IDictionary<string, object> changes)
        {
            return null;
        }
        
        /// <summary>
        /// Method called to apply a collection add event.
        /// </summary>
        /// <param name="resource">Resource to add the value to.</param>
        /// <param name="value">Value to add.</param>
        /// <param name="idx">Index position where to add the value.</param>
        public virtual void ApplyAdd(IResourceContext resource, object value, int idx)
        {
        }

        /// <summary>
        /// Method called to apply a collection remove event.
        /// </summary>
        /// <param name="resource">Resource to remove the value from.</param>
        /// <param name="idx">Index position of the value to remove.</param>
        /// <returns>The removed value.</returns>
        public virtual object ApplyRemove(IResourceContext resource, int idx)
        {
            return null;
        }

        /// <summary>
        /// Method called to apply a resource create event.
        /// </summary>
        /// <param name="resource">Resource to create.</param>
        /// <param name="data">The resource data object.</param>
        public virtual void ApplyCreate(IResourceContext resource, object data)
        {
        }
        
        /// <summary>
        /// Method called to apply a resource delete event.
        /// </summary>
        /// <param name="resource">Resource to delete.</param>
        /// <returns>The deleted resource data object.</returns>
        public virtual object ApplyDelete(IResourceContext resource)
        {
            return null;
        }


        private bool isMethodOverridden(string methodName, Type[] types)
        {
            MethodInfo m = this.GetType().GetMethod(methodName, types);
            return m.GetBaseDefinition().DeclaringType != m.DeclaringType;
        }

        private bool isPropertyOverridden(string propertyName)
        {
            MethodInfo m = this.GetType().GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly).GetGetMethod(false);
            return m.GetBaseDefinition().DeclaringType != m.DeclaringType;
        }
    }
}
