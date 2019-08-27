using System;

namespace ResgateIO.Service
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CallMethod : Attribute
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
        public CallMethod()
        {
            MethodName = null;
            Ignore = false;
        }

        /// <summary>
        /// Initializes a new instance of the CallMethod class.
        /// </summary>
        /// <param name="name">Name of the resource call method.</param>
        public CallMethod(string name)
        {
            MethodName = name;
            Ignore = false;
        }
    }

    [AttributeUsage(AttributeTargets.Method)]
    public class AuthMethod : Attribute
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
        public AuthMethod()
        {
            MethodName = null;
            Ignore = false;
        }

        /// <summary>
        /// Initializes a new instance of the AuthMethod class.
        /// </summary>
        /// <param name="name">Name of the resource auth method.</param>
        public AuthMethod(string name)
        {
            MethodName = name;
            Ignore = false;
        }
    }
}
