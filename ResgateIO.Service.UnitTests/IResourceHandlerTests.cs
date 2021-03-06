using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace ResgateIO.Service.UnitTests
{
    public class IResourceHandlerTests : TestsBase
    {
        public IResourceHandlerTests(ITestOutputHelper output) : base(output) { }

        #region ApplyChange
        public static IEnumerable<object[]> GetApplyChangeTestData()
        {
            yield return new object[] { new Dictionary<string, object> { { "foo", 42 } } };
            yield return new object[] { new Dictionary<string, object> { { "foo", "bar" } } };
            yield return new object[] { new Dictionary<string, object> { { "foo", null } } };
            yield return new object[] { new Dictionary<string, object> { { "foo", 12 }, { "bar", true } } };
            yield return new object[] { new Dictionary<string, object> { { "foo", ResAction.Delete } } };
            yield return new object[] { new Dictionary<string, object> { { "foo", new Ref("test.model.bar") } } };
        }

        [Theory]
        [MemberData(nameof(GetApplyChangeTestData))]
        public void ApplyChange_WithChangedProperty_CallsApplyChangeAndSendsChangeEvent(Dictionary<string, object> changed)
        {
            bool called = false;
            Service.AddHandler("model", new DynamicHandler()
                .Call(r =>
                {
                    r.ChangeEvent(changed);
                    Assert.True(called, "apply callback was not called");
                    r.Ok();
                })
                .ApplyChange((rc, ev) =>
                {
                    called = true;
                    Assert.Equal(changed, ev.ChangedProperties);
                    Assert.Equal("test.model", rc.ResourceName);
                    ev.SetRevert(new Dictionary<string, object> { { "foo", "baz" } });
                }));
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
        [MemberData(nameof(GetApplyChangeTestData))]
        public void ApplyChange_WithChangedPropertyUsingWith_CallsApplyChangeAndSendsChangeEvent(Dictionary<string, object> changed)
        {
            AutoResetEvent re = new AutoResetEvent(false);
            bool called = false;
            Service.AddHandler("model", new DynamicHandler()
                .Get(r => r.Model(Test.Model))
                .ApplyChange((rc, ev) =>
                {
                    called = true;
                    Assert.Equal(changed, ev.ChangedProperties);
                    Assert.Equal("test.model", rc.ResourceName);
                    ev.SetRevert(new Dictionary<string, object> { { "foo", "baz" } });
                }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            Service.With("test.model", r =>
            {
                r.ChangeEvent(changed);
                Assert.True(called, "apply callback was not called");
                re.Set();
            });
            Assert.True(re.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg()
                .AssertSubject("event.test.model.change")
                .AssertPayload(new { values = new { foo = changed["foo"] } });
        }

        [Theory]
        [MemberData(nameof(GetApplyChangeTestData))]
        public void ApplyChange_EmptyRevertDictionary_NoChangeEvent(Dictionary<string, object> changed)
        {
            bool called = false;
            Service.AddHandler("model", new DynamicHandler()
                .Call(r =>
                {
                    r.ChangeEvent(changed);
                    Assert.True(called, "apply callback was not called");
                    r.Ok();
                })
                .ApplyChange((rc, ev) =>
                {
                    called = true;
                    Assert.Equal(changed, ev.ChangedProperties);
                    Assert.Equal("test.model", rc.ResourceName);
                    ev.SetRevert(new Dictionary<string, object> {});
                }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Theory]
        [MemberData(nameof(GetApplyChangeTestData))]
        public void ApplyChange_NullRevertDictionaryUsingWith_ChangeEvent(Dictionary<string, object> changed)
        {
            AutoResetEvent re = new AutoResetEvent(false);
            bool called = false;
            Service.AddHandler("model", new DynamicHandler()
                .Get(r => r.Model(Test.Model))
                .Call(r =>
                {
                    r.ChangeEvent(changed);
                    Assert.True(called, "apply callback was not called");
                    r.Ok();
                })
                .ApplyChange((rc, ev) =>
                {
                    called = true;
                    Assert.Equal(changed, ev.ChangedProperties);
                    Assert.Equal("test.model", rc.ResourceName);
                }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            Service.With("test.model", r =>
            {
                r.ChangeEvent(changed);
                Assert.True(called, "apply callback was not called");
                re.Set();
            });
            Assert.True(re.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg()
                .AssertSubject("event.test.model.change")
                .AssertPayload(new { values = changed });
        }
        #endregion

        #region ApplyAdd
        public static IEnumerable<object[]> GetApplyAddTestData()
        {
            yield return new object[] { 42, 0, new { value = 42, idx = 0 } };
            yield return new object[] { "bar", 1, new { value = "bar", idx = 1 } };
            yield return new object[] { null, 2, new { value = (object)null, idx = 2 } };
            yield return new object[] { true, 3, new { value = true, idx = 3 } };
            yield return new object[] { new Ref("test.model.bar"), 4, new { value = new { rid = "test.model.bar" }, idx = 4 } };
        }

        [Theory]
        [MemberData(nameof(GetApplyAddTestData))]
        public void ApplyAdd_UsingRequest_CallsApplyAddAndSendsAddEvent(object value, int idx, object expected)
        {
            bool called = false;
            Service.AddHandler("collection", new DynamicHandler()
                .Call(r =>
                {
                    r.AddEvent(value, idx);
                    Assert.True(called, "apply callback was not called");
                    r.Ok();
                })
                .ApplyAdd((rc, ev) =>
                {
                    called = true;
                    Assert.Equal("test.collection", rc.ResourceName);
                    Assert.Equal(value, ev.Value);
                    Assert.Equal(idx, ev.Idx);
                }));
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
        [MemberData(nameof(GetApplyAddTestData))]
        public void ApplyAdd_UsingWith_CallsApplyAddAndSendsAddEvent(object value, int idx, object expected)
        {
            AutoResetEvent re = new AutoResetEvent(false);
            bool called = false;
            Service.AddHandler("collection", new DynamicHandler()
                .Get(r => r.Collection(Test.Collection))
                .ApplyAdd((rc, ev) =>
                {
                    called = true;
                    Assert.Equal("test.collection", rc.ResourceName);
                    Assert.Equal(value, ev.Value);
                    Assert.Equal(idx, ev.Idx);
                }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            Service.With("test.collection", r =>
            {
                r.AddEvent(value, idx);
                Assert.True(called, "apply callback was not called");
                re.Set();
            });
            Assert.True(re.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg()
                .AssertSubject("event.test.collection.add")
                .AssertPayload(expected);
        }
        #endregion

        #region ApplyRemove
        public static IEnumerable<object[]> GetApplyRemoveTestData()
        {
            yield return new object[] { 0, new { idx = 0 } };
            yield return new object[] { 1, new { idx = 1 } };
            yield return new object[] { 2, new { idx = 2 } };
        }

        [Theory]
        [MemberData(nameof(GetApplyRemoveTestData))]
        public void ApplyRemove_UsingRequest_CallsApplyRemoveAndSendsRemoveEvent(int idx, object expected)
        {
            bool called = false;
            Service.AddHandler("collection", new DynamicHandler()
                .Call(r =>
                {
                    r.RemoveEvent(idx);
                    Assert.True(called, "apply callback was not called");
                    r.Ok();
                })
                .ApplyRemove((rc, ev) =>
                {
                    called = true;
                    Assert.Equal("test.collection", rc.ResourceName);
                    Assert.Equal(idx, ev.Idx);
                    ev.SetRevert("foo");
                }));
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
        [MemberData(nameof(GetApplyRemoveTestData))]
        public void ApplyRemove_UsingWith_CallsApplyRemoveAndSendsRemoveEvent(int idx, object expected)
        {
            AutoResetEvent re = new AutoResetEvent(false);
            bool called = false;
            Service.AddHandler("collection", new DynamicHandler()
                .Get(r => r.Collection(Test.Collection))
                .ApplyRemove((rc, ev) =>
                {
                    called = true;
                    Assert.Equal("test.collection", rc.ResourceName);
                    Assert.Equal(idx, ev.Idx);
                    ev.SetRevert("foo");
                }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            Service.With("test.collection", r =>
            {
                r.RemoveEvent(idx);
                Assert.True(called, "apply callback was not called");
                re.Set();
            });
            Assert.True(re.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg()
                .AssertSubject("event.test.collection.remove")
                .AssertPayload(expected);
        }
        #endregion

        #region ApplyCreate
        [Fact]
        public void ApplyCreate_UsingRequest_CallsApplyCreateAndSendsCreateEvent()
        {
            bool called = false;
            Service.AddHandler("model", new DynamicHandler()
                .Call(r =>
                {
                    r.CreateEvent(Test.Model);
                    Assert.True(called, "apply callback was not called");
                    r.Ok();
                })
                .ApplyCreate((rc, ev) =>
                {
                    called = true;
                    Assert.Equal("test.model", rc.ResourceName);
                    Assert.Equal(Test.Model, ev.Data);
                }));
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
        public void ApplyCreate_UsingWith_CallsApplyCreateAndSendsCreateEvent()
        {
            AutoResetEvent re = new AutoResetEvent(false);
            bool called = false;
            Service.AddHandler("model", new DynamicHandler()
                .ApplyCreate((rc, ev) =>
                {
                    called = true;
                    Assert.Equal("test.model", rc.ResourceName);
                    Assert.Equal(Test.Model, ev.Data);
                }));
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                r.CreateEvent(Test.Model);
                Assert.True(called, "apply callback was not called");
                re.Set();
            });
            Assert.True(re.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg()
                .AssertSubject("event.test.model.create")
                .AssertNoPayload();
        }
        #endregion

        #region ApplyDelete
        [Fact]
        public void ApplyDelete_UsingRequest_CallsApplyDeleteAndSendsDeleteEvent()
        {
            bool called = false;
            Service.AddHandler("model", new DynamicHandler()
                .Call(r =>
                {
                    r.DeleteEvent();
                    Assert.True(called, "apply callback was not called");
                    r.Ok();
                })
                .ApplyDelete((rc, ev) =>
                {
                    called = true;
                    Assert.Equal("test.model", rc.ResourceName);
                    ev.SetRevert(Test.Model);
                }));
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
        public void ApplyDelete_UsingWith_CallsApplyDeleteAndSendsDeleteEvent()
        {
            AutoResetEvent re = new AutoResetEvent(false);
            bool called = false;
            Service.AddHandler("model", new DynamicHandler()
                .ApplyDelete((rc, ev) =>
                {
                    called = true;
                    Assert.Equal("test.model", rc.ResourceName);
                    ev.SetRevert(Test.Model);
                }));
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                r.DeleteEvent();
                Assert.True(called, "apply callback was not called");
                re.Set();
            });
            Assert.True(re.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg()
                .AssertSubject("event.test.model.delete")
                .AssertNoPayload();
        }
        #endregion
    }
}
