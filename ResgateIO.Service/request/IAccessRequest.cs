using Newtonsoft.Json.Linq;
using System;

namespace ResgateIO.Service
{
    /// <summary>
    /// Provides context information and methods for responding to an access request.
    /// </summary>
    public interface IAccessRequest : IResourceContext
    {
        /// <summary>
        /// Connection ID of the requesting client connection.
        /// </summary>
        string CID { get; }

        /// <summary>
        /// Gets the access token, or null if the request had no token.
        /// </summary>
        JToken Token { get; }

        /// <summary>
        /// Sends a successful response for the access request.
        /// The get flag tells if the client has access to get (read) the resource.
        /// The call string is a comma separated list of methods that the client can
        /// call. Eg. "set,foo,bar". A single asterisk character ("*") means the client
        /// is allowed to call any method. Empty string or null means no calls are allowed.
        /// </summary>
        /// <param name="get">Get access flag</param>
        /// <param name="call">Accessible call methods as a comma separated list</param>
        void Access(bool get, string call);

        /// <summary>
        /// Sends a system.accessDenied response for the access request.
        /// </summary>
        void AccessDenied();

        /// <summary>
        /// Sends a successful response granting full access to the resource.
        /// Same as calling Access(true, "*");
        /// </summary>
        void AccessGranted();

        /// <summary>
        /// Sends an error response to the request.
        /// </summary>
        void Error(ResError error);

        /// <summary>
        /// Sends a system.notFound response.
        /// </summary>
        void NotFound();

        /// <summary>
        /// Deserializes the token into an object of type T.
        /// </summary>
        /// <typeparam name="T">Type to parse the token into.</typeparam>
        /// <returns>An object with the parsed token, or default value on a null token.</returns>
        T ParseToken<T>();

        /// <summary>
        /// Attempts to set the timeout duration of the request.
        /// The call has no effect if the requester has already timed out the request,
        /// or if a reply has already been sent.
        /// </summary>
        /// <param name="milliseconds">Timeout duration in milliseconds.</param>
        void Timeout(int milliseconds);

        /// <summary>
        /// Attempts to set the timeout duration of the request.
        /// The call has no effect if the requester has already timed out the request,
        /// or if a reply has already been sent.
        /// </summary>
        /// <param name="milliseconds">Timeout duration.</param>
        void Timeout(TimeSpan duration);
    }
}