using System;

namespace ResgateIO.Service
{
    public class ResException: Exception
    {
        public string Code { get; }
        public object ErrorData { get; }

        public ResException()
        {
            Code = ResError.CodeInternalError;
        }

        public ResException(string message)
            : base(message)
        {
            Code = ResError.CodeInternalError;
        }

        public ResException(string message, Exception inner)
            : base(message, inner)
        {
            Code = ResError.CodeInternalError;
        }

        public ResException(string code, string message)
            : base(message)
        {
            Code = code;
        }

        public ResException(string code, string message, Exception inner)
            : base(message, inner)
        {
            Code = code;
        }

        public ResException(string code, string message, object errorData)
            : base(message)
        {
            Code = code;
            ErrorData = errorData;
        }

        public ResException(string code, string message, object errorData, Exception inner)
            : base(message, inner)
        {
            Code = code;
            ErrorData = errorData;
        }

        public ResException(ResError error)
            : base(error.Message)
        {
            Code = error.Code;
            ErrorData = error.Data;
        }

        public ResException(ResError error, Exception inner)
            : base(error.Message, inner)
        {
            Code = error.Code;
            ErrorData = error.Data;
        }
    }
}
