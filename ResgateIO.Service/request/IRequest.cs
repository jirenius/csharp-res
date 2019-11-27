using System;
using System.Collections.Generic;
using System.Text;
using NATS.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ResgateIO.Service
{
    /// <summary>
    /// Provides context information and methods for responding to a request.
    /// </summary>
    public interface IRequest: IResourceContext
    {
        /// <summary>
        /// Type of request.
        /// </summary>
        RequestType Type { get; }

        /// <summary>
        /// Resource method.
        /// This property is not set for RequestType.Access and RequestType.Get.
        /// </summary>
        string Method { get; }

        /// <summary>
        /// Connection ID of the requesting client connection.
        /// </summary>
        /// <remarks>
        /// This property is not set for RequestType.Get.
        /// </remarks>
        string CID { get; }

        /// <summary>
        /// Gets the method parameters, or null if the request had no parameters.
        /// </summary>
        /// <remarks>
        /// This property is not set for RequestType.Access and RequestType.Get.
        /// </remarks>
        JToken Params { get; }

        /// <summary>
        /// Gets the access token, or null if the request had no token.
        /// </summary>
        /// <remarks>
        /// This property is not set for RequestType.Get.
        /// </remarks>
        JToken Token { get; }

        /// <summary>
        /// HTTP headers sent by client on connect.
        /// This property is only set for RequestType.Auth.
        /// </summary>
        Dictionary<string, string[]> Header { get; }

        /// <summary>
        /// The host on which the URL is sought by the client. Per RFC 2616,
        /// this is either the value of the "Host" header or the host name given
        /// in the URL itself.
        /// This property is only set for RequestType.Auth.
        /// </summary>
        string Host { get; }

        /// <summary>
        /// The network address of the client sent on connect.
        /// The format is not specified.
        /// This property is only set for RequestType.Auth.
        /// </summary>
        string RemoteAddr { get; }

        /// <summary>
        /// The unmodified Request-URI of the Request-Line (RFC 2616, Section 5.1)
        /// as sent by the client when on connect.
        /// This property is only set for RequestType.Auth.
        /// </summary>
        string URI { get; }

        /// <summary>
        /// Flag telling if the request handler is called as a result of Value
        /// or RequireValue being called from another handler.
        /// </summary>
        bool ForValue { get; }

        /// <summary>
        /// Sends a raw RES protocol response to a request.
        /// Throws an exception if a response has already been sent.
        /// </summary>
        /// <remarks>
        /// Only use this method if you are familiar with the RES protocol,
        /// and you know what you are doing.
        /// </remarks>
        /// <param name="data">JSON encoded RES response data. Text encoding must be UTF8 without BOM.</param>
        void RawResponse(byte[] data);

        /// <summary>
        /// Sends a successful empty response to a request.
        /// </summary>
        void Ok();

        /// <summary>
        /// Sends a successful response to a request.
        /// </summary>
        /// <param name="result">Result object. May be null.</param>
        void Ok(object result);

        /// <summary>
        /// Sends a successful response to a new call request.
        /// </summary>
        /// <remarks>Only valid for new call requests.</remarks>
        /// <param name="resourceID">Valid resource ID to the newly created resource.</param>
        void New(Ref resourceID);

        /// <summary>
        /// Sends an error response to a request.
        /// </summary>
        void Error(ResError error);

        /// <summary>
        /// Sends a system.notFound response.
        /// </summary>
        void NotFound();

        /// <summary>
        /// Sends a system.methodNotFound response.
        /// </summary>
        /// <remarks>
        /// Only valid for RequestType.Call and RequestType.Auth.
        /// </remarks>
        void MethodNotFound();

        /// <summary>
        /// Sends a system.invalidParams response with a default error message.
        /// </summary>
        /// <remarks>
        /// Only valid for RequestType.Call and RequestType.Auth.
        /// </remarks>
        void InvalidParams();

        /// <summary>
        /// Sends a system.invalidParams response with a custom error message.
        /// </summary>
        /// <remarks>
        /// Only valid for RequestType.Call and RequestType.Auth.
        /// </remarks>
        /// <param name="message">Error message.</param>
        void InvalidParams(string message);

        /// <summary>
        /// Sends a system.invalidParams response with a custom error message and data.
        /// </summary>
        /// <remarks>
        /// Only valid for RequestType.Call and RequestType.Auth.
        /// </remarks>
        /// <param name="message">Error message.</param>
        /// <param name="data">Additional data. Must be JSON serializable.</param>
        void InvalidParams(string message, object data);

        /// <summary>
        /// Sends a system.invalidQuery response with a default error message.
        /// </summary>
        void InvalidQuery();

        /// <summary>
        /// Sends a system.invalidQuery response with a custom error message.
        /// </summary>
        /// <param name="message">Error message.</param>
        void InvalidQuery(string message);

        /// <summary>
        /// Sends a system.invalidQuery response with a custom error message and data.
        /// </summary>
        /// <param name="message">Error message.</param>
        /// <param name="data">Additional data. Must be JSON serializable.</param>
        void InvalidQuery(string message, object data);

        /// <summary>
        /// Sends a successful response for the access request.
        /// The get flag tells if the client has access to get (read) the resource.
        /// The call string is a comma separated list of methods that the client can
        /// call. Eg. "set,foo,bar". A single asterisk character ("*") means the client
        /// is allowed to call any method. Empty string or null means no calls are allowed.
        /// </summary>
        /// <remarks>Only valid for RequestType.Access requests.</remarks>
        /// <param name="get">Get access flag</param>
        /// <param name="call">Accessible call methods as a comma separated list</param>
        void Access(bool get, string call);

        /// <summary>
        /// Sends a system.accessDenied response.
        /// </summary>
        /// <remarks>Only valid for RequestType.Access requests.</remarks>
        void AccessDenied();

        /// <summary>
        /// Sends a successful response granting full access to the resource.
        /// Same as calling Access(true, "*");
        /// </summary>
        /// <remarks>Only valid for RequestType.Access requests.</remarks>
        void AccessGranted();

        /// <summary>
        /// Sends a successful model response for the get request.
        /// The model must be serializable into a JSON object.
        /// </summary>
        /// <remarks>Only valid for RequestType.Get requests for a model resource.</remarks>
        /// <param name="model">Model data</param>
        void Model(object model);

        // <summary>
        /// Sends a successful query model response for the get request.
        /// The model must be serializable into a JSON object.
        /// </summary>
        /// <remarks>Only valid for RequestType.Get requests for a model resource.</remarks>
        /// <param name="model">Model data</param>
        /// <param name="query">Normalized query</param>
        void Model(object model, string query);

        /// <summary>
        /// Sends a successful collection response for the get request.
        /// The collection must be serializable into a JSON array.
        /// </summary>
        /// <remarks>Only valid for RequestType.Get requests for a collection resource.</remarks>
        /// <param name="collection">Collection data</param>
        void Collection(object collection);

        // <summary>
        /// Sends a successful query collection response for the get request.
        /// The collection must be serializable into a JSON array.
        /// </summary>
        /// <remarks>Only valid for RequestType.Get requests for a collection resource.</remarks>
        /// <param name="collection">Collection data</param>
        /// <param name="query">Normalized query</param>
        void Collection(object collection, string query);

        /// <summary>
        /// Deserializes the parameters into an object of type T.
        /// </summary>
        /// <remarks>Only valid for RequestType.Call and RequestType.Auth requests.</remarks>
        /// <typeparam name="T">Type to parse the parameters into.</typeparam>
        /// <returns>An object with the parameters, or default value on null parameters.</returns>
        T ParseParams<T>();

        /// <summary>
        /// Deserializes the token into an object of type T.
        /// </summary>
        /// <remarks>Not valid for RequestType.Get requests.</remarks>
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

        /// <summary>
        /// Sends a connection token event that sets the connection's access token,
        /// discarding any previously set token.
        /// A change of token will invalidate any previous access response received using the old token.
        /// </summary>
        /// <remarks>
        /// To set the connection token for a different connection ID, use ResService.TokenEvent.
        /// Only valid for RequestType.Auth requests.
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#connection-token-event
        /// </remarks>
        /// <param name="token">Access token. A null token clears any previously set token.</param>
        void TokenEvent(object token);

        /// <summary>
        /// Creates a new object that is a copy of the current IResourceContext,
        /// with the exception of the Query string and the Item context.
        /// </summary>
        /// <param name="query">Query string to use for the clone.</param>
        /// <returns>A new object that is a copy of the IResourceContext instance.</returns>
        IResourceContext CloneWithQuery(string query);
    }
}
