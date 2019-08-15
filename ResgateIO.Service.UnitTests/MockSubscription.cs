using System;
using System.Threading.Tasks;
using NATS.Client;

namespace ResgateIO.Service.UnitTests
{
    public class MockSubscription : IAsyncSubscription
    {
        public MockConnection Conn { get;  }
        public string Subject { get; }

        public EventHandler<MsgHandlerEventArgs> Handler { get; }

        public MockSubscription(MockConnection conn, string subject, EventHandler<MsgHandlerEventArgs> handler)
        {
            Conn = conn;
            Subject = subject;
            Handler = handler;
        }

        public void Dispose()
        {
            Conn.Unsubscribe(this);
        }

        public bool Matches(string subject)
        {
            return true;
        }

        // Not used methods and properties

        public string Queue => throw new NotImplementedException();

        public Connection Connection => throw new NotImplementedException();

        public bool IsValid => throw new NotImplementedException();

        public int QueuedMessageCount => throw new NotImplementedException();

        public long PendingByteLimit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public long PendingMessageLimit { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public long PendingBytes => throw new NotImplementedException();

        public long PendingMessages => throw new NotImplementedException();

        public long MaxPendingBytes => throw new NotImplementedException();

        public long MaxPendingMessages => throw new NotImplementedException();

        public long Delivered => throw new NotImplementedException();

        public long Dropped => throw new NotImplementedException();

        public event EventHandler<MsgHandlerEventArgs> MessageHandler;

        public void AutoUnsubscribe(int max)
        {
            throw new NotImplementedException();
        }

        public void ClearMaxPending()
        {
            throw new NotImplementedException();
        }

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

        public void GetMaxPending(out long maxPendingBytes, out long maxPendingMessages)
        {
            throw new NotImplementedException();
        }

        public void GetPending(out long pendingBytes, out long pendingMessages)
        {
            throw new NotImplementedException();
        }

        public void SetPendingLimits(long messageLimit, long bytesLimit)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe()
        {
            throw new NotImplementedException();
        }
    }
}
