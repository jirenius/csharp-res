using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ResgateIO.Service
{
    public interface ICallRequest : IResource
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
    }
}