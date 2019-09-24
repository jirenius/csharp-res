using System;

namespace ResgateIO.Service
{
    /// <summary>
    /// Specifies a pattern attribute to a <see cref="IResourceHandler"/> class, which will be used
    /// as subpattern by <see cref="Router.AddHandler"/>, when called without an explicitly given subpattern.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ResourcePatternAttribute : Attribute
    {
        /// <summary>
        /// Resource pattern.
        /// </summary>
        public virtual string Pattern { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourcePatternAttribute"/>.
        /// 
        /// A pattern may contain placeholders that acts as wildcards, and will be
        /// parsed and stored in the request.PathParams dictionary.
        /// A placeholder is a resource name part starting with a dollar ($) character:
        /// <code>[ResourcePattern("user.$id")] // Will match "user.10", "user.foo", etc.</code>
        /// An anonymous placeholder is a resource name part using an asterisk (*) character:
        /// <code>[ResourcePattern("user.*")]   // Will match "user.10", "user.foo", etc.</code>
        /// A full wildcard can be used as last part using a greather than (>) character:
        /// <code>[ResourcePattern("data.>")]   // Will match "data.foo", "data.foo.bar", etc.</code>
        /// </summary>
        /// <param name="pattern">Resource pattern.</param>
        public ResourcePatternAttribute(string pattern)
        {
            Pattern = pattern;
        }
    }

    /// <summary>
    /// Specifies a group attribute to a <see cref="IResourceHandler"/> class, which will be used
    /// as group pattern by <see cref="Router.AddHandler"/>, when called without an explicitly given group.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ResourceGroupAttribute : Attribute
    {
        /// <summary>
        /// Resource group.
        /// </summary>
        public virtual string Group { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceGroupAttribute"/>.
        /// 
        /// All resources of the same group will be handled in order on a single worker task.
        /// The group may contain tags, <code>${tagName}</code>, where the tag name matches a parameter
        /// placeholder name in the resource pattern.
        /// </summary>
        /// <param name="group">Resource group.</param>
        public ResourceGroupAttribute(string group)
        {
            Group = group;
        }
    }

    /// <summary>
    /// 
    /// Specifies an attribute event listener to a method matching the <see cref="EventHandler"/> delegate, on a <see cref="IResourceHandler"/> class.
    /// When the handler is registered with <see cref="Router.AddHandler"/>, the method will be added as a handler for events
    /// on the resources matching the subpattern.
    /// The subpattern must be an exact match of a registered resource, including any placeholder tags.
    /// 
    /// The sender will always implement the <see cref="IResourceContext"/>, and the <see cref="EventArgs"/> may be any
    /// of the following, based on type of event:
    /// <list type="bullet">
    /// <item><description><see cref="ChangeEventArgs"/></description></item>
    /// <item><description><see cref="AddEventArgs"/></description></item>
    /// <item><description><see cref="RemoveEventArgs"/></description></item>
    /// <item><description><see cref="CreateEventArgs"/></description></item>
    /// <item><description><see cref="DeleteEventArgs"/></description></item>
    /// <item><description><see cref="CustomEventArgs"/></description></item>
    /// </list> 
    /// </summary>
    /// <example><code>
    /// [EventListener("foo.$id")]
    /// public void OnFooEvent(object sender, EventArgs ev)
    /// {
    ///     var resource = (IResourceContext)sender;
    ///     switch (ev)
    ///     {
    ///         case ChangeEventArgs change:
    ///             // Handle change event
    ///             break;
    ///         case CustomEventArgs custom:
    ///             // Handle custom event
    ///             break;
    ///         default:
    ///             // Default handling
    ///     }
    /// }
    /// </code></example>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class EventListenerAttribute : Attribute
    {
        /// <summary>
        /// Resource pattern.
        /// </summary>
        public virtual string Pattern { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="EventListenerAttribute"/>.
        /// <param name="pattern">Resource pattern.</param>
        public EventListenerAttribute(string pattern)
        {
            Pattern = pattern;
        }
    }
}
