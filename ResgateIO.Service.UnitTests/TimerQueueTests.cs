using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Xunit;

namespace ResgateIO.Service.UnitTests
{
    public class TimerQueueTests
    {
        public const int TimeUnits = 8; // Milliseconds
        public const int QueueDuration = 10 * TimeUnits;

        public enum TestAction
        {
            Add,
            Remove,
            Reset,
            Clear,
            Flush,
            Count
        }

        public class TestSet: List<TestEvent>
        {
            public void Add(int delay, TestAction action, int item)
            {
                Add(new TestEvent { Delay = delay, Action = action, Item = item });
            }
        }

        public class TestEvent
        {
            public int Delay;
            public TestAction Action;
            public int Item;
        }

        public static IEnumerable<object[]> GetTestSets()
        {
            // Add single item
            yield return new object[] { new TestSet {
                { 0, TestAction.Add, 1 },
            }, new int[] { 1 } };
            // Add multiple items without delay
            yield return new object[] { new TestSet {
                { 0, TestAction.Add, 1 },
                { 0, TestAction.Add, 2 },
                { 0, TestAction.Add, 3 },
            }, new int[] { 1, 2, 3 } };
            // Add multiple items with delay
            yield return new object[] { new TestSet {
                { 0, TestAction.Add, 1 },
                { 5, TestAction.Add, 2 },
                { 5, TestAction.Add, 3 },
            }, new int[] { 1, 2, 3 } };
            // Add multiple items with long delay
            yield return new object[] { new TestSet {
                { 0, TestAction.Add, 1 },
                { 15, TestAction.Add, 2 },
                { 15, TestAction.Add, 3 },
            }, new int[] { 1, 2, 3 } };
            // Reset on single item
            yield return new object[] { new TestSet {
                { 0, TestAction.Add, 1 },
                { 5, TestAction.Reset, 1 },
            }, new int[] { 1 } };
            // Reset on first item
            yield return new object[] { new TestSet {
                { 0, TestAction.Add, 1 },
                { 1, TestAction.Add, 2 },
                { 1, TestAction.Add, 3 },
                { 1, TestAction.Reset, 1 },
            }, new int[] { 2, 3, 1 } };
            // Reset on middle item
            yield return new object[] { new TestSet {
                { 0, TestAction.Add, 1 },
                { 1, TestAction.Add, 2 },
                { 1, TestAction.Add, 3 },
                { 1, TestAction.Reset, 2 },
            }, new int[] { 1, 3, 2 } };
            // Reset on last item
            yield return new object[] { new TestSet {
                { 0, TestAction.Add, 1 },
                { 1, TestAction.Add, 2 },
                { 1, TestAction.Add, 3 },
                { 1, TestAction.Reset, 3 },
            }, new int[] { 1, 2, 3 } };
            // Remove item
            yield return new object[] { new TestSet {
                { 0, TestAction.Add, 1 },
                { 1, TestAction.Remove, 1 },
            }, new int[] {} };
            // Remove on first item
            yield return new object[] { new TestSet {
                { 0, TestAction.Add, 1 },
                { 1, TestAction.Add, 2 },
                { 1, TestAction.Add, 3 },
                { 1, TestAction.Remove, 1 },
            }, new int[] { 2, 3 } };
            // Remove on middle item
            yield return new object[] { new TestSet {
                { 0, TestAction.Add, 1 },
                { 1, TestAction.Add, 2 },
                { 1, TestAction.Add, 3 },
                { 1, TestAction.Remove, 2 },
            }, new int[] { 1, 3 } };
            // Remove on last item
            yield return new object[] { new TestSet {
                { 0, TestAction.Add, 1 },
                { 1, TestAction.Add, 2 },
                { 1, TestAction.Add, 3 },
                { 1, TestAction.Remove, 3 },
            }, new int[] { 1, 2 } };
            // Clear on empty
            yield return new object[] { new TestSet {
                { 0, TestAction.Clear, 0 },
            }, new int[] {} };
            // Clear on list
            yield return new object[] { new TestSet {
                { 0, TestAction.Add, 1 },
                { 15, TestAction.Add, 2 },
                { 1, TestAction.Clear, 0 },
                { 1, TestAction.Add, 3 },
            }, new int[] { 1, 3 } };
            // Flush on empty
            yield return new object[] { new TestSet {
                { 0, TestAction.Flush, 0 },
            }, new int[] {} };
            // Flush on list
            yield return new object[] { new TestSet {
                { 0, TestAction.Add, 1 },
                { 1, TestAction.Add, 2 },
                { 1, TestAction.Flush, 0 },
                { 1, TestAction.Count, 0 },
            }, new int[] { 1, 2 } };
            // Len
            yield return new object[] { new TestSet {
                { 0, TestAction.Count, 0 },
                { 1, TestAction.Add, 1 },
                { 0, TestAction.Count, 1 },
                { 1, TestAction.Add, 2 },
                { 0, TestAction.Count, 2 },
                { 1, TestAction.Add, 3 },
                { 1, TestAction.Count, 3 },
            }, new int[] { 1, 2, 3 } };
            // Remove on non-existing
            yield return new object[] { new TestSet {
                { 0, TestAction.Add, 1 },
                { 1, TestAction.Add, 2 },
                { 1, TestAction.Add, 3 },
                { 15, TestAction.Remove, 1 },
            }, new int[] { 1, 2, 3 } };
            // Reset on non-existing
            yield return new object[] { new TestSet {
                { 0, TestAction.Add, 1 },
                { 1, TestAction.Add, 2 },
                { 1, TestAction.Add, 3 },
                { 15, TestAction.Reset, 1 },
            }, new int[] { 1, 2, 3 } };
        }

        [Theory]
        [MemberData(nameof(GetTestSets))]
        public void RunSet_CallbackCalledOnExpectedItems(TestSet set, int[] expected)
        {
            object locker = new object();
            int expectedCount = expected.Length;
            AutoResetEvent waiter = new AutoResetEvent(false);
            List<int> items = new List<int>(3);
            TimerQueue<int> queue = new TimerQueue<int>(v => {
                lock (locker)
                {
                    items.Add(v);
                    expectedCount--;
                    if (expectedCount == 0)
                    {
                        waiter.Set();
                    }
                }
            }, TimeSpan.FromMilliseconds(QueueDuration));

            foreach (TestEvent ev in set)
            {
                Thread.Sleep(TimeUnits * ev.Delay);

                switch (ev.Action)
                {
                    case TestAction.Add:
                        queue.Add(ev.Item);
                        break;
                    case TestAction.Remove:
                        queue.Remove(ev.Item);
                        break;
                    case TestAction.Reset:
                        queue.Reset(ev.Item);
                        break;
                    case TestAction.Clear:
                        queue.Clear();
                        break;
                    case TestAction.Flush:
                        queue.Flush();
                        break;
                    case TestAction.Count:
                        Assert.Equal(ev.Item, queue.Count);
                        break;
                }
            }

            waiter.WaitOne(QueueDuration * 4);

            string expectedStr = "[" + String.Join(",", expected.Select(x => x.ToString()).ToArray()) + "]";
            string actualStr = "[" + String.Join(",", items.Select(x => x.ToString()).ToArray()) + "]";

            Assert.Equal(expectedStr, actualStr);
        }

        [Fact]
        public void Add_WithValueAlreadyInQueue_ThrowsInvalidOperationException()
        {
            using (TimerQueue<int> queue = new TimerQueue<int>(v => { }, TimeSpan.FromMilliseconds(QueueDuration)))
            {
                queue.Add(42);
                Assert.Throws<InvalidOperationException>(() => queue.Add(42));
            }
        }
    }
}
