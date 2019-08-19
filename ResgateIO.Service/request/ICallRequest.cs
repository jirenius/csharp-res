using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ResgateIO.Service
{
    /// <summary>
    /// Provides context information and methods for responding to a call request.
    /// </summary>
    public interface ICallRequest : IResourceContext
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
        /// Gets the access token, or null if the request had no token.
        /// </summary>
        JToken Token { get; }

        /// <summary>
        /// Gets the method parameters, or null if the request had no parameters.
        /// </summary>
        JToken Params { get; }

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
        /// Sends a system.invalidParams response with a custom error message and data.
        /// </summary>
        void InvalidParams(string message, object data);

        /// <summary>
        /// Deserializes the parameters into an object of type T.
        /// </summary>
        /// <typeparam name="T">Type to parse the parameters into.</typeparam>
        /// <returns>An object with the parameters, or default value on null parameters.</returns>
        T ParseParams<T>();

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