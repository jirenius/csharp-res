using NATS.Client;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace ResgateIO.Service.UnitTests
{
    public class MockConnection : IConnection
    {
        private const int TimeoutDuration = 100; // 100 milliseconds

        public bool Closed { get { return closed; } }

        private readonly Object locker = new Object();
        private HashSet<string> subStrings = new HashSet<string>();
        private HashSet<IAsyncSubscription> subs = new HashSet<IAsyncSubscription>();
        private BlockingCollection<MockMsg> queue = new BlockingCollection<MockMsg>();

        private bool closed = false;
        private bool failNextSubscription = false;
        private Random r = null;

        private Options opts = ConnectionFactory.GetDefaultOptions();

        public int SubscriptionCount {
            get
            {
                lock (locker)
                {
                    return subs.Count;
                }
            }
        }

        // Constructor

        public MockConnection()
        {
        }

        // Required methods

        public void Close()
        {
            lock (locker)
            {
                subs.Clear();
                subStrings.Clear();

                closed = true;
            }
        }

        public void Dispose()
        {
            Close();
        }

        public void Publish(string subject, byte[] data)
        {
            lock (locker)
            {
                queue.Add(new MockMsg(subject, data));
            }
        }

        public string NewInbox()
        {
            if (r == null)
            {
                r = new Random(Guid.NewGuid().GetHashCode());
            }
            byte[] buf = new byte[13];
            r.NextBytes(buf);
            return "_INBOX." + BitConverter.ToString(buf).Replace("-", "");
        }

        /// <summary>
        /// Sends a request from NATS to service with raw data payload.
        /// </summary>
        /// <param name="subject">NATS message subject.</param>
        /// <param name="data">JSON encoded message data.</param>'
        /// <returns>Inbox for the request.</returns>
        public string NATSRequest(string subject, byte[] data)
        {
            var inbox = NewInbox();
            Msg msg = new Msg(subject, inbox, data);

            lock (locker)
            {
                foreach (MockSubscription sub in subs)
                {
                    if (sub.Matches(subject))
                    {
                        var e = new MsgHandlerEventArgs();
                        // Since the message field is internal,
                        // use reflection to set it.
                        var prop = e
                            .GetType()
                            .GetField("msg", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        prop.SetValue(e, msg);

                        sub.Handler((IConnection)this, e);
                    }
                }
            }

            return inbox;
        }

        /// <summary>
        /// Sends a request from NATS to service with data object to use as JSON encoded payload.
        /// </summary>
        /// <param name="subject">NATS message subject.</param>
        /// <param name="data">JSON serializable message data.</param>'
        /// <returns>Inbox for the request.</returns>
        public string NATSRequest(string subject, object data)
        {
            return NATSRequest(subject, data == null ? null : Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(data)));
        }

        /// <summary>
        /// Sends a request from NATS to service without data payload.
        /// </summary>
        /// <param name="subject">NATS message subject.</param>
        /// <returns>Inbox for the request.</returns>
        public string NATSRequest(string subject)
        {
            return NATSRequest(subject, null);
        }

        /// <summary>
        /// Gets a message published from the service to NATS.
        /// Asserts that a message is available within the timeout duration.
        /// </summary>
        /// <returns>First available message.</returns>
        public MockMsg GetMsg()
        {
            Assert.True(queue.TryTake(out MockMsg msg, TimeoutDuration), "no available messages");
            return msg;
        }

        public MockConnection AssertNoMsg()
        {
            if (queue.TryTake(out MockMsg msg, TimeoutDuration))
            {
                Assert.True(false, String.Format("message found: {0}", msg.Subject));
            }
            return this;
        }

        public IAsyncSubscription SubscribeAsync(string subject, EventHandler<MsgHandlerEventArgs> handler)
        {
            lock (locker)
            {
                if (failNextSubscription)
                {
                    failNextSubscription = false;
                    throw new Exception("Failing subscription as requested.");
                }
                if (subStrings.Contains(subject))
                {
                    throw new Exception("Already subscribing to subject: " + subject);
                }

                subStrings.Add(subject);

                MockSubscription sub = new MockSubscription(this, subject, handler);
                subs.Add(sub);

                return sub;
            }
        }

        public void Unsubscribe(MockSubscription sub)
        {
            lock (locker)
            {
                if (!subStrings.Contains(sub.Subject))
                {
                    throw new Exception("No subscription found on subject: " + sub.Subject);
                }
                subStrings.Remove(sub.Subject);
                subs.Remove(sub);
            }
        }

        public void FailNextSubscription()
        {
            lock (locker)
            {
                failNextSubscription = true;
            }
        }

        // Required properties

        public Options Opts { get { return opts; } }

        // Unused methods and properties

        public string ConnectedUrl => throw new NotImplementedException();

        public string ConnectedId => throw new NotImplementedException();

        public string[] Servers => throw new NotImplementedException();

        public string[] DiscoveredServers => throw new NotImplementedException();

        public Exception LastError => throw new NotImplementedException();

        public ConnState State => throw new NotImplementedException();

        public IStatistics Stats => throw new NotImplementedException();

        public long MaxPayload => throw new NotImplementedException();

        public void Drain()
        {
            throw new NotImplementedException();
        }

        public void Drain(int timeout)
        {
            throw new NotImplementedException();
        }

        public Task DrainAsync()
        {
            throw new NotImplementedException();
        }

        public Task DrainAsync(int timeout)
        {
            throw new NotImplementedException();
        }

        public void Flush(int timeout)
        {
            throw new NotImplementedException();
        }

        public void Flush()
        {
            throw new NotImplementedException();
        }

        public bool IsClosed()
        {
            throw new NotImplementedException();
        }

        public bool IsDraining()
        {
            throw new NotImplementedException();
        }

        public bool IsReconnecting()
        {
            throw new NotImplementedException();
        }

        public void Publish(string subject, byte[] data, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public void Publish(Msg msg)
        {
            throw new NotImplementedException();
        }

        public void Publish(string subject, string reply, byte[] data)
        {
            throw new NotImplementedException();
        }

        public void Publish(string subject, string reply, byte[] data, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public Msg Request(string subject, byte[] data, int timeout)
        {
            throw new NotImplementedException();
        }

        public Msg Request(string subject, byte[] data, int offset, int count, int timeout)
        {
            throw new NotImplementedException();
        }

        public Msg Request(string subject, byte[] data)
        {
            throw new NotImplementedException();
        }

        public Msg Request(string subject, byte[] data, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public Task<Msg> RequestAsync(string subject, byte[] data, int timeout)
        {
            throw new NotImplementedException();
        }

        public Task<Msg> RequestAsync(string subject, byte[] data, int offset, int count, int timeout)
        {
            throw new NotImplementedException();
        }

        public Task<Msg> RequestAsync(string subject, byte[] data)
        {
            throw new NotImplementedException();
        }

        public Task<Msg> RequestAsync(string subject, byte[] data, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public Task<Msg> RequestAsync(string subject, byte[] data, int timeout, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<Msg> RequestAsync(string subject, byte[] data, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public Task<Msg> RequestAsync(string subject, byte[] data, int offset, int count, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public void ResetStats()
        {
            throw new NotImplementedException();
        }

        public IAsyncSubscription SubscribeAsync(string subject)
        {
            throw new NotImplementedException();
        }

        public IAsyncSubscription SubscribeAsync(string subject, string queue)
        {
            throw new NotImplementedException();
        }

        public IAsyncSubscription SubscribeAsync(string subject, string queue, EventHandler<MsgHandlerEventArgs> handler)
        {
            throw new NotImplementedException();
        }

        public ISyncSubscription SubscribeSync(string subject)
        {
            throw new NotImplementedException();
        }

        public ISyncSubscription SubscribeSync(string subject, string queue)
        {
            throw new NotImplementedException();
        }
    }
}
