using System;
using System.Threading.Tasks;

namespace ResgateIO.Service
{
    /// <summary>
    /// Identifies a method to be a request handler.
    /// </summary>
    /// <remarks>
    /// The method should belong to a class that derives from <see cref="BaseHandler"/>.
    /// The method should return either <see cref="void"/> or <see cref="Task"/>.
    /// The method should take one of the arguments below:
    /// <list type="bullet">
    /// <item><term><see cref="IAccessRequest"/></term><description>Access request handler.</description></item>
    /// <item><term><see cref="IGetRequest"/></term><description>Get request handler.</description></item>
    /// <item><term><see cref="ICallRequest"/></term><description>Call request handler.</description></item>
    /// <item><term><see cref="IAuthRequest"/></term><description>Auth request handler.</description></item>
    /// <item><term><see cref="IModelRequest"/></term><description>Model request handler.</description></item>
    /// <item><term><see cref="ICollectionRequest"/></term><description>Collection request handler.</description></item>
    /// </list>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class RequestHandlerAttribute : Attribute
    {
        /// <summary>
        /// Flag indicating if the request handler should be ignored.
        /// </summary>
        public virtual bool Ignore { get; set; }
    }

    /// <summary>
    /// Identifies a method to be a call request method handler.
    /// </summary>
    /// <remarks>
    /// The method should belong to a class that derives from <see cref="BaseHandler"/>.
    /// The method should take a <see cref="ICallRequest"/> as a single argument.
    /// The method should return either <see cref="void"/> or <see cref="Task"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class CallMethodAttribute : Attribute
    {
        /// <summary>
        /// Name of the resource call method.
        /// </summary>
        public virtual string MethodName { get; set; }

        /// <summary>
        /// Flag indicating if the method should be ignored.
        /// </summary>
        public virtual bool Ignore { get; set; }

        /// <summary>
        /// Initializes a new instance of the CallMethod class, deriving the method name from the class method with first-letter-lowercase.
        /// </summary>
        public CallMethodAttribute()
        {
            MethodName = null;
            Ignore = false;
        }

        /// <summary>
        /// Initializes a new instance of the CallMethod class.
        /// </summary>
        /// <param name="name">Name of the resource call method.</param>
        public CallMethodAttribute(string name)
        {
            MethodName = name;
            Ignore = false;
        }
    }

    /// <summary>
    /// Identifies a method to be a auth request method handler.
    /// </summary>
    /// <remarks>
    /// The method should belong to a class that derives from <see cref="BaseHandler"/>.
    /// The method should take a <see cref="IAuthRequest"/> as a single argument.
    /// The method should return either <see cref="void"/> or <see cref="Task"/>.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class AuthMethodAttribute : Attribute
    {
        /// <summary>
        /// Name of the resource auth method.
        /// </summary>
        public virtual string MethodName { get; set; }

        /// <summary>
        /// Flag indicating if the method should be ignored.
        /// </summary>
        public virtual bool Ignore { get; set; }

        /// <summary>
        /// Initializes a new instance of the AuthMethod class, deriving the method name from the class method with first-letter-lowercase.
        /// </summary>
        public AuthMethodAttribute()
        {
            MethodName = null;
            Ignore = false;
        }

        /// <summary>
        /// Initializes a new instance of the AuthMethod class.
        /// </summary>
        /// <param name="name">Name of the resource auth method.</param>
        public AuthMethodAttribute(string name)
        {
            MethodName = name;
            Ignore = false;
        }
    }

    /// <summary>
    /// Identifies a method to be an apply handler.
    /// </summary>
    /// <remarks>
    /// The method should belong to a class that derives from <see cref="BaseHandler"/>.
    /// The method should return either <see cref="void"/> or <see cref="Task"/>.
    /// The method should take two arguments, <see cref="IResourceContext"/> and one of the below:
    /// <list type="bullet">
    /// <item><term><see cref="EventArgs"/></term><description>Apply all events.</description></item>
    /// <item><term><see cref="ChangeEventArgs"/></term><description>Apply change events.</description></item>
    /// <item><term><see cref="AddEventArgs"/></term><description>Apply add events.</description></item>
    /// <item><term><see cref="RemoveEventArgs"/></term><description>Apply remove events.</description></item>
    /// <item><term><see cref="CreateEventArgs"/></term><description>Apply create events.</description></item>
    /// <item><term><see cref="DeleteEventArgs"/></term><description>Apply delete events.</description></item>
    /// <item><term><see cref="CustomEventArgs"/></term><description>Apply custom events.</description></item>
    /// </list>
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class ApplyHandlerAttribute : Attribute
    {
        /// <summary>
        /// Flag indicating if the apply handler should be ignored.
        /// </summary>
        public virtual bool Ignore { get; set; }
    }
}
