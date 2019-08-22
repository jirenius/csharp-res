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
    public class Request: ResourceContext, IAccessRequest, IGetRequest, ICallRequest, IAuthRequest, IModelRequest, ICollectionRequest, INewRequest
    {
        private readonly Msg msg;

        private bool replied = false;

        private ILogger Log { get { return Service.Log; } }

        public RequestType Type { get; }

        /// <summary>
        /// Resource method.
        /// This property is not set for RequestType.Access and RequestType.Get.
        /// </summary>
        public string Method { get; }

        /// <summary>
        /// Connection ID of the requesting client connection.
        /// </summary>
        /// <remarks>
        /// This property is not set for RequestType.Get.
        /// </remarks>
        public string CID { get; }

        /// <summary>
        /// Gets the method parameters, or null if the request had no parameters.
        /// </summary>
        /// <remarks>
        /// This property is not set for RequestType.Access and RequestType.Get.
        /// </remarks>
        public JToken Params { get; }

        /// <summary>
        /// Gets the access token, or null if the request had no token.
        /// </summary>
        /// <remarks>
        /// This property is not set for RequestType.Get.
        /// </remarks>
        public JToken Token { get; }

        /// <summary>
        /// HTTP headers sent by client on connect.
        /// This property is only set for RequestType.Auth.
        /// </summary>
        public Dictionary<string, string[]> Header { get; }

        /// <summary>
        /// The host on which the URL is sought by the client. Per RFC 2616,
        /// this is either the value of the "Host" header or the host name given
        /// in the URL itself.
        /// This property is only set for RequestType.Auth.
        /// </summary>
        public string Host { get; }

        /// <summary>
        /// The network address of the client sent on connect.
        /// The format is not specified.
        /// This property is only set for RequestType.Auth.
        /// </summary>
        public string RemoteAddr { get; }

        /// <summary>
        /// The unmodified Request-URI of the Request-Line (RFC 2616, Section 5.1)
        /// as sent by the client when on connect.
        /// This property is only set for RequestType.Auth.
        /// </summary>
        public string URI { get; }

        public Request(
            ResService service,
            Msg msg)
            : base(service)
        {
            this.msg = msg;
        }

        public Request(
            ResService service,
            Msg msg,
            string rtype,
            string rname,
            string method,
            IResourceHandler handler,
            Dictionary<string, string> pathParams,
            string cid,
            JToken rawParams,
            JToken rawToken,
            Dictionary<string, string[]> header,
            string host,
            string remoteAddr,
            string uri,
            string query)
            : base(service, rname, handler, pathParams, query)
        {
            this.msg = msg;
            Type = RequestTypeHelper.FromString(rtype);
            Method = method;
            CID = cid;
            Params = rawParams == null || rawParams.Type == JTokenType.Null ? null : rawParams;
            Token = rawToken == null || rawToken.Type == JTokenType.Null ? null : rawToken;
            Header = header;
            Host = host;
            RemoteAddr = remoteAddr;
            URI = uri;
        }

        /// <summary>
        /// Sends a raw RES protocol response to a request.
        /// Throws an exception if a response has already been sent.
        /// </summary>
        /// <remarks>
        /// Only use this method if you are familiar with the RES protocol,
        /// and you know what you are doing.
        /// </remarks>
        /// <param name="data">JSON encoded RES response data</param>
        public void RawResponse(byte[] data)
        {
            if (replied)
            {
                throw new InvalidOperationException("Response already sent on request");
            }
            replied = true;
            Log.Trace(String.Format("<== {0}: {1}", msg.Subject, Encoding.UTF8.GetString(data)));
            Service.RawSend(msg.Reply, data);
        }


        /// <summary>
        /// Sends a successful empty response to a request.
        /// </summary>
        public void Ok()
        {
            RawResponse(ResService.ResponseSuccess);
        }

        /// <summary>
        /// Sends a successful response to a request.
        /// </summary>
        /// <param name="result">Result object. May be null.</param>
        public void Ok(object result)
        {
            if (result == null)
            {
                RawResponse(ResService.ResponseSuccess);
            }
            else
            {
                try
                {
                    byte[] data = JsonUtils.Serialize(new SuccessDto(result));
                    RawResponse(data);
                }
                catch (Exception ex)
                {
                    Log.Error(String.Format("Error serializing success response: {0}", ex.Message));
                    Error(new ResError(ex));
                }
            }
        }

        /// <summary>
        /// Sends a successful response to a new call request.
        /// </summary>
        /// <remarks>Only valid for new call requests.</remarks>
        /// <param name="resourceID">Valid resource ID to the newly created resource.</param>
        public void New(Ref resourceID)
        {
            if (!resourceID.IsValid())
            {
                throw new ArgumentException("Invalid Resource ID: " + resourceID);
            }
            Ok(resourceID);
        }

        /// <summary>
        /// Sends an error response to a request.
        /// </summary>
        public void Error(ResError error)
        {
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new ErrorDto(error)));
                RawResponse(data);
            }
            catch(Exception ex)
            {
                Log.Error(String.Format("Error serializing error response: {0}", ex.Message));
                RawResponse(ResService.ResponseInternalError);
            }
        }

        /// <summary>
        /// Sends a system.notFound response.
        /// </summary>
        public void NotFound()
        {
            RawResponse(ResService.ResponseNotFound);
        }

        /// <summary>
        /// Sends a system.methodNotFound response.
        /// </summary>
        /// <remarks>
        /// Only valid for RequestType.Call and RequestType.Auth.
        /// </remarks>
        public void MethodNotFound()
        {
            RawResponse(ResService.ResponseMethodNotFound);
        }

        /// <summary>
        /// Sends a system.invalidParams response with a default error message.
        /// </summary>
        /// <remarks>
        /// Only valid for RequestType.Call and RequestType.Auth.
        /// </remarks>
        public void InvalidParams()
        {
            RawResponse(ResService.ResponseInvalidParams);
        }
        
        /// <summary>
        /// Sends a system.invalidParams response with a custom error message.
        /// </summary>
        /// <remarks>
        /// Only valid for RequestType.Call and RequestType.Auth.
        /// </remarks>
        /// <param name="message">Error message.</param>
        public void InvalidParams(string message)
        {
            Error(new ResError(ResError.CodeInvalidParams, message));
        }

        /// <summary>
        /// Sends a system.invalidParams response with a custom error message and data.
        /// </summary>
        /// <remarks>
        /// Only valid for RequestType.Call and RequestType.Auth.
        /// </remarks>
        /// <param name="message">Error message.</param>
        /// <param name="data">Additional data. Must be JSON serializable.</param>
        public void InvalidParams(string message, object data)
        {
            Error(new ResError(ResError.CodeInvalidParams, message, data));
        }

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
        public void Access(bool get, string call)
        {
            if (get)
            {
                if (String.IsNullOrEmpty(call))
                {
                    RawResponse(ResService.ResponseAccessGetOnly);
                }
                else if (call.Length == 1 && call[0] == '*')
                {
                    RawResponse(ResService.ResponseAccessGranted);
                }
                else
                {
                    Ok(new AccessDto(get, call));
                }
            }
            else
            {
                if (String.IsNullOrEmpty(call))
                {
                    RawResponse(ResService.ResponseAccessDenied);
                }
                else
                {
                    Ok(new AccessDto(get, call));
                }
            }
        }
                    

        /// <summary>
        /// Sends a system.accessDenied response.
        /// </summary>
        /// <remarks>Only valid for RequestType.Access requests.</remarks>
        public void AccessDenied()
        {
            RawResponse(ResService.ResponseAccessDenied);
        }

        /// <summary>
        /// Sends a successful response granting full access to the resource.
        /// Same as calling Access(true, "*");
        /// </summary>
        /// <remarks>Only valid for RequestType.Access requests.</remarks>
        public void AccessGranted()
        {
            RawResponse(ResService.ResponseAccessGranted);
        }

        /// <summary>
        /// Sends a successful model response for the get request.
        /// The model must be serializable into a JSON object.
        /// </summary>
        /// <remarks>Only valid for RequestType.Get requests for a model resource.</remarks>
        /// <param name="model">Model data</param>
        public void Model(object model)
        {
            Model(model, null);
        }

        // <summary>
        /// Sends a successful query model response for the get request.
        /// The model must be serializable into a JSON object.
        /// </summary>
        /// <remarks>Only valid for RequestType.Get requests for a query model resource.</remarks>
        /// <param name="model">Model data</param>
        /// <param name="query">Normalized query</param>
        public void Model(object model, string query)
        {
            if (!String.IsNullOrEmpty(query) && String.IsNullOrEmpty(Query))
            {
                throw new ArgumentException("Query model response on non-query request");
            }

            Ok(new ModelDto(model, query));
        }

        /// <summary>
        /// Sends a successful collection response for the get request.
        /// The collection must be serializable into a JSON array.
        /// </summary>
        /// <remarks>Only valid for RequestType.Get requests for a collection resource.</remarks>
        /// <param name="collection">Collection data</param>
        public void Collection(object collection)
        {
            Collection(collection, null);
        }

        // <summary>
        /// Sends a successful query collection response for the get request.
        /// The collection must be serializable into a JSON array.
        /// </summary>
        /// <remarks>Only valid for RequestType.Get requests for a query collection resource.</remarks>
        /// <param name="collection">Collection data</param>
        /// <param name="query">Normalized query</param>
        public void Collection(object collection, string query)
        {
            if (!String.IsNullOrEmpty(query) && String.IsNullOrEmpty(Query))
            {
                throw new ArgumentException("Query collection response on non-query request");
            }

            Ok(new CollectionDto(collection, query));
        }

        internal void ExecuteHandler()
        {
            try
            {
                switch (Type)
                {
                    case RequestType.Access:
                        if (Handler.EnabledHandlers.HasFlag(HandlerTypes.Access))
                        {
                            Handler.Access(this);
                        }
                        // No additional handling. Assume the access request is handled by another service.
                        break;

                    case RequestType.Get:
                        if (Handler.EnabledHandlers.HasFlag(HandlerTypes.Get))
                        {
                            Handler.Get(this);
                        }
                        else
                        {
                            NotFound();
                        }
                        break;

                    case RequestType.Call:
                        if (Method == "new")
                        {
                            if (Handler.EnabledHandlers.HasFlag(HandlerTypes.New))
                            {
                                Handler.New(this);
                            }
                            else
                            {
                                MethodNotFound();
                            }
                        }
                        else if (Handler.EnabledHandlers.HasFlag(HandlerTypes.Call))
                        {
                            Handler.Call(this);
                        }
                        else
                        {
                            MethodNotFound();
                        }
                        break;

                    case RequestType.Auth:
                        if (Handler.EnabledHandlers.HasFlag(HandlerTypes.Auth))
                        {
                            Handler.Auth(this);
                        }
                        else
                        {
                            MethodNotFound();
                        }
                        break;

                    default:
                        Log.Error(String.Format("Unknown request type: {0}", msg.Subject));
                        return;
                }
            }
            catch(ResException ex)
            {
                if (!replied)
                {
                    // If a reply isn't sent yet, send an error response
                    // and return, as throwing a ResException within a handler
                    // is considered valid behaviour.
                    Error(new ResError(ex));
                    return;
                }

                Log.Error(String.Format("Error handling request {0}: {1} - {2}", msg.Subject, ex.Code, ex.Message));
            }
            catch(Exception ex)
            {
                if (!replied)
                {
                    Error(new ResError(ex));
                }

                // Write to log as only ResExceptions are considered valid behaviour.
                Log.Error(String.Format("Error handling request {0}: {1}", msg.Subject, ex.Message));
            }
        }

        /// <summary>
        /// Deserializes the parameters into an object of type T.
        /// </summary>
        /// <remarks>Only valid for RequestType.Call and RequestType.Auth requests.</remarks>
        /// <typeparam name="T">Type to parse the parameters into.</typeparam>
        /// <returns>An object with the parameters, or default value on null parameters.</returns>
        public T ParseParams<T>()
        {
            if (Params == null)
            {
                return default(T);
            }

            try
            {
                return Params.ToObject<T>();
            }
            catch (Exception ex)
            {
                throw new ResException(ResError.InvalidParams, ex);
            }
        }

        /// <summary>
        /// Deserializes the token into an object of type T.
        /// </summary>
        /// <remarks>Not valid for RequestType.Get requests.</remarks>
        /// <typeparam name="T">Type to parse the token into.</typeparam>
        /// <returns>An object with the parsed token, or default value on a null token.</returns>
        public T ParseToken<T>()
        {
            if (Token == null)
            {
                return default(T);
            }

            return Token.ToObject<T>();
        }

        /// <summary>
        /// Attempts to set the timeout duration of the request.
        /// The call has no effect if the requester has already timed out the request,
        /// or if a reply has already been sent.
        /// </summary>
        /// <param name="milliseconds">Timeout duration in milliseconds.</param>
        public void Timeout(int milliseconds)
        {
            if (milliseconds < 0)
            {
                throw new InvalidOperationException("Negative timeout duration");
            }
            if (replied)
            {
                return;
            }

            var str = "timeout:\"" + milliseconds.ToString() + "\"";
            Log.Trace(String.Format("<-- {0}: {1}", msg.Subject, str));
            Service.RawSend(msg.Reply, Encoding.UTF8.GetBytes(str));
        }

        /// <summary>
        /// Attempts to set the timeout duration of the request.
        /// The call has no effect if the requester has already timed out the request,
        /// or if a reply has already been sent.
        /// </summary>
        /// <param name="milliseconds">Timeout duration.</param>
        public void Timeout(TimeSpan duration)
        {
            Timeout((int)duration.TotalMilliseconds);
        }

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
        public void TokenEvent(object token)
        {
            Service.Send("conn." + CID + ".token", new TokenEventDto(token));
        }

        /// <summary>
        /// Flag telling if the request handler is called as a result of Value
        /// or RequireValue being called from another handler.
        /// </summary>
        public bool ForValue { get { return false; } }
    }
}
