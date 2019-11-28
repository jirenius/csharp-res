using System;
using System.Threading.Tasks;

namespace ResgateIO.Service
{
    /// <summary>
    /// Defines the methods that are required for an asynchronous handler.
    /// </summary>
    public interface IAsyncHandler
    {
        /// <summary>
        /// Gets the resource type associated with the handler.
        /// </summary>
        ResourceType Type { get; }

        /// <summary>
        /// Gets the enabled handler types.
        /// </summary>
        HandlerTypes EnabledHandlers { get; }

        /// <summary>
        /// Handles the specified request.
        /// </summary>
        /// <param name="request">The request context.</param>
        /// <returns>A task that represents the asynchronous handling.</returns>
        Task Handle(IRequest request);

        /// <summary>
        /// Applies modifying events onto the resource.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The <see cref="EventArgs"/> may be any
        /// of the following, based on type of event:
        /// <list type="bullet">
        /// <item><description><see cref="ChangeEventArgs"/></description></item>
        /// <item><description><see cref="AddEventArgs"/></description></item>
        /// <item><description><see cref="RemoveEventArgs"/></description></item>
        /// <item><description><see cref="CreateEventArgs"/></description></item>
        /// <item><description><see cref="DeleteEventArgs"/></description></item>
        /// <item><description><see cref="CustomEventArgs"/></description></item>
        /// </list>
        /// </para>
        /// <para>
        /// The information in the event arguments should be appended with
        /// additional information required to revert the modification. This is
        /// to allow middleware to roll back events in case of conflicts during
        /// synchronization between services, or for event listeners to determine
        /// prior state.
        /// </para>
        /// <para>
        /// This applies to:
        /// <list type="bullet">
        /// <item><term><see cref="ChangeEventArgs"/></term><description><see cref="ChangeEventArgs.OldProperties"/> should be set.</description></item>
        /// <item><term><see cref="RemoveEventArgs"/></term><description><see cref="RemoveEventArgs.Value"/> should be set.</description></item>
        /// <item><term><see cref="DeleteEventArgs"/></term><description><see cref="DeleteEventArgs.Data"/> should be set.</description></item>
        /// </list>
        /// </para>
        /// </remarks>
        /// <param name="resource">Resource context.</param>
        /// <param name="ev"></param>
        /// <returns>A task that represents the asynchronous handling.</returns>
        Task Apply(IResourceContext resource, EventArgs ev);
    }
}