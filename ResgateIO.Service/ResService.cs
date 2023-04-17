using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NATS.Client;
using Newtonsoft.Json;

namespace ResgateIO.Service
{
    public class ResService: Router, IDisposable
    {
        // Public read-only values
        public static readonly TimeSpan DefaultQueryDuration = new TimeSpan(0, 0, 3);

        // Properties
        public IConnection Connection { get; private set; }

        // Constants
        /// <value>Supported RES protocol version.</value>
        public readonly string ProtocolVersion = "1.2.2";

        // Events

        /// <summary>
        /// Event triggered when the service has started after sending the initial system reset event.
        /// </summary>
        public event EventHandler<ServeEventArgs> Serving;

        /// <summary>
        /// Event triggered when the service has disconnected from NATS server.
        /// </summary>
        public event EventHandler<ServeEventArgs> Disconnected;

        /// <summary>
        /// Event triggered when the service has reconnected to NATS server and sent a system reset event.
        /// </summary>
        public event EventHandler<ServeEventArgs> Reconnected;

        /// <summary>
        /// Event triggered on errors within the service, or incoming messages not complying with the RES protocol.
        /// </summary>
        /// <remarks>
        /// The same error message will also be logged as Error.
        /// </remarks>
        public event EventHandler<ErrorEventArgs> Error;

        /// <summary>
        /// Event triggered when the service has stopped.
        /// </summary>
        public event EventHandler<ServeEventArgs> Stopped;

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
        private CountdownEvent activeWorkers = null;
        private Stack<Action> cleanupActions = new Stack<Action>();
        private JsonSerializerSettings serializerSettings = null;

        // Internal logger
        internal ILogger Log { get; private set; }

        // Constants and readonly
        private const int shutdownTimeout = 5000; // milliseconds
        private static readonly Task completedTask = Task.FromResult(false);
        internal static readonly byte[] EmptyData = new byte[] { };
        internal static readonly ServeEventArgs EmptyServeEventArgs = new ServeEventArgs();


        // Predefined raw data responses
        internal static readonly byte[] ResponseAccessDenied = Encoding.UTF8.GetBytes("{\"result\":{\"get\":false}}");
        internal static readonly byte[] ResponseInternalError = Encoding.UTF8.GetBytes("{\"error\":{\"code\":\"system.internalError\",\"message\":\"Internal error\"}}");
        internal static readonly byte[] ResponseNotFound = Encoding.UTF8.GetBytes("{\"error\":{\"code\":\"system.notFound\",\"message\":\"Not found\"}}");
        internal static readonly byte[] ResponseMethodNotFound = Encoding.UTF8.GetBytes("{\"error\":{\"code\":\"system.methodNotFound\",\"message\":\"Method not found\"}}");
        internal static readonly byte[] ResponseInvalidParams = Encoding.UTF8.GetBytes("{\"error\":{\"code\":\"system.invalidParams\",\"message\":\"Invalid parameters\"}}");
        internal static readonly byte[] ResponseInvalidQuery = Encoding.UTF8.GetBytes("{\"error\":{\"code\":\"system.invalidQuery\",\"message\":\"Invalid query\"}}");
        internal static readonly byte[] ResponseMissingResponse = Encoding.UTF8.GetBytes("{\"error\":{\"code\":\"system.internalError\",\"message\":\"Internal error: missing response\"}}");
        internal static readonly byte[] ResponseBadRequest = Encoding.UTF8.GetBytes("{\"error\":{\"code\":\"system.internalError\",\"message\":\"Internal error: bad request\"}}");
        internal static readonly byte[] ResponseMissingQuery = Encoding.UTF8.GetBytes("{\"error\":{\"code\":\"system.internalError\",\"message\":\"Internal error: missing query\"}}");
        internal static readonly byte[] ResponseAccessGranted = Encoding.UTF8.GetBytes("{\"result\":{\"get\":true,\"call\":\"*\"}}");
        internal static readonly byte[] ResponseAccessGetOnly = Encoding.UTF8.GetBytes("{\"result\":{\"get\":true}}");
        internal static readonly byte[] ResponseSuccess = Encoding.UTF8.GetBytes("{\"result\":null}");
        internal static readonly byte[] ResponseNoQueryEvents = Encoding.UTF8.GetBytes("{\"result\":{\"events\":[]}}");

        internal ErrorHandlerDelegate ErrorHandler = null;


        /// <summary>
        /// Initializes a new instance of the ResService class without a resource name prefix.
        /// </summary>
        public ResService() : base()
        {
            Log = new ConsoleLogger();
            Register(this);
        }

        /// <summary>
        /// Initializes a new instance of the ResService class with a service resource name prefix.
        /// </summary>
        /// <param name="name">Name of the service. The name must be a non-empty alphanumeric string with no embedded whitespace.</param>
        public ResService(string name) : base(name)
        {
            Log = new ConsoleLogger();
            Register(this);
        }

        /// <summary>
        /// Sets the duration in milliseconds for which the service will listen for query requests sent on a query event.
        /// Default is 3000 milliseconds.
        /// </summary>
        /// <remarks>Service must be stopped when calling the method.</remarks>
        /// <param name="duration">Query event duration in milliseconds.</param>
        /// <returns>The ResService instance.</returns>
        public ResService SetQueryDuration(int duration)
        {
            return SetQueryDuration(TimeSpan.FromMilliseconds((double)duration));
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
            Log = logger ?? new VoidLogger();
            return this;
        }

        /// <summary>
        /// Sets the calling delegate which is invoked when an unhandled exception occurs during execution of a request.
        /// </summary>
        /// <param name="globalErrorHandler">The method invoked upon unhandled exception when processing request.</param>
        /// <returns>The ResService instance.</returns>
        public ResService AddGlobalRequestErrorHandler(ErrorHandlerDelegate globalErrorHandler)
        {
            assertStopped();
            this.ErrorHandler = globalErrorHandler;
            return this;
        }

        /// <summary>
        /// Sets the resource patterns matching the resources owned and handled by the service.
        /// If set to null, the service will default to set ownership of all resources
        /// starting with its own name if one was provided (eg. "serviceName.>") to the
        /// constructor, or to all resources if no name was provided.
        /// It will take resource ownership if it has at least one handler of
        /// HandlerTypes Get, Call, Auth, or New.
        /// It will take access ownership if it has at least one handler of HandlerTypes.Access.
        /// </summary>
        /// <remarks>
        ///  For more details on system reset, see:
        ///      https://resgate.io/docs/specification/res-service-protocol/#system-reset-event
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
        /// Sets the settings used with JSON serialization.
        /// </summary>
        /// <param name="settings">JSON serializer settings.</param>
        /// <returns>The ResService instance.</returns>
        public ResService SetSerializerSettings(JsonSerializerSettings settings)
        {
            serializerSettings = settings;
            return this;
        }

        /// <summary>
        /// Subscribes to incoming requests on the IConnection, serving them on
        /// a single thread in the order they are received. For each request,
        /// it calls the appropriate handler method.
        /// </summary>
        /// <param name="conn">Connection to NATS Server</param>
        public void Serve(IConnection conn)
        {
            lock (stateLock)
            {
                assertStopped();
                state = State.Starting;
            }
            cleanupActions.Push(() => state = State.Stopped);

            Connection = conn;
            cleanupActions.Push(() => Connection = null);

            serve();
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
        public void Serve(string url)
        {
            lock (stateLock)
            {
                assertStopped();
                state = State.Starting;
            }
            cleanupActions.Push(() => state = State.Stopped);

            try
            {
                Options opts = ConnectionFactory.GetDefaultOptions();
                opts.Url = url;
                opts.AllowReconnect = true;
                opts.MaxReconnect = Options.ReconnectForever;
                if (Pattern != "")
                {
                    opts.Name = Pattern;
                }

                Log.Info("Connecting to NATS server at {0}", url);
                Connection = new ConnectionFactory().CreateConnection(opts);
                cleanupActions.Push(() =>
                {
                    Log.Debug("Disposing NATS connection");
                    Connection.Dispose();
                    Connection = null;
                });
            }
            catch (Exception ex)
            {
                OnError("Failed to connect to NATS ({0}): {1}", url, ex.Message);
                cleanup();
                throw ex;
            }

            serve();
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

            cleanup();

            Log.Info("Stopped");
            Stopped?.Invoke(this, EmptyServeEventArgs);
        }

        /// <summary>
        /// Matches the resource ID, rid, with the registered resource handlers,
        /// and returns the matching IResourceContext, or null if no matching resource
        /// handler was found.
        /// </summary>
        /// <remarks>
        /// Should only be called from within the resource's group callback.
        /// Using the returned value from another thread may cause race conditions.
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
                : new ResourceContext(this, rname, match.Handler, match.EventHandler, match.Params, query, match.Group);
        }

        /// <summary>
        /// Matches the resource ID, rid, with the registered resource handlers,
        /// before calling the callback on the resource's worker thread.
        /// It will throw an ArgumentException if there is no handler matching
        /// the resource ID, rid.
        /// </summary>
        /// <param name="rid">Resource ID.</param>
        /// <param name="callback">Callback to be called on the resource's worker thread.</param>
        public void With(string rid, Func<IResourceContext, Task> callback)
        {
            IResourceContext resource = Resource(rid);
            if (resource == null)
            {
                throw new ArgumentException(String.Format("No matching handler found for resource ID: {0}", rid));
            }

            runWith(resource.Group, () => callback(resource));
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
            With(rid, r =>
            {
                callback(r);
                return completedTask;
            });
        }

        /// <summary>
        /// Calls the callback on the resource's worker thread.
        /// </summary>
        /// <param name="resource">Resource context.</param>
        /// <param name="callback">Callback to be called on the resource's worker thread.</param>
        public void With(IResourceContext resource, Func<Task> callback)
        {
            runWith(resource.Group, callback);
        }

        /// <summary>
        /// Calls the callback on the resource's worker thread.
        /// </summary>
        /// <param name="resource">Resource context.</param>
        /// <param name="callback">Callback to be called on the resource's worker thread.</param>
        public void With(IResourceContext resource, Action callback)
        {
            runWith(resource.Group, () =>
            {
                callback();
                return completedTask;
            });
        }

        /// <summary>
        /// Calls the callback on the resource's worker thread.
        /// </summary>
        /// <param name="resource">Resource context.</param>
        /// <param name="callback">Callback to be called on the resource's worker thread.</param>
        public void With(IResourceContext resource, Func<IResourceContext, Task> callback)
        {
            runWith(resource.Group, () => callback(resource));
        }

        /// <summary>
        /// Calls the callback on the resource's worker thread.
        /// </summary>
        /// <param name="resource">Resource context.</param>
        /// <param name="callback">Callback to be called on the resource's worker thread.</param>
        public void With(IResourceContext resource, Action<IResourceContext> callback)
        {
            runWith(resource.Group, () =>
            {
                callback(resource);
                return completedTask;
            });
        }

        /// <summary>
        /// Calls the callback on the specified group's worker thread.
        /// </summary>
        /// <param name="group">Group ID.</param>
        /// <param name="callback">Callback to be called on the group's worker thread.</param>
        public void WithGroup(string group, Func<Task> callback)
        {
            runWith(group, callback);
        }

        /// <summary>
        /// Calls the callback on the specified group's worker thread.
        /// </summary>
        /// <param name="group">Group ID.</param>
        /// <param name="callback">Callback to be called on the group's worker thread.</param>
        public void WithGroup(string group, Action callback)
        {
            runWith(group, () =>
            {
                callback();
                return completedTask;
            });
        }

        /// <summary>
        /// Calls the callback on the specified group's worker thread.
        /// </summary>
        /// <param name="group">Group ID.</param>
        /// <param name="callback">Callback to be called on the group's worker thread.</param>
        public void WithGroup(string group, Func<ResService, Task> callback)
        {
            runWith(group, () => callback(this));
        }

        /// <summary>
        /// Calls the callback on the specified group's worker thread.
        /// </summary>
        /// <param name="group">Group ID.</param>
        /// <param name="callback">Callback to be called on the group's worker thread.</param>
        public void WithGroup(string group, Action<ResService> callback)
        {
            runWith(group, () =>
            {
                callback(this);
                return completedTask;
            });
        }

        private void serve()
        {
            Log.Info("Starting service");

            Log.Debug("Registering NATS event handlers");
            Connection.Opts.ReconnectedEventHandler += onReconnect;
            Connection.Opts.DisconnectedEventHandler += onDisconnect;
            Connection.Opts.ClosedEventHandler += onClosed;
            cleanupActions.Push(() =>
            {
                Log.Debug("Unregistering NATS event handlers");
                Connection.Opts.ReconnectedEventHandler -= onReconnect;
                Connection.Opts.DisconnectedEventHandler -= onDisconnect;
                Connection.Opts.ClosedEventHandler -= onClosed;
            });

            try
            {
                ValidateEventListeners();
            }
            catch (Exception ex)
            {
                OnError("Failed event listener validation: {0}", ex.Message);
                cleanup();
                throw ex;
            }

            rwork = new Dictionary<string, Work>();
            queryTimerQueue = new TimerQueue<QueryEvent>(onQueryEventExpire, queryDuration);
            activeWorkers = new CountdownEvent(1);
            cleanupActions.Push(() =>
            {
                Log.Debug("Waiting for {0} task worker(s)", activeWorkers.CurrentCount - 1);
                activeWorkers.Signal();
                if (!activeWorkers.Wait(shutdownTimeout))
                    OnError("Timed out waiting for {0} task worker(s) to finish", activeWorkers.CurrentCount);

                activeWorkers.Dispose();
                queryTimerQueue.Dispose();
                rwork = null;
            });

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
                Serving?.Invoke(this, EmptyServeEventArgs);
            }
            catch (Exception ex)
            {
                OnError("Failed to subscribe: {0}", ex.Message);
                cleanup();
                throw ex;
            }
        }

        private void subscribe()
        {
            setDefaultResourceOwnership();

            foreach (string type in new string[] { "get", "call", "auth" })
            {
                foreach (string p in resetResources)
                {
                    var subject = type + "." + p;
                    if (type != "get" && !subject.EndsWith(">"))
                    {
                        subject += ".*";
                    }
                    Log.Debug("Subscribing to {0}", subject);
                    var sub = Connection.SubscribeAsync(subject, handleMessage);
                    cleanupActions.Push(() =>
                    {
                        Log.Debug("Unsubscribing to {0}", sub.Subject);
                        sub.Unsubscribe();
                    });
                }
            }
            foreach (string p in resetAccess)
            {
                var subject = "access." + p;
                Log.Debug("Subscribing to {0}", subject);
                var sub = Connection.SubscribeAsync(subject, handleMessage);
                cleanupActions.Push(() =>
                {
                    Log.Debug("Unsubscribing to {0}", sub.Subject);
                    sub.Unsubscribe();
                });
            }
        }

        private void handleMessage(object sender, MsgHandlerEventArgs e)
        {
            Msg msg = e.Message;
            String subj = msg.Subject;

            Log.Trace("==> {0}: {1}", subj, msg.Data == null ? "<null>" : Encoding.UTF8.GetString(msg.Data));

            // Assert there is a reply subject
            if (String.IsNullOrEmpty(msg.Reply))
            {
                OnError("Missing reply subject on request: {0}", subj);
                return;
            }

            // Get request type
            Int32 idx = subj.IndexOf('.');
            if (idx < 0)
            {
                // Shouldn't be possible unless NATS is really acting up
                OnError("Invalid request subject: {0}", subj);
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
                    OnError("Invalid request subject: {0}", subj);
                    return;
                }
                method = rname.Substring(lastIdx + 1);
                rname = rname.Substring(0, lastIdx);
            }

            Router.Match match = GetHandler(rname);

            runWith(match == null ? rname : match.Group, () => processRequest(msg, rtype, rname, method, match));
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
            TokenEvent(cid, token, null);
        }

        /// <summary>
        /// Sends a connection token event, including a token ID, that sets the connection's access token,
        /// discarding any previously set token.
        /// A change of token will invalidate any previous access response received using the old token.
        /// The token ID is an identifier of the token, used when calling TokenReset to update or clear a token.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://github.com/resgateio/resgate/blob/master/docs/res-service-protocol.md#connection-token-event
        /// </remarks>
        /// <param name="cid">Connection ID</param>
        /// <param name="token">Access token. A null token clears any previously set token.</param>
        /// <param name="tid">Token ID, used to identify a token when calling TokenReset.</param>
        public void TokenEvent(string cid, object token, string tid)
        {
            if (!IsValidPart(cid))
            {
                throw new ArgumentException(String.Format("Invalid connection ID: {0}", cid));
            }
            Send("conn." + cid + ".token", new TokenEventDto(token, tid));
        }

        /// <summary>
        /// TokenReset sends a token reset event for the provided token IDs.
        /// 
        /// The subject string is a message subject that will receive auth requests for
        /// any connections with a token matching any of the token IDs.
        /// </summary>
        /// <remarks>
        /// See the protocol specification for more information:
        ///    https://resgate.io/docs/specification/res-service-protocol/#system-token-reset-event
        /// </remarks>
        /// <param name="subject">Message subject for auth requests</param>
        /// <param name="tokenIds">Token IDs for tokens to reset.</param>
        public void TokenReset(string subject, params string[] tokenIds)
        {
            if (String.IsNullOrEmpty(subject))
            {
                throw new ArgumentException(String.Format("Invalid message subject: {0}", subject));
            }
            if (tokenIds == null || tokenIds.Length == 0)
            {
                return;
            }
            Send("system.tokenReset", new SystemTokenResetDto(subject, tokenIds));
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
        /// Serializes an object into UTF8 encoded JSON.
        /// </summary>
        /// <param name="payload">Payload object to serialize.</param>
        /// <returns>UTF8 encoded JSON.</returns>
        internal byte[] JsonSerialize(object payload)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload, serializerSettings));
        }

        /// <summary>
        /// Sends a raw data message to NATS server on a given subject,
        /// logging any exception.
        /// </summary>
        /// <remarks>This is a low level method, only to be used if you are familiar with the RES protocol.</remarks>
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
                OnError("Error sending message {0}: {1}", subject, ex.Message);
            }
        }

        /// <summary>
        /// Sends a JSON encoded message to NATS server on a given subject,
        /// trace logging the message, and logging any exception.
        /// </summary>
        /// <remarks>This is a low level method, only to be used if you are familiar with the RES protocol.</remarks>
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
                    Log.Trace("<-- {0}: {1}", subject, json);
                    RawSend(subject, data);
                }
                else
                {
                    Log.Trace("<-- {0}", subject);
                    RawSend(subject, EmptyData);
                }
            }
            catch (Exception ex)
            {
                OnError("Error serializing event payload for {0}: {1}", subject, ex.Message);
            }
        }

        /// <summary>
        /// Adds a query
        /// </summary>
        /// <param name="queryEvent"></param>
        internal void AddQueryEvent(QueryEvent queryEvent)
        {
            if (queryEvent.Start())
            {
                queryTimerQueue.Add(queryEvent);
            }
        }

        internal static bool IsValidPart(string part)
        {
            if (String.IsNullOrEmpty(part))
            {
                return false;
            }
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

        internal void OnError(string format, params object[] args)
        {
            Log.Error(format, args);
            if (Error != null)
            {
                Error.Invoke(this, new ErrorEventArgs(String.Format(format, args)));
            }
        }

        private void onQueryEventExpire(QueryEvent queryEvent)
        {
            queryEvent.Stop();
        }

        private void runWith(string groupId, Func<Task> callback)
        {
            Work work;
            lock (stateLock)
            {
                if (rwork.TryGetValue(groupId, out work))
                {
                    work.AddTask(callback);
                    return;
                }

                work = new Work(groupId, callback);
                rwork.Add(groupId, work);
                activeWorkers.AddCount();
            }

            Task.Run(async () =>
            {
                try
                {
                    await processWork(work);
                }
                finally
                {
                    activeWorkers.Signal();
                }
            });
        }

        private async Task processWork(Work work)
        {
            Func<Task> task;

            while (true)
            {
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

                try
                {
                    await task();
                }
                catch (Exception ex)
                {
                    OnError("Exception encountered running resource task:\n{0}", ex.ToString());
                }
            }
        }

        private Task processRequest(Msg msg, String rtype, String rname, String method, Router.Match match)
        {
            Request req;

            // Check if there is no matching handler
            if (match == null)
            {
                // [TODO] Allow for a default handler
                req = new Request(this, msg);
                req.NotFound();
                return completedTask;
            }

            try
            {
                byte[] d = msg.Data;
                RequestDto reqInput = d == null || d.Length == 0 || (d.Length == 2 && d[0] == '{' && d[1] == '}')
                    ? RequestDto.Empty
                    : JsonConvert.DeserializeObject<RequestDto>(Encoding.UTF8.GetString(msg.Data));

                req = new Request(
                    this,
                    msg,
                    rtype,
                    rname,
                    method,
                    match.Handler,
                    match.EventHandler,
                    match.Params,
                    match.Group,
                    reqInput.CID,
                    reqInput.RawParams,
                    reqInput.RawToken,
                    reqInput.Header,
                    reqInput.Host,
                    reqInput.RemoteAddr,
                    reqInput.URI,
                    reqInput.Query);
            }
            catch (Exception ex)
            {
                OnError("Error deserializing incoming request: {0}", msg.Data == null ? "<null>" : Encoding.UTF8.GetString(msg.Data));
                req = new Request(this, msg);
                req.Error(new ResError(ex));
                return completedTask;
            }


            return req.ExecuteHandler();
        }

        private void onReconnect(object sender, ConnEventArgs args)
        {
            Log.Info("Reconnected to NATS. Sending reset event.");
            ResetAll();
            Reconnected?.Invoke(this, EmptyServeEventArgs);
        }

        private void onDisconnect(object sender, ConnEventArgs args)
        {
            Log.Info("Lost connection to NATS.");
            Disconnected?.Invoke(this, EmptyServeEventArgs);
        }

        private void onClosed(object sender, ConnEventArgs args)
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
                resetResources = Contains(h => (h.EnabledHandlers & (HandlerTypes.Get | HandlerTypes.Call | HandlerTypes.Auth | HandlerTypes.New)) != HandlerTypes.None)
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

        private void cleanup()
        {
            while (cleanupActions.Count > 0)
            {
                try
                {
                    cleanupActions.Pop()();
                }
                catch (Exception ex)
                {
                    OnError("Error cleaning up: {0}", ex.Message);
                }
            }
        }

        public void Dispose()
        {
            cleanup();
        }
    }
}
