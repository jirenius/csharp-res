using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace ResgateIO.Service
{
#pragma warning disable 0649 // These fields are assigned by JSON deserialization

    internal class RequestDto
    {
        [JsonProperty(PropertyName = "cid")]
        public string CID;

        // JSON encoded method parameters, or nil if the request had no parameters.
        // For access and get requests it is unused.
        [JsonProperty(PropertyName = "params")]
        public JToken RawParams;

        // JSON encoded access token, or nil if the request had no token.
        // For get requests it is unused.
        [JsonProperty(PropertyName = "token")]
        public JToken RawToken;

        // HTTP headers sent by client on connect.
        // This field is only populated for auth requests.
        [JsonProperty(PropertyName = "header")]
        public Dictionary<string, string[]> Header;

        // The host on which the URL is sought by the client. Per RFC 2616,
        // this is either the value of the "Host" header or the host name given
        // in the URL itself.
        // This field is only populated for auth requests.
        [JsonProperty(PropertyName = "host")]
        public string Host;

        // The network address of the client sent on connect.
        // The format is not specified.
        // This field is only populated for auth requests.
        [JsonProperty(PropertyName = "remoteAddr")]
        public string RemoteAddr;

        // The unmodified Request-URI of the Request-Line (RFC 2616, Section 5.1)
        // as sent by the client when on connect.
        // This field is only populated for auth requests.
        [JsonProperty(PropertyName = "uri")]
        public string URI;

        // Query part of the resource ID without the question mark separator.
        // May be null.
        [JsonProperty(PropertyName = "query")]
        public string Query;
    }

#pragma warning restore 0649
}
