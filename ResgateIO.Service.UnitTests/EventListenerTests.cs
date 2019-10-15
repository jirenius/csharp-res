using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace ResgateIO.Service.UnitTests
{
    public class EventListenerTests : TestsBase
    {
        public EventListenerTests(ITestOutputHelper output) : base(output) { }

        #region ChangeEvent
        public static IEnumerable<object[]> GetChangeEventTestData()
        {
            yield return new object[] { new Dictionary<string, object> { { "foo", 42 } } };
            yield return new object[] { new Dictionary<string, object> { { "foo", "bar" } } };
            yield return new object[] { new Dictionary<string, object> { { "foo", null } } };
            yield return new object[] { new Dictionary<string, object> { { "foo", 12 }, { "bar", true } } };
            yield return new object[] { new Dictionary<string, object> { { "foo", ResAction.Delete } } };
            yield return new object[] { new Dictionary<string, object> { { "foo", new Ref("test.model.bar") } } };
        }

        [Theory]
        [MemberData(nameof(GetChangeEventTestData))]
        public void ChangeEvent_WithApplyChange_CallsListenerWithEventChangeArgs(Dictionary<string, object> changed)
        {
            int called = 0;
            var revert = new Dictionary<string, object> { { "foo", "baz" } };
            Service.AddHandler("model", new DynamicHandler()
                .SetCall(r =>
                {
                    Assert.Equal(0, called);
                    r.ChangeEvent(changed);
                    Assert.Equal(1, called);
                    r.Ok();
                })
                .SetApplyChange((rc, ev) => ev.Revert = revert));
            Service.AddEventListener("model", (sender, ev) =>
            {
                called++;
                var resource = (IResourceContext)sender;
                var args = (ChangeEventArgs)ev;
                Assert.Equal(new Dictionary<string, object>{ { "foo", changed["foo"] } }, args.Changed);
                Assert.Equal(revert, args.Revert);
            });

            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject("event.test.model.change")
                .AssertPayload(new { values = new { foo = changed["foo"] } });
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Theory]
        [MemberData(nameof(GetChangeEventTestData))]
        public void ChangeEvent_WithoutApplyChange_CallsListenerWithEventChangeArgs(Dictionary<string, object> changed)
        {
            int called = 0;
            Service.AddHandler("model", new DynamicHandler()
                .SetCall(r =>
                {
                    Assert.Equal(0, called);
                    r.ChangeEvent(changed);
                    Assert.Equal(1, called);
                    r.Ok();
                }));
            Service.AddEventListener("model", (sender, ev) =>
            {
                called++;
                var resource = (IResourceContext)sender;
                var args = (ChangeEventArgs)ev;
                Assert.Equal(changed, args.Changed);
                Assert.Null(args.Revert);
            });

            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject("event.test.model.change")
                .AssertPayload(new { values = changed });
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Theory]
        [MemberData(nameof(GetChangeEventTestData))]
        public void ChangeEvent_EmptyRevertDictionary_NoCallToListener(Dictionary<string, object> changed)
        {
            bool called = false;
            Service.AddHandler("model", new DynamicHandler()
                .SetCall(r =>
                {
                    r.ChangeEvent(changed);
                    Assert.False(called, "event listener was called");
                    r.Ok();
                })
                .SetApplyChange((rc, changes) => new Dictionary<string, object> { }));
            Service.AddEventListener("model", (sender, ev) => called = true);
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Theory]
        [MemberData(nameof(GetChangeEventTestData))]
        public void ChangeEvent_NullRevertDictionary_NoCallToListener(Dictionary<string, object> changed)
        {
            bool called = false;
            Service.AddHandler("model", new DynamicHandler()
                .SetCall(r =>
                {
                    r.ChangeEvent(changed);
                    Assert.False(called, "event listener was called");
                    r.Ok();
                })
                .SetApplyChange((rc, changes) => null));
            Service.AddEventListener("model", (sender, ev) => called = true);
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }
        #endregion

        #region AddEvent
        public static IEnumerable<object[]> GetAddEventTestData()
        {
            yield return new object[] { 42, 0, new { value = 42, idx = 0 } };
            yield return new object[] { "bar", 1, new { value = "bar", idx = 1 } };
            yield return new object[] { null, 2, new { value = (object)null, idx = 2 } };
            yield return new object[] { true, 3, new { value = true, idx = 3 } };
            yield return new object[] { new Ref("test.model.bar"), 4, new { value = new { rid = "test.model.bar" }, idx = 4 } };
        }

        [Theory]
        [MemberData(nameof(GetAddEventTestData))]
        public void AddEvent_WithApplyAdd_CallsListenerWithAddEventArgs(object value, int idx, object expected)
        {
            int called = 0;
            Service.AddHandler("collection", new DynamicHandler()
                .SetCall(r =>
                {
                    Assert.Equal(0, called);
                    r.AddEvent(value, idx);
                    Assert.Equal(1, called);
                    r.Ok();
                })
                .SetApplyAdd((rc, ev) => { }));
            Service.AddEventListener("collection", (sender, ev) =>
            {
                called++;
                var resource = (IResourceContext)sender;
                var args = (AddEventArgs)ev;
                Assert.Equal(value, args.Value);
                Assert.Equal(idx, args.Idx);
            });
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.collection.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject("event.test.collection.add")
                .AssertPayload(expected);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Theory]
        [MemberData(nameof(GetAddEventTestData))]
        public void AddEvent_WithoutApplyAdd_CallsListenerWithAddEventArgs(object value, int idx, object expected)
        {
            int called = 0;
            Service.AddHandler("collection", new DynamicHandler()
                .SetCall(r =>
                {
                    Assert.Equal(0, called);
                    r.AddEvent(value, idx);
                    Assert.Equal(1, called);
                    r.Ok();
                }));
            Service.AddEventListener("collection", (sender, ev) =>
            {
                called++;
                var resource = (IResourceContext)sender;
                var args = (AddEventArgs)ev;
                Assert.Equal(value, args.Value);
                Assert.Equal(idx, args.Idx);
            });
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.collection.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject("event.test.collection.add")
                .AssertPayload(expected);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }
        #endregion

        #region RemoveEvent
        public static IEnumerable<object[]> GetRemoveEventTestData()
        {
            yield return new object[] { 0, new { idx = 0 } };
            yield return new object[] { 1, new { idx = 1 } };
            yield return new object[] { 2, new { idx = 2 } };
        }

        [Theory]
        [MemberData(nameof(GetRemoveEventTestData))]
        public void RemoveEvent_WithApplyRemove_CallsListenerWithRemoveEventArgs(int idx, object expected)
        {
            int called = 0;
            Service.AddHandler("collection", new DynamicHandler()
                .SetCall(r =>
                {
                    Assert.Equal(0, called);
                    r.RemoveEvent(idx);
                    Assert.Equal(1, called);
                    r.Ok();
                })
                .SetApplyRemove((rc, ev) => ev.Value = Test.IntValue));
            Service.AddEventListener("collection", (sender, ev) =>
            {
                called++;
                var resource = (IResourceContext)sender;
                var args = (RemoveEventArgs)ev;
                Assert.Equal(Test.IntValue, args.Value);
                Assert.Equal(idx, args.Idx);
            });
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.collection.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject("event.test.collection.remove")
                .AssertPayload(expected);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Theory]
        [MemberData(nameof(GetRemoveEventTestData))]
        public void RemoveEvent_WithoutApplyRemove_CallsListenerWithRemoveEventArgsWithNullValue(int idx, object expected)
        {
            int called = 0;
            Service.AddHandler("collection", new DynamicHandler()
                .SetCall(r =>
                {
                    Assert.Equal(0, called);
                    r.RemoveEvent(idx);
                    Assert.Equal(1, called);
                    r.Ok();
                }));
            Service.AddEventListener("collection", (sender, ev) =>
            {
                called++;
                var resource = (IResourceContext)sender;
                var args = (RemoveEventArgs)ev;
                Assert.Null(args.Value);
                Assert.Equal(idx, args.Idx);
            });
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.collection.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject("event.test.collection.remove")
                .AssertPayload(expected);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }
        #endregion

        #region CreateEvent
        [Fact]
        public void CreateEvent_WithApplyCreate_CallsListenerWithCreateEventArgs()
        {
            int called = 0;
            Service.AddHandler("model", new DynamicHandler()
                .SetCall(r =>
                {
                    Assert.Equal(0, called);
                    r.CreateEvent(Test.Model);
                    Assert.Equal(1, called);
                    r.Ok();
                })
                .SetApplyCreate((rc, ev) => { }));
            Service.AddEventListener("model", (sender, ev) =>
            {
                called++;
                var resource = (IResourceContext)sender;
                var args = (CreateEventArgs)ev;
                Assert.Equal(Test.Model, args.Data);
            });
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject("event.test.model.create")
                .AssertNoPayload();
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Fact]
        public void CreateEvent_WithoutApplyCreate_CallsListenerWithCreateEventArgs()
        {
            int called = 0;
            Service.AddHandler("model", new DynamicHandler()
                .SetCall(r =>
                {
                    Assert.Equal(0, called);
                    r.CreateEvent(Test.Model);
                    Assert.Equal(1, called);
                    r.Ok();
                }));
            Service.AddEventListener("model", (sender, ev) =>
            {
                called++;
                var resource = (IResourceContext)sender;
                var args = (CreateEventArgs)ev;
                Assert.Equal(Test.Model, args.Data);
            });
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject("event.test.model.create")
                .AssertNoPayload();
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }
        #endregion

        #region DeleteEvent
        [Fact]
        public void DeleteEvent_WithApplyDelete_CallsListenerWithDeleteEventArgs()
        {
            int called = 0;
            Service.AddHandler("model", new DynamicHandler()
                .SetCall(r =>
                {
                    Assert.Equal(0, called);
                    r.DeleteEvent();
                    Assert.Equal(1, called);
                    r.Ok();
                })
                .SetApplyDelete((rc, ev) => ev.Data = Test.Model));
            Service.AddEventListener("model", (sender, ev) =>
            {
                called++;
                var resource = (IResourceContext)sender;
                var args = (DeleteEventArgs)ev;
                Assert.Equal(Test.Model, args.Data);
            });
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject("event.test.model.delete")
                .AssertNoPayload();
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Fact]
        public void DeleteEvent_WithoutApplyDelete_CallsListenerWithDeleteEventArgsWithNullData()
        {
            int called = 0;
            Service.AddHandler("model", new DynamicHandler()
                .SetCall(r =>
                {
                    Assert.Equal(0, called);
                    r.DeleteEvent();
                    Assert.Equal(1, called);
                    r.Ok();
                }));
            Service.AddEventListener("model", (sender, ev) =>
            {
                called++;
                var resource = (IResourceContext)sender;
                var args = (DeleteEventArgs)ev;
                Assert.Null(args.Data);
            });
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject("event.test.model.delete")
                .AssertNoPayload();
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }
        #endregion
    }
}
