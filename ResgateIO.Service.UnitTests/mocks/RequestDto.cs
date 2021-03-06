﻿using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace ResgateIO.Service.UnitTests
{
#pragma warning disable 0649 // These fields are assigned by JSON deserialization

    public struct RequestDto
    {
        [JsonProperty(PropertyName = "cid", NullValueHandling = NullValueHandling.Ignore)]
        public string CID;

        // JSON encoded method parameters, or nil if the request had no parameters.
        // For access and get requests it is unused.
        [JsonProperty(PropertyName = "params", NullValueHandling = NullValueHandling.Ignore)]
        public object Params;

        // JSON encoded access token, or nil if the request had no token.
        // For get requests it is unused.
        [JsonProperty(PropertyName = "token", NullValueHandling = NullValueHandling.Ignore)]
        public object Token;

        // HTTP headers sent by client on connect.
        // This field is only populated for auth requests.
        [JsonProperty(PropertyName = "header", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string[]> Header;

        // The host on which the URL is sought by the client. Per RFC 2616,
        // this is either the value of the "Host" header or the host name given
        // in the URL itself.
        // This field is only populated for auth requests.
        [JsonProperty(PropertyName = "host", NullValueHandling = NullValueHandling.Ignore)]
        public string Host;

        // The network address of the client sent on connect.
        // The format is not specified.
        // This field is only populated for auth requests.
        [JsonProperty(PropertyName = "remoteAddr", NullValueHandling = NullValueHandling.Ignore)]
        public string RemoteAddr;

        // The unmodified Request-URI of the Request-Line (RFC 2616, Section 5.1)
        // as sent by the client when on connect.
        // This field is only populated for auth requests.
        [JsonProperty(PropertyName = "uri", NullValueHandling = NullValueHandling.Ignore)]
        public string URI;

        // Query part of the resource ID without the question mark separator.
        // May be null.
        [JsonProperty(PropertyName = "query", NullValueHandling = NullValueHandling.Ignore)]
        public string Query;
    }

#pragma warning restore 0649
}
