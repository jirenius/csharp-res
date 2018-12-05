using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ResgateIO.Service
{
    public interface IAccessRequest
    {
        string ResourceName { get; }
        Dictionary<string, string> PathParams { get; }
        string Query { get; }

        /// <summary>
        /// Connection ID of the requesting client connection.
        /// </summary>
        string CID { get; }

        /// <summary>
        /// JSON encoded access token, or nil if the request had no token.
        /// </summary>
        JRaw RawToken { get; }

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
    }
}