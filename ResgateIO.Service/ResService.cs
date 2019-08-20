using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using NATS.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ResgateIO.Service
{
    public class ResService: Router
    {
        // Public read-only values
        public static readonly JRaw DeleteAction = new JRaw("{\"action\":\"delete\"}");
        public static readonly TimeSpan DefaultQueryDuration = new TimeSpan(0, 0, 3);

        internal static readonly byte[] EmptyData = new byte[] { };

        // Properties
        public IConnection Connection { get; private set; }

        // Enums
        private enum State { Stopped, Starting, Started, Stopping };

        // Fields
        private State state = State.Stopped;
        private readonly Object stateLock = new Object();
        private Dictionary<string, Work> rwork;
        private TimerQueue<QueryEvent> queryTimerQueue;
        private TimeSpan queryDuration = DefaultQueryDuration;
        private string[] resetResources = null;
        private string[] resetAccess = null;

        // Internal logger
        internal ILogger Log { get; private set; }

        // Predefined raw data responses
        internal static readonly byte[] ResponseAccessDenied = Encoding.UTF8.GetBytes("{\"result\":{\"get\":false}}");
        internal static readonly byte[] ResponseInternalError = Encoding.UTF8.GetBytes("{\"error\":{\"code\":\"system.internalError\",\"message\":\"Internal error\"}}");
        internal static readonly byte[] ResponseNotFound = Encoding.UTF8.GetBytes("{\"error\":{\"code\":\"system.notFound\",\"message\":\"Not found\"}}");
        internal static readonly byte[] ResponseMethodNotFound = Encoding.UTF8.GetBytes("{\"error\":{\"code\":\"system.methodNotFound\",\"message\":\"Method not found\"}}");
        internal static readonly byte[] ResponseInvalidParams = Encoding.UTF8.GetBytes("{\"error\":{\"code\":\"system.invalidParams\",\"message\":\"Invalid parameters\"}}");
        internal static readonly byte[] ResponseMissingResponse = Encoding.UTF8.GetBytes("{\"error\":{\"code\":\"system.internalError\",\"message\":\"Internal error: missing response\"}}");
        internal static readonly byte[] ResponseBadRequest = Encoding.UTF8.GetBytes("{\"error\":{\"code\":\"system.internalError\",\"message\":\"Internal error: bad request\"}}");
        internal static readonly byte[] ResponseMissingQuery = Encoding.UTF8.GetBytes("{\"error\":{\"code\":\"system.internalError\",\"message\":\"Internal error: missing query\"}}");
        internal static readonly byte[] ResponseAccessGranted = Encoding.UTF8.GetBytes("{\"result\":{\"get\":true,\"call\":\"*\"}}");
        internal static readonly byte[] ResponseAccessGetOnly = Encoding.UTF8.GetBytes("{\"result\":{\"get\":true}}");
        internal static readonly byte[] ResponseSuccess = Encoding.UTF8.GetBytes("{\"result\":null}");
        internal static readonly byte[] ResponseNoQueryEvents = Encoding.UTF8.GetBytes("{\"result\":{\"events\":[]}}");


        /// <summary>
        /// Initializes a new instance of the ResService class without a resource name prefix.
        /// </summary>
        public ResService() : base()
        {
            Log = new ConsoleLogger();
        }

        /// <summary>
        /// Initializes a new instance of the ResService class with a service resource name prefix.
        /// </summary>
        /// <param name="name">Name of the service. The name must be a non-empty alphanumeric string with no embedded whitespace.</param>
        public ResService(string name) : base(name)
        {
            Log = new ConsoleLogger();
        }

        /// <summary>
        /// Sets the duration for which the service will listen for query requests sent on a query event.
        /// Default is 3 seconds.
        /// </summary>
        /// <remarks>Service must be stopped when calling the method.</remarks>
        /// <param name="duration">Query event duration.</param>
        /// <returns>The ResService instance.</returns>
        public ResService SetQueryDuration(TimeSpan duration)
        {
            assertStopped();
            queryDuration = duration;
            return this;
        }

        /// <summary>
        /// Sets the logger used by the service.
        /// Defaults to ConsoleLogger set to log all levels to the console.
        /// </summary>
        /// <remarks>Service must be stopped when calling the method.</remarks>
        /// <param name="logger">Logger. If null, logging will be disabled.</param>
        /// <returns>The ResService instance.</returns>
        public ResService SetLogger(ILogger logger)
        {
            assertStopped();
            if (logger == null)
            {
                logger = new VoidLogger();
            }
            else
            {
                Log = logger;
            }
            return this;
        }

        /// <summary>
        /// Sets the resource patterns matching the resources owned by the service.
        /// If set to null, the service will default to set ownership of all resources
        /// starting with its own name if one was provided (eg. "serviceName.>") to the
        /// constructor, or to all resources if no name was provided.
        /// It will take resource ownership if it has at least one handler of
        /// HandlerTypes Get, Call, or Auth.
        /// It will take access ownership if it has at least one handler of HandlerTypes.Access.
        /// </summary>
        /// <remarks>
        //  For more details on system reset, see:
        //      https://resgate.io/docs/specification/res-service-protocol/#system-reset-event
        /// </remarks>
        /// <param name="resources">Resource patterns, or null if using default.</param>
        /// <param name="access">Access patterns, or null if using default.</param>
        /// <returns>The ResService instance.</returns>
        public ResService SetOwnedResources(string[] resources, string[] access)
        {
            resetResources = resources;
            resetAccess = access;
            return this;
        }

        /// <summary>
        /// Subscribes to incoming requests on the IConnection, serving them on
        /// a single thread in the order they are received. For each request,
        /// it calls the appropriate handler method.
        /// </summary>
        /// <param name="conn">Connection to NATS Server</param>
        public void Serve(IConnection conn) {
            lock (stateLock)
            {
                if (state != State.Stopped)
                {
                    throw new Exception("Service is not stopped.");
                }
                state = State.Starting;
            }

            serve(conn);
        }

        /// <summary>
        /// Connects to the NATS server at the url. Once connected,
        /// it subscribes to incoming requests and serves them on a single thread
        /// in the order they are received. For each request, it calls the appropriate
        /// handler method.
        ///
        /// In case of disconnect, it will try to reconnect until Close is called,
        /// or until successfully reconnecting, upon which Reset will be called.
        /// </summary>
        /// <param name="url">URL to NATS Server.</param>
        public void Serve(string url) {
            lock (stateLock)
            {
                assertStopped();
                state = State.Starting;
            }

            Options opts = ConnectionFactory.GetDefaultOptions();
            opts.Url = url;
            opts.AllowReconnect = true;
            opts.MaxReconnect = Options.ReconnectForever;
            opts.ReconnectedEventHandler += handleReconnect;
            opts.DisconnectedEventHandler += handleDisconnect;
            opts.ClosedEventHandler += handleClosed;
            if (Pattern != "")
            {
                opts.Name = Pattern;
            }

            IConnection conn = new ConnectionFactory().CreateConnection(opts);
            serve(conn);
        }

        /// <summary>
        /// Closes any existing connection to NATS Server.
        /// Does nothing if service isn't started.
        /// </summary>
        public void Shutdown()
        {
            lock (stateLock)
            {
                if (state != State.Started)
                {
                    return;
                }
                state = State.Stopping;
            }
            Log.Info("Stopping service...");

            close();
            // TODO: Wait for all the threads to be done

            queryTimerQueue.Dispose();
            Connection.Dispose();

            state = State.Stopped;
            Log.Info("Stopped");
        }

        /// <summary>
        /// Matches the resource ID, rid, with the registered resource handlers,
        /// and returns the matching IResourceContext, or null if no matching resource
        /// handler was found.
        /// </summary>
        /// <remarks>
        /// Should only be called from within the resource's group callback.
        /// Using the returned value from another goroutine may cause race conditions.
        /// </remarks>
        /// <param name="rid">Resource ID.</param>
        /// <returns>Resource context matching the resource ID, or null if no match is found.</returns>
        public IResourceContext Resource(string rid)
        {
            string rname = rid;
            string query = "";
            int idx = rid.IndexOf('?');
            if (idx > -1)
            {
                rname = rid.Substring(0, idx);
                query = rid.Substring(idx + 1);
            }
            Router.Match match = GetHandler(rname);
            return match == null
                ? null
                : new ResourceContext(this, rname, match.Handler, match.Params, query);
        }

        /// <summary>
        /// Matches the resource ID, rid, with the registered resource handlers,
        /// before calling the callback on the resource's worker thread.
        /// It will throw an ArgumentException if there is no handler matching
        /// the resource ID, rid.
        /// </summary>
        /// <param name="rid">Resource ID.</param>
        /// <param name="callback">Callback to be called on the resource's worker thread.</param>
        public void With(string rid, Action<IResourceContext> callback)
        {
            IResourceContext resource = Resource(rid);
            if (resource == null)
            {
                throw new ArgumentException("No matching handler found for resource ID: " + rid);
            }

            runWith(resource.ResourceName, () => callback(resource));
        }

        /// <summary>
        /// Calls the callback on the resource's worker thread.
        /// </summary>
        /// <param name="resource">Resource context.</param>
        /// <param name="callback">Callback to be called on the resource's worker thread.</param>
        public void With(IResourceContext resource, Action callback)
        {
            runWith(resource.ResourceName, callback);
        }

        private void serve(IConnection conn)
        {
            Connection = conn;
            rwork = new Dictionary<string, Work>();
            queryTimerQueue = new TimerQueue<QueryEvent>(onQueryEventExpire, queryDuration);

            lock (stateLock)
            {
                state = State.Started;
            }

            try
            {
                subscribe();
                // Always start with a reset
                ResetAll();
                Log.Info("Listening for requests");
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Failed to subscribe: {0}", ex.Message));
                close();
            }
        }

        private void close()
        {
            Connection.Close();
        }

        private void subscribe()
        {
            setDefaultResourceOwnership();

            foreach (string type in new string[] { "get", "call", "auth" })
            {
                foreach (string p in resetResources)
                {
                    Connection.SubscribeAsync(type + "." + p, handleMessage);
                }
            }
            foreach (string p in resetAccess)
            {
                Connection.SubscribeAsync("access." + p, handleMessage);
            }
        }

        private void handleMessage(object sender, MsgHandlerEventArgs e)
        {
            Msg msg = e.Message;
            String subj = msg.Subject;

            Log.Trace(String.Format("==> {0}: {1}", subj, msg.Data == null ? "<null>" : Encoding.UTF8.GetString(msg.Data)));

            // Assert there is a reply subject
            if (String.IsNullOrEmpty(msg.Reply))
            {
                Log.Error(String.Format("Missing reply subject on request: {0}", subj));
                return;
            }

            // Get request type
            Int32 idx = subj.IndexOf('.');
            if (idx < 0) {
                // Shouldn't be possible unless NATS is really acting up
                Log.Error(String.Format("Invalid request subject: {0}", subj));
                return;
            }
            String method = null;
            String rtype = subj.Substring(0, idx);
            String rname = subj.Substring(idx + 1);

            if (rtype == "call" || rtype == "auth")
            {
                Int32 lastIdx = rname.LastIndexOf('.');
                if (idx < 0)
                {
                    // No method? Resgate must be acting up
                    Log.Error(String.Format("Invalid request subject: {0}", subj));
                    return;
                }
                method = rname.Substring(lastIdx + 1);
                rname = rname.Substring(0, lastIdx);
            }

            runWith(rname, () => processRequest(msg, rtype, rname, method));
        }

private void runWith(string workId, Action callback)
{
    lock (stateLock)
    { 
        if (rwork.TryGetValue(workId, out Work work))
        {
            work.AddTask(callback);
        }
        else
        {
            work = new Work(workId, callback);
            rwork.Add(workId, work);
            ThreadPool.QueueUserWorkItem(new WaitCallback(processWork), work);
        }
    }
}

        /// <summary>
        /// Sends a connection token event that sets the connection's access token,
        /// discarding any previously set token.
        /// A change of token will invalidate any previous access response received using the old token.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#connection-token-event
        /// </remarks>
        /// <param name="cid">Connection ID</param>
        /// <param name="token">Access token. A null token clears any previously set token.</param>
        public void TokenEvent(string cid, object token)
        {
            if (!IsValidPart(cid))
            {
                throw new ArgumentException("Invalid connection ID: " + cid);
            }
            Send("conn." + cid + ".token", new TokenEventDto(token));
        }

        /// <summary>
        /// Reset sends a system reset event.
        /// </summary>
        /// <param name="resources">Resource patterns to reset cached get responses for.</param>
        /// <param name="access">Resource patterns to reset cached access responses for.</param>
        public void Reset(string[] resources, string[] access)
        {
            if ((resources == null || resources.Length == 0) && (access == null || access.Length == 0))
            {
                return;
            }
            Send("system.reset", new SystemResetDto(resources, access));
        }

        /// <summary>
        /// Sends a system.reset to trigger any gateway to update their cache
        /// for all resources handled by the service.
        /// The method is automatically called on server start and reconnects.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://resgate.io/docs/specification/res-service-protocol/#system-reset-event
        /// </remarks>
        public void ResetAll()
        {
            setDefaultResourceOwnership();
            Reset(resetResources, resetAccess);
        }

        /// <summary>
        /// Sends a raw data message to NATS server on a given subject,
        /// logging any exception.
        /// </summary>
        /// <param name="subject">Message subject</param>
        /// <param name="data">Message JSON encoded data</param>
        internal void RawSend(string subject, byte[] data)
        {
            try
            {
                Connection.Publish(subject, data);
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error sending message {0}: {1}", subject, ex.Message));
            }
        }

        /// <summary>
        /// Sends a message to NATS server on a given subject,
        /// logging any exception.
        /// </summary>
        /// <param name="subject">Message subject</param>
        /// <param name="payload">Message payload</param>
        internal void Send(string subject, object payload)
        {
            try
            {
                if (payload != null)
                {
                    string json = JsonConvert.SerializeObject(payload);
                    byte[] data = Encoding.UTF8.GetBytes(json);
                    Log.Trace(String.Format("<-- {0}: {1}", subject, json));
                    RawSend(subject, data);
                }
                else
                {
                    Log.Trace(String.Format("<-- {0}", subject));
                    RawSend(subject, EmptyData);
                }
            }
            catch (Exception ex)
            {
                Log.Error(String.Format("Error serializing event payload for {0}: {1}", subject, ex.Message));
            }
        }

        internal void AddQueryEvent(QueryEvent queryEvent)
        {
            queryEvent.Start();
            queryTimerQueue.Add(queryEvent);
        }

        internal static bool IsValidPart(string part)
        {
            foreach (char c in part)
            {
                if (c == '?')
                {
                    return false;
                }
                if (c < 33 || c > 126 || c == '?' || c == '*' || c == '>' || c == '.')
                {
                    return false;
                }
            }
            return true;
        }

        private void onQueryEventExpire(QueryEvent queryEvent)
        {
            queryEvent.Stop();
        }

        private void processWork(Object obj)
        {
            Work work = (Work)obj;
            Action task;

            while (true) {
                lock (stateLock)
                {
                    task = work.NextTask();
                    // Check if work tasks are exhausted
                    if (task == null)
                    {
                        // Work completed
                        rwork.Remove(work.ResourceName);
                        return;
                    }
                }

                task();
            }
        }
        
        private void processRequest(Msg msg, String rtype, String rname, String method)
        {
            Router.Match match = GetHandler(rname);
            Request req;

            // Check if there is no matching handler
            if (match == null)
            {
                // [TODO] Allow for a default handler
                req = new Request(this, msg);
                req.NotFound();
                return;
            }

            try
            {
                byte[] d = msg.Data;
                RequestDto reqInput = d == null || d.Length == 0 || (d.Length == 2 && d[0] == '{' && d[1] == '}')
                    ? RequestDto.Empty
                    : JsonUtils.Deserialize<RequestDto>(msg.Data);

                req = new Request(
                    this,
                    msg,
                    rtype,
                    rname,
                    method,
                    match.Handler,
                    match.Params,
                    reqInput.CID,
                    reqInput.RawParams,
                    reqInput.RawToken,
                    reqInput.Header,
                    reqInput.Host,
                    reqInput.RemoteAddr,
                    reqInput.URI,
                    reqInput.Query);
            }
            catch(Exception ex)
            {
                Log.Error(String.Format("Error deserializing incoming request: {0}", msg.Data == null ? "<null>" : Encoding.UTF8.GetString(msg.Data)));
                req = new Request(this, msg);
                req.Error(new ResError(ex));
                return;
            }

            req.ExecuteHandler();
        }

        private void handleReconnect(object sender, ConnEventArgs args)
        {
            Log.Info("Reconnected to NATS. Sending reset event.");
            ResetAll();
        }

        private void handleDisconnect(object sender, ConnEventArgs args)
        {
            Log.Info("Lost connection to NATS.");
        }

        private void handleClosed(object sender, ConnEventArgs args)
        {
            Shutdown();
        }

        private void assertStopped()
        {
            if (state != State.Stopped)
            {
                throw new Exception("Service is not stopped.");
            }
        }

        private void setDefaultResourceOwnership()
        {
            if (resetResources == null)
            {
                resetResources = Contains(h => (h.EnabledHandlers & (HandlerTypes.Get | HandlerTypes.Call | HandlerTypes.Auth)) != HandlerTypes.None)
                    ? new string[] { MergePattern(Pattern, ">") }
                    : new string[] { };
            }
            if (resetAccess == null)
            {
                resetAccess = Contains(h => h.EnabledHandlers.HasFlag(HandlerTypes.Access))
                    ? new string[] { MergePattern(Pattern, ">") }
                    : new string[] { };
            }
        }

    }
}
