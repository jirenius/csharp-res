using System;
using Newtonsoft.Json;

namespace ResgateIO.Service
{
    public class ResError
    {
        // Predefined error codes
        public const string CodeAccessDenied = "system.accessDenied";
        public const string CodeInternalError = "system.internalError";
        public const string CodeInvalidParams = "system.invalidParams";
        public const string CodeMethodNotFound = "system.methodNotFound";
        public const string CodeNotFound = "system.notFound";
        public const string CodeTimeout = "system.timeout";

        [JsonProperty(PropertyName = "code")]
        public string Code;

        [JsonProperty(PropertyName = "message")]
        public string Message;

        [JsonProperty(PropertyName = "data", NullValueHandling = NullValueHandling.Ignore)]
        public object Data = null;

        public ResError(Exception ex)
        {
            Code = ResError.CodeInternalError;
            Message = "Internal error: " + ex.Message;
        }

        public ResError(ResException ex)
        {
            Code = ex.Code;
            Message = ex.Message;
            Data = ex.ErrorData;
        }

        public ResError(string code, string message)
        {
            Code = code;
            Message = message;
        }

        public ResError(string code, string message, object data)
        {
            Code = code;
            Message = message;
            Data = data;
        }
    }
}