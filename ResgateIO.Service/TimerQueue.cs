using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

[assembly: InternalsVisibleTo("ResgateIO.Service.UnitTests")]
namespace ResgateIO.Service
{
    /// <summary>
    /// TimerQueue holds a list of items. When a new item is added to the
    /// queue, the queue callback will be called with the item after
    /// the set queue duration.
    /// </summary>
    /// <typeparam name="T">Item type stored in the queue.</typeparam>
    internal class TimerQueue<T>: IDisposable
    {
        private readonly object locker = new object();

        private Action<T> callback;
        private TimeSpan duration;
        private Timer timer = null;
        private Node first = null;
        private Node last = null;
        private Dictionary<T, Node> nodes;
        private UInt32 counter = 0;

        private class Node
        {
            public readonly T Item;
            public Node Next = null;
            public Node Previous = null;
            public DateTime Time;

            public Node(T item, DateTime time)
            {
                Item = item;
                Time = time;
            }
        }

        /// <summary>
        /// Initializes a new instance of the TimerQueue class.
        /// </summary>
        /// <param name="callback">Callback called when duration expires for an item.</param>
        /// <param name="duration">Duration until the an item in the queue expires.</param>
        public TimerQueue(Action<T> callback, TimeSpan duration)
        {
            this.callback = callback;
            this.duration = duration;
            nodes = new Dictionary<T, Node>();
        }

        /// <summary>
        /// Adds a new item to the queue, starting a timer for the fixed
        /// duration for the queue. Once the duration has passed, the queue
        /// callback will be called.
        /// </summary>
        /// <param name="item">Item to queue.</param>
        public void Add(T item)
        {
            lock (locker)
            {
                // Ensure it is not a duplicate
                if (nodes.ContainsKey(item))
                {
                    throw new InvalidOperationException("Item already in queue");
                }

                Node node = new Node(item, DateTime.Now.Add(duration));

                nodes[item] = node;
                push(node);
                if (node == first)
                {
                    startTimer();
                }
            }
        }

        /// <summary>
        /// Gets the number of items queued.
        /// </summary>
        public int Count
        {
            get {
                lock (locker)
                {
                    return nodes.Count;
                }
            }
        }

        /// <summary>
        /// Removes all items from the queue.
        /// </summary>
        public void Clear()
        {
            clear();
        }

        /// <summary>
        /// Calls the callback for each item in the queue.
        /// Any new item added while flushing, will not be called.
        /// </summary>
        public void Flush()
        {
            Node node = clear();
            while (node != null)
            {
                callback(node.Item);
                node = node.Next;
            }
        }

        /// <summary>
        /// Removes an item from the queue.
        /// Has no effect in case the item was not in the queue.
        /// </summary>
        /// <param name="item">Item to remove</param>
        /// <returns>True if the item was in the queue, otherwise false.</returns>
        public bool Remove(T item)
        {
            lock (locker)
            {
                if (nodes.TryGetValue(item, out Node node))
                {
                    Node f = first;
                    nodes.Remove(item);
                    remove(node);

                    // If the element was first, we must dispose current timer.
                    if (f == node)
                    {
                        disposeTimer();
                        // Start a new timer if there are still items in the queue.
                        if (first != null)
                        {
                            startTimer();
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Sets the time of the item callback back to full duration.
        /// Has no effect in case the item was not in the queue.
        /// </summary>
        /// <param name="item">Item to reset.</param>
        /// <returns>True if the item was in the queue, otherwise false.</returns>
        public bool Reset(T item)
        {
            lock (locker)
            {
                if (nodes.TryGetValue(item, out Node node))
                {
                    node.Time = DateTime.Now.Add(duration);
                    Node f = first;
                    remove(node);
                    push(node);

                    // If the element was first, we need to start a new timer
                    if (f == node)
                    {
                        disposeTimer();
                        startTimer();
                    }
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private Node clear()
        {
            lock (locker)
            {
                var f = first;
                first = null;
                last = null;
                nodes.Clear();
                disposeTimer();
                return f;
            }
        }

        private void remove(Node node)
        {
            if (first == node)
            {
                first = node.Next;
            }
            else
            {
                node.Previous.Next = node.Next;
            }

            if (last == node)
            {
                last = node.Previous;
            }
            else
            {
                node.Next.Previous = node.Previous;
            }
        }

        private void push(Node node)
        {
            if (last == null)
            {
                first = node;
                node.Previous = null;
            }
            else
            {
                last.Next = node;
                node.Previous = last;
            }

            node.Next = null;
            last = node;
        }

        private void startTimer()
        {
            TimeSpan span = first.Time.Subtract(DateTime.Now);
            if (span.CompareTo(TimeSpan.Zero) < 0)
            {
                span = TimeSpan.Zero;
            }
            timer = new Timer(onTimeout, counter, (int)span.TotalMilliseconds, Timeout.Infinite);
        }

        private void onTimeout(Object stateInfo)
        {
            T item;
            uint c;
            bool restart;
            lock (locker)
            {
                // Assert it is the next item in order.
                // If not, a new timer has already been created.
                if ((UInt32)stateInfo != counter)
                {
                    return;
                }

                item = first.Item;
                remove(first);
                nodes.Remove(item);

                disposeTimer();

                restart = first != null;
                c = counter;
            }
            callback(item);
            lock (locker)
            {
                if (restart && c == counter)
                {
                    startTimer();
                }
            }
        }

        private void disposeTimer()
        {
            counter++;
            if (timer != null)
            {
                timer.Dispose();
                timer = null;
            }
        }

        /// <summary>
        /// Releases all resources used by the current instance of TimerQueue.
        /// </summary>
        public void Dispose()
        {
            clear();
        }
    }
}
