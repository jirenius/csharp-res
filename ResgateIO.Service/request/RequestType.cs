namespace ResgateIO.Service
{
    public enum RequestType
    {
        Unknown,
        Access,
        Get,
        Call,
        Auth
    }

    public static class RequestTypeExtension
    {
        public static string ToActionString(this RequestType type) {
            switch (type)
            {
                case RequestType.Access:
                    return "access";
                case RequestType.Get:
                    return "get";
                case RequestType.Call:
                    return "call";
                case RequestType.Auth:
                    return "auth";
                default:
                    return type.ToString();
            }
        }
    }

    public static class RequestTypeHelper
    {
        public static RequestType FromString(string rtype)
        {
            switch (rtype)
            {
                case "access":
                    return RequestType.Access;
                case "get":
                    return RequestType.Get;
                case "call":
                    return RequestType.Call;
                case "auth":
                    return RequestType.Auth;
                default:
                    return RequestType.Unknown;
            }
        }
    }
}
