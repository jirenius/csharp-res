using System;
using Newtonsoft.Json;

namespace ResgateIO.Service
{
    /// <summary>
    /// Represents a RES service error.
    /// </summary>
    /// <see>https://resgate.io/docs/specification/res-service-protocol/#error-object</see>
    public class ResError
    {
        // Predefined error codes
        public const string CodeAccessDenied = "system.accessDenied";
        public const string CodeInternalError = "system.internalError";
        public const string CodeInvalidParams = "system.invalidParams";
        public const string CodeInvalidQuery = "system.invalidQuery";
        public const string CodeMethodNotFound = "system.methodNotFound";
        public const string CodeNotFound = "system.notFound";
        public const string CodeTimeout = "system.timeout";

        // Predefined errors
        public static readonly ResError AccessDenied = new ResError(CodeAccessDenied, "Not found");
        public static readonly ResError InternalError = new ResError(CodeInternalError, "Internal error");
        public static readonly ResError InvalidParams = new ResError(CodeInvalidParams, "Invalid parameters");
        public static readonly ResError InvalidQuery = new ResError(CodeInvalidQuery, "Invalid query");
        public static readonly ResError MethodNotFound = new ResError(CodeMethodNotFound, "Method not found");
        public static readonly ResError NotFound = new ResError(CodeNotFound, "Not found");
        public static readonly ResError Timeout = new ResError(CodeTimeout, "Request timeout");

        [JsonProperty(PropertyName = "code")]
        public string Code;

        [JsonProperty(PropertyName = "message")]
        public string Message;

        [JsonProperty(PropertyName = "data", NullValueHandling = NullValueHandling.Ignore)]
        public object Data = null;

        /// <summary>
        /// Initializes a new instance of the ResError class with CodeInternalError.
        /// </summary>
        /// <param name="ex">Exception to base the error message on.</param>
        public ResError(Exception ex)
        {
            Code = ResError.CodeInternalError;
            Message = ex.Message;
        }

        /// <summary>
        /// Initializes a new instance of the ResError class based on a ResException.
        /// </summary>
        /// <param name="ex">ResException to get the error code, message, and data from.</param>
        public ResError(ResException ex)
        {
            Code = ex.Code;
            Message = ex.Message;
            Data = ex.ErrorData;
        }

        /// <summary>
        /// Initializes a new instance of the ResError class with CodeInternalError and custom error message.
        /// </summary>
        /// <param name="message">Error message.</param>
        public ResError(string message)
        {
            Code = ResError.CodeInternalError;
            Message = message;
        }

        /// <summary>
        /// Initializes a new instance of the ResError class with custom error code and message.
        /// </summary>
        /// <param name="code">Error code.</param>
        /// <param name="message">Error message.</param>
        public ResError(string code, string message)
        {
            Code = code;
            Message = message;
        }


        /// <summary>
        /// Initializes a new instance of the ResError class with custom error code, message, and data.
        /// </summary>
        /// <param name="code">Error code.</param>
        /// <param name="message">Error message.</param>
        /// <param name="data">Additional data. Must be JSON serializable.</param>
        public ResError(string code, string message, object data)
        {
            Code = code;
            Message = message;
            Data = data;
        }
    }
}