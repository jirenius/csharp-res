using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ResgateIO.Service
{
    public interface IAuthRequest : IResourceRequest
    {
        /// <summary>
        /// Resource method.
        /// </summary>
        string Method { get; }

        /// <summary>
        /// Connection ID of the requesting client connection.
        /// </summary>
        string CID { get; }

        /// <summary>
        /// JSON encoded access token, or nil if the request had no token.
        /// </summary>
        JToken RawToken { get; }

        /// <summary>
        /// JSON encoded parameters, or nil if the request had no parameters.
        /// </summary>
        JToken RawParams { get; }

        /// <summary>
        /// HTTP headers sent by client on connect.
        /// </summary>
        Dictionary<string, string[]> Header { get; }

        /// <summary>
        /// The host on which the URL is sought by the client. Per RFC 2616,
        /// this is either the value of the "Host" header or the host name given
        /// in the URL itself.
        /// </summary>
        string Host { get; }

        /// <summary>
        /// The network address of the client sent on connect.
        /// The format is not specified.
        /// </summary>
        string RemoteAddr { get; }

        /// <summary>
        /// The unmodified Request-URI of the Request-Line (RFC 2616, Section 5.1)
        /// as sent by the client when on connect.
        /// </summary>
        string URI { get; }

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
        /// Sends an error response to the request.
        /// </summary>
        void Error(ResError error);

        /// <summary>
        /// Sends a system.notFound response.
        /// </summary>
        void NotFound();

        /// <summary>
        /// Sends a system.methodNotFound response.
        /// </summary>
        void MethodNotFound();

        /// <summary>
        /// Sends a system.invalidParams response with a default error message.
        /// </summary>
        void InvalidParams();

        /// <summary>
        /// Sends a system.invalidParams response with a custom error message.
        /// </summary>
        void InvalidParams(string message);

        /// <summary>
        /// Deserializes the parameters into an object of type T.
        /// </summary>
        /// <typeparam name="T">Type to parse the parameters into.</typeparam>
        /// <returns>An object with the parameters.</returns>
        T ParseParams<T>();

        /// <summary>
        /// Deserializes the token into an object of type T.
        /// </summary>
        /// <typeparam name="T">Type to parse the token into.</typeparam>
        /// <returns>Parsed token object.</returns>
        T ParseToken<T>();

        /// <summary>
        /// Sends a connection token event that sets the connection's access token,
        /// discarding any previously set token.
        /// A change of token will invalidate any previous access response received using the old token.
        /// </summary>
        /// <remarks>
        /// To set the connection token for a different connection ID, use ResService.ConnectionTokenEvent.
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#connection-token-event
        /// </remarks>
        /// <param name="token">Access token. A null token clears any previously set token.</param>
        void ConnectionTokenEvent(object token);
    }
}