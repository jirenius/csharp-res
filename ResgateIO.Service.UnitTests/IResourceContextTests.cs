using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace ResgateIO.Service.UnitTests
{
    public class IResourceContextTests : TestsBase
    {
        public IResourceContextTests(ITestOutputHelper output) : base(output) { }

        #region Properties
        public static IEnumerable<object[]> GetPropertiesTestData()
        {
            yield return new object[] { "model", "test.model", "", "{}" };
            yield return new object[] { "model", "test.model", null, "{}" };
            yield return new object[] { "model.foo", "test.model.foo", null, "{}" };
            yield return new object[] { "model.foo.bar", "test.model.foo.bar", null, "{}" };
            // Pattern with placeholders
            yield return new object[] { "model.$id", "test.model.42", null, "{\"id\":\"42\"}" };
            yield return new object[] { "model.$id.bar", "test.model.foo.bar", null, "{\"id\":\"foo\"}" };
            yield return new object[] { "model.$id.bar.$type", "test.model.foo.bar.baz", null, "{\"id\":\"foo\",\"type\":\"baz\"}" };
            // Pattern with full wild card
            yield return new object[] { "model.>", "test.model.42", null, "{}" };
            yield return new object[] { "model.>", "test.model.foo.42", null, "{}" };
            yield return new object[] { "model.$id.>", "test.model.foo.bar", null, "{\"id\":\"foo\"}" };
            yield return new object[] { "model.$id.>", "test.model.foo.bar.42", null, "{\"id\":\"foo\"}" };
            yield return new object[] { "model.foo.>", "test.model.foo.bar", null, "{}" };
            yield return new object[] { "model.foo.>", "test.model.foo.bar.42", null, "{}" };
            // Simple RID with query
            yield return new object[] { "model.foo", "test.model.foo", "foo=bar", "{}" };
            yield return new object[] { "model.foo.bar", "test.model.foo.bar", "bar.baz=zoo.42", "{}" };
            yield return new object[] { "model.foo.bar.baz", "test.model.foo.bar.baz", "foo=?bar*.>zoo", "{}" };
            // Pattern with placeholders, wildcards, and query
            yield return new object[] { "model.$id", "test.model.42", "foo=bar", "{\"id\":\"42\"}" };
            yield return new object[] { "model.>", "test.model.42", "foo=bar", "{}" };
            yield return new object[] { "model.$id.>", "test.model.foo.bar.42", "foo=bar", "{\"id\":\"foo\"}" };
        }

        [Theory]
        [MemberData(nameof(GetPropertiesTestData))]
        public void Properties_WithValidRID_ReturnsCorrectValue(string pattern, string resourceName, string query, string expectedPathParams)
        {
            var handler = new DynamicHandler();
            Service.AddHandler(pattern, handler.SetCall(r =>
            {
                Assert.Equal(Service, r.Service);
                Assert.Equal(resourceName, r.ResourceName);
                Assert.Equal(query == null ? "" : query, r.Query);
                Assert.Equal(resourceName, r.Group);
                Assert.Equal(handler, r.Handler);
                Assert.NotNull(r.PathParams);
                Test.AssertJsonEqual(JToken.Parse(expectedPathParams), r.PathParams);
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call." + resourceName + ".method", new RequestDto { CID = Test.CID, Query = query });
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Theory]
        [MemberData(nameof(GetPropertiesTestData))]
        public void Properties_WithValidRIDUsingWith_ReturnsCorrectValue(string pattern, string resourceName, string query, string expectedPathParams)
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            var handler = new DynamicHandler();
            Service.AddHandler(pattern, handler.SetCall(r => r.Ok()));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string rid = resourceName + (String.IsNullOrEmpty(query) ? "" : "?" + query);
            Service.With(rid, r =>
            {
                Assert.Equal(Service, r.Service);
                Assert.Equal(resourceName, r.ResourceName);
                Assert.Equal(query == null ? "" : query, r.Query);
                Assert.Equal(handler, r.Handler);
                Assert.NotNull(r.PathParams);
                Test.AssertJsonEqual(JToken.Parse(expectedPathParams), r.PathParams);
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
        }

        public static IEnumerable<object[]> GetPropertyGroupTestData()
        {
            yield return new object[] { "model", "test.model", "foo", "foo" };
            yield return new object[] { "model.foo", "test.model.foo", "bar", "bar" };
            yield return new object[] { "model.$id", "test.model.42", "foo.bar", "foo.bar" };
            yield return new object[] { "model.$id", "test.model.42", "${id}", "42" };
            yield return new object[] { "model.$id", "test.model.42", "${id}foo", "42foo" };
            yield return new object[] { "model.$id", "test.model.42", "foo${id}", "foo42" };
            yield return new object[] { "model.$id", "test.model.42", "foo${id}bar", "foo42bar" };
            yield return new object[] { "model.$id.$type", "test.model.42.foo", "foo.bar", "foo.bar" };
            yield return new object[] { "model.$id.$type", "test.model.42.foo", "${id}", "42" };
            yield return new object[] { "model.$id.$type", "test.model.42.foo", "${type}", "foo" };
            yield return new object[] { "model.$id.$type", "test.model.42.foo", "${id}${type}", "42foo" };
            yield return new object[] { "model.$id.$type", "test.model.42.foo", "${id}.${type}", "42.foo" };
            yield return new object[] { "model.$id.$type", "test.model.42.foo", "${type}${id}", "foo42" };
            yield return new object[] { "model.$id.$type", "test.model.42.foo", "bar.${type}.${id}.baz", "bar.foo.42.baz" };
        }

        [Theory]
        [MemberData(nameof(GetPropertyGroupTestData))]
        public void PropertyGroup_WithValidGroup_ReturnsCorrectValue(string pattern, string resourceName, string group, string expectedGroup)
        {
            Service.AddHandler(pattern, group, new DynamicHandler().SetCall(r =>
            {
                Assert.Equal(expectedGroup, r.Group);
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call." + resourceName + ".method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Theory]
        [MemberData(nameof(GetPropertyGroupTestData))]
        public void PropertyGroup_WithValidGroupUsingWith_ReturnsCorrectValue(string pattern, string resourceName, string group, string expectedGroup)
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler(pattern, group, new DynamicHandler().SetCall(r => r.Ok()));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            Service.With(resourceName, r =>
            {
                Assert.Equal(expectedGroup, r.Group);
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
        }
        #endregion

        #region PathParam
        public static IEnumerable<object[]> GetPathParamTestData()
        {
            yield return new object[] { "model.$id", "test.model.42", "id", "42" };
            yield return new object[] { "model.$id.bar", "test.model.foo.bar", "id", "foo" };
            yield return new object[] { "model.$id.bar.$type", "test.model.foo.bar.baz", "id", "foo" };
            yield return new object[] { "model.$id.bar.$type", "test.model.foo.bar.baz", "type", "baz" };
            yield return new object[] { "model.$id.>", "test.model.foo.bar", "id", "foo" };
            yield return new object[] { "model.$id.bar.$type.>", "test.model.foo.bar.baz.zoo", "id", "foo" };
            yield return new object[] { "model.$id.bar.$type.>", "test.model.foo.bar.baz.zoo", "type", "baz" };
        }

        [Theory]
        [MemberData(nameof(GetPathParamTestData))]
        public void PathParam_WithValidRID_ReturnsCorrectValue(string pattern, string resourceName, string key, string expected)
        {
            Service.AddHandler(pattern, new DynamicHandler().SetCall(r =>
            {
                Assert.Equal(expected, r.PathParam(key));
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call." + resourceName + ".method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Theory]
        [MemberData(nameof(GetPathParamTestData))]
        public void PathParam_WithValidRIDUsingWith_ReturnsCorrectValue(string pattern, string resourceName, string key, string expected)
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler(pattern, new DynamicHandler().SetCall(r => r.Ok()));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            Service.With(resourceName, r =>
            {
                Assert.Equal(expected, r.PathParam(key));
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
        }

        [Fact]
        public void PathParam_WithInvalidPlaceholderKey_ThrowsException()
        {
            Service.AddHandler("model", new DynamicHandler().SetCall(r =>
            {
                r.PathParam("foo");
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInternalError);
        }

        [Fact]
        public void PathParam_WithInvalidPlaceholderKeyUsingWith_ThrowsException()
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            bool hasException = false;
            Service.AddHandler("model", new DynamicHandler().SetCall(r =>
            {
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            Service.With("test.model", r =>
            {
                try
                {
                    r.PathParam("foo");
                }
                catch (Exception)
                {
                    hasException = true;
                }
                finally
                {
                    ev.Set();
                }
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Assert.True(hasException, "no exception thrown");
        }
        #endregion

        #region Value
        public static IEnumerable<object[]> GetValueTestData()
        {
            // With Model returns model
            yield return new object[] {
                "Model",
                (Action<IGetRequest>)(r => r.Model(Test.Model)),
                (Action<IResourceContext>)(r => Assert.Equal(Test.Model, r.Value<ModelDto>()))
            };

            // With Model with query returns model
            yield return new object[] {
                "ModelWithQuery",
                (Action<IGetRequest>)(r => r.Model(Test.Model, Test.NormalizedQuery)),
                (Action<IResourceContext>)(r => Assert.Equal(Test.Model, r.Value<ModelDto>()))
            };

            // With Collection returns collection
            yield return new object[] {
                "Collection",
                (Action<IGetRequest>)(r => r.Collection(Test.Collection)),
                (Action<IResourceContext>)(r => Assert.Equal(Test.Collection, r.Value<object[]>()))
            };

            // With Collection with query returns collection
            yield return new object[] {
                "CollectionWithQuery",
                (Action<IGetRequest>)(r => r.Collection(Test.Collection, Test.NormalizedQuery)),
                (Action<IResourceContext>)(r => Assert.Equal(Test.Collection, r.Value<object[]>()))
            };

            // With Error returns null
            yield return new object[] {
                "Error",
                (Action<IGetRequest>)(r => r.Error(Test.CustomError)),
                (Action<IResourceContext>)(r => Assert.Null(r.Value<ModelDto>()))
            };

            // With NotFound returns null
            yield return new object[] {
                "NotFound",
                (Action<IGetRequest>)(r => r.NotFound()),
                (Action<IResourceContext>)(r => Assert.Null(r.Value<ModelDto>()))
            };

            // With InvalidQuery returns null
            yield return new object[] {
                "InvalidQuery",
                (Action<IGetRequest>)(r => r.InvalidQuery()),
                (Action<IResourceContext>)(r => Assert.Null(r.Value<ModelDto>()))
            };

            // With InvalidQuery with message returns null
            yield return new object[] {
                "InvalidQuery_WithMessage",
                (Action<IGetRequest>)(r => r.InvalidQuery(Test.ErrorMessage)),
                (Action<IResourceContext>)(r => Assert.Null(r.Value<ModelDto>()))
            };

            // With InvalidQuery with message and data returns null
            yield return new object[] {
                "InvalidQuery_WithMessageAndData",
                (Action<IGetRequest>)(r => r.InvalidQuery(Test.ErrorMessage, Test.ErrorData)),
                (Action<IResourceContext>)(r => Assert.Null(r.Value<ModelDto>()))
            };

            // With different resource type throws InvalidCastException
            yield return new object[] {
                "Model_WithDifferentResourceType",
                (Action<IGetRequest>)(r => r.Model(new { id = 42, foo = "bar" })),
                (Action<IResourceContext>)(r => Assert.Throws<InvalidCastException>(() => r.Value<ModelDto>()))
            };

            // Calling Value throws InvalidOperationException
            yield return new object[] {
                "Value",
                (Action<IGetRequest>)(r => r.Value<ModelDto>()),
                (Action<IResourceContext>)(r => Assert.Throws<InvalidOperationException>(() => r.Value<ModelDto>()))
            };

            // Calling RequireValue throws InvalidOperationException
            yield return new object[] {
                "RequireValue",
                (Action<IGetRequest>)(r => r.RequireValue<ModelDto>()),
                (Action<IResourceContext>)(r => Assert.Throws<InvalidOperationException>(() => r.Value<ModelDto>()))
            };

            // With Timeout_Int and Model returns model
            yield return new object[] {
                "Model_WithTimeout_Int",
                (Action<IGetRequest>)(r => { r.Timeout(5000); r.Model(Test.Model); }),
                (Action<IResourceContext>)(r => Assert.Equal(Test.Model, r.Value<ModelDto>()))
            };

            // With Timeout_Timespan and Model returns model
            yield return new object[] {
                "Model_WithTimeout_TimeSpan",
                (Action<IGetRequest>)(r => { r.Timeout(TimeSpan.FromSeconds(5)); r.Model(Test.Model); }),
                (Action<IResourceContext>)(r => Assert.Equal(Test.Model, r.Value<ModelDto>()))
            };
        }

        [Theory]
        [MemberData(nameof(GetValueTestData))]
        public void Value_UsingCall_ReturnsCorrectValue(string resourceName, Action<IGetRequest> getHandler, Action<IResourceContext> assertion)
        {
            Service.AddHandler(resourceName, new DynamicHandler()
                .SetCall(r =>
                {
                    assertion(r);
                    r.Ok();
                })
                .SetGet(getHandler)
           );
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test."+resourceName+".method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Theory]
        [MemberData(nameof(GetValueTestData))]
        public void Value_UsingWith_ReturnsCorrectValue(string resourceName, Action<IGetRequest> getHandler, Action<IResourceContext> assertion)
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler(resourceName, new DynamicHandler().SetGet(getHandler));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            Service.With("test."+resourceName, r =>
            {
                assertion(r);
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
        }
        #endregion

        #region RequireValue
        public static IEnumerable<object[]> GetRequireValueTestData()
        {
            // With Model returns model
            yield return new object[] {
                "Model",
                (Action<IGetRequest>)(r => r.Model(Test.Model)),
                (Action<IResourceContext>)(r => Assert.Equal(Test.Model, r.RequireValue<ModelDto>()))
            };

            // With Model with query returns model
            yield return new object[] {
                "ModelWithQuery",
                (Action<IGetRequest>)(r => r.Model(Test.Model, Test.NormalizedQuery)),
                (Action<IResourceContext>)(r => Assert.Equal(Test.Model, r.RequireValue<ModelDto>()))
            };

            // With Collection returns collection
            yield return new object[] {
                "Collection",
                (Action<IGetRequest>)(r => r.Collection(Test.Collection)),
                (Action<IResourceContext>)(r => Assert.Equal(Test.Collection, r.RequireValue<object[]>()))
            };

            // With Collection with query returns collection
            yield return new object[] {
                "CollectionWithQuery",
                (Action<IGetRequest>)(r => r.Collection(Test.Collection, Test.NormalizedQuery)),
                (Action<IResourceContext>)(r => Assert.Equal(Test.Collection, r.RequireValue<object[]>()))
            };

            // With Error returns throws ResException
            yield return new object[] {
                "Error",
                (Action<IGetRequest>)(r => r.Error(Test.CustomError)),
                (Action<IResourceContext>)(r => Assert.Throws<ResException>(() => r.RequireValue<ModelDto>()))
            };

            // With NotFound throws ResException
            yield return new object[] {
                "NotFound",
                (Action<IGetRequest>)(r => r.NotFound()),
                (Action<IResourceContext>)(r => Assert.Throws<ResException>(() => r.RequireValue<ModelDto>()))
            };

            // With InvalidQuery returns null
            yield return new object[] {
                "InvalidQuery",
                (Action<IGetRequest>)(r => r.InvalidQuery()),
                (Action<IResourceContext>)(r => Assert.Throws<ResException>(() => r.RequireValue<ModelDto>()))
            };

            // With InvalidQuery with message returns null
            yield return new object[] {
                "InvalidQuery_WithMessage",
                (Action<IGetRequest>)(r => r.InvalidQuery(Test.ErrorMessage)),
                (Action<IResourceContext>)(r => Assert.Throws<ResException>(() => r.RequireValue<ModelDto>()))
            };

            // With InvalidQuery with message and data returns null
            yield return new object[] {
                "InvalidQuery_WithMessageAndData",
                (Action<IGetRequest>)(r => r.InvalidQuery(Test.ErrorMessage, Test.ErrorData)),
                (Action<IResourceContext>)(r => Assert.Throws<ResException>(() => r.RequireValue<ModelDto>()))
            };

            // With different resource type throws InvalidCastException
            yield return new object[] {
                "Model_WithDifferentResourceType",
                (Action<IGetRequest>)(r => r.Model(new { id = 42, foo = "bar" })),
                (Action<IResourceContext>)(r => Assert.Throws<InvalidCastException>(() => r.RequireValue<ModelDto>()))
            };

            // Calling Value throws InvalidOperationException
            yield return new object[] {
                "Value",
                (Action<IGetRequest>)(r => r.Value<ModelDto>()),
                (Action<IResourceContext>)(r => Assert.Throws<InvalidOperationException>(() => r.RequireValue<ModelDto>()))
            };

            // Calling RequireValue throws InvalidOperationException
            yield return new object[] {
                "RequireValue",
                (Action<IGetRequest>)(r => r.RequireValue<ModelDto>()),
                (Action<IResourceContext>)(r => Assert.Throws<InvalidOperationException>(() => r.RequireValue<ModelDto>()))
            };

            // With Timeout_Int and Model returns model
            yield return new object[] {
                "Model_WithTimeout_Int",
                (Action<IGetRequest>)(r => { r.Timeout(5000); r.Model(Test.Model); }),
                (Action<IResourceContext>)(r => Assert.Equal(Test.Model, r.RequireValue<ModelDto>()))
            };

            // With Timeout_Timespan and Model returns model
            yield return new object[] {
                "Model_WithTimeout_TimeSpan",
                (Action<IGetRequest>)(r => { r.Timeout(TimeSpan.FromSeconds(5)); r.Model(Test.Model); }),
                (Action<IResourceContext>)(r => Assert.Equal(Test.Model, r.RequireValue<ModelDto>()))
            };
        }

        [Theory]
        [MemberData(nameof(GetRequireValueTestData))]
        public void RequireValue_UsingCall_ReturnsCorrectValue(string resourceName, Action<IGetRequest> getHandler, Action<IResourceContext> assertion)
        {
            Service.AddHandler(resourceName, new DynamicHandler()
                .SetCall(r =>
                {
                    assertion(r);
                    r.Ok();
                })
                .SetGet(getHandler)
           );
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test."+resourceName+".method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Theory]
        [MemberData(nameof(GetRequireValueTestData))]
        public void RequireValue_UsingWith_ReturnsCorrectValue(string resourceName, Action<IGetRequest> getHandler, Action<IResourceContext> assertion)
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler(resourceName, new DynamicHandler().SetGet(getHandler));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            Service.With("test."+ resourceName, r =>
            {
                assertion(r);
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
        }
        #endregion

        #region Event
        public static IEnumerable<object[]> GetEventTestData()
        {
            yield return new object[] { "foo", "{\"bar\":42}" };
            yield return new object[] { "foo", "{\"bar\":42,\"baz\":null}" };
            yield return new object[] { "foo", "[\"bar\",42]" };
            yield return new object[] { "foo", "\"bar\"" };
            yield return new object[] { "foo", "null" };
            yield return new object[] { "foo", null };
            yield return new object[] { "_foo_", "{\"bar\":42}" };
            yield return new object[] { "12", "{\"bar\":42}" };
            yield return new object[] { "<_!", "{\"bar\":42}" };
        }

        [Theory]
        [MemberData(nameof(GetEventTestData))]
        public void Event_ValidEvent_SendsEvent(string eventName, string payload)
        {
            Service.AddHandler("model", new DynamicHandler().SetCall(r =>
            {
                if (payload == null)
                    r.Event(eventName);
                else
                    r.Event(eventName, JToken.Parse(payload));
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            if (payload == null)
            {
                Conn.GetMsg()
                    .AssertSubject("event.test.model." + eventName)
                    .AssertNoPayload();
            }
            else
            {
                Conn.GetMsg()
                    .AssertSubject("event.test.model." + eventName)
                    .AssertPayload(JToken.Parse(payload));
            }
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Theory]
        [MemberData(nameof(GetEventTestData))]
        public void Event_ValidEventUsingWith_SendsEvent(string eventName, string payload)
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler());
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                if (payload == null)
                    r.Event(eventName);
                else
                    r.Event(eventName, JToken.Parse(payload));
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            if (payload == null)
            {
                Conn.GetMsg()
                    .AssertSubject("event.test.model." + eventName)
                    .AssertNoPayload();
            }
            else
            {
                Conn.GetMsg()
                    .AssertSubject("event.test.model." + eventName)
                    .AssertPayload(JToken.Parse(payload));
            }
        }

        [Theory]
        [InlineData("change")]
        [InlineData("delete")]
        [InlineData("add")]
        [InlineData("remove")]
        [InlineData("patch")]
        [InlineData("reaccess")]
        [InlineData("unsubscribe")]
        [InlineData("foo.bar")]
        [InlineData("foo.>")]
        [InlineData("*")]
        [InlineData("*.bar")]
        [InlineData("?foo")]
        [InlineData("foo?")]
        [InlineData(">.baz")]
        public void Event_InvalidEventName_ThrowsArgumentException(string eventName)
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler());
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                Assert.Throws<ArgumentException>(() => r.Event(eventName));
                r.Event("valid");
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg().AssertSubject("event.test.model.valid");
        }
        #endregion

        #region ChangeEvent
        public static IEnumerable<object[]> GetChangeEventTestData()
        {
            yield return new object[] { new Dictionary<string, object> { { "foo", 42 } } };
            yield return new object[] { new Dictionary<string, object> { { "foo", "bar" } } };
            yield return new object[] { new Dictionary<string, object> { { "foo", null } } };
            yield return new object[] { new Dictionary<string, object> { { "foo", 12 }, { "bar", true } } };
            yield return new object[] { new Dictionary<string, object> { { "foo", "bar" }, { "deleted", ResAction.Delete } } };
            yield return new object[] { new Dictionary<string, object> { { "foo", new Ref("test.model.bar") } } };
        }

        [Theory]
        [MemberData(nameof(GetChangeEventTestData))]
        public void ChangeEvent_ValidEvent_SendsChangeEvent(Dictionary<string, object> changed)
        {
            Service.AddHandler("model", new DynamicHandler().SetCall(r =>
            {
                r.ChangeEvent(changed);
                r.Ok();
            }));
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
        public void ChangeEvent_ValidEventUsingWith_SendsChangeEvent(Dictionary<string, object> changed)
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler());
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                r.ChangeEvent(changed);
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg()
                .AssertSubject("event.test.model.change")
                .AssertPayload(new { values = changed });
        }

        [Fact]
        public void ChangeEvent_NoChanges_NoChangeEventSent()
        {
            Service.AddHandler("model", new DynamicHandler().SetCall(r =>
            {
                r.ChangeEvent(new Dictionary<string, object>());
                r.ChangeEvent(null);
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Fact]
        public void ChangeEvent_NoChangesUsingWith_NoChangeEventSent()
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler());
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                r.ChangeEvent(new Dictionary<string, object>());
                r.ChangeEvent(null);
                r.Event("foo");
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg().AssertSubject("event.test.model.foo");
        }

        [Fact]
        public void ChangeEvent_OnCollection_ThrowsInvalidOperationException()
        {
            Service.AddHandler("collection", new DynamicHandler()
                .SetType(ResourceType.Collection)
                .SetCall(r =>
                {
                    Assert.Throws<InvalidOperationException>(() => r.ChangeEvent(new Dictionary<string, object>()));
                    r.Ok();
                }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.collection.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Fact]
        public void ChangeEvent_OnCollectionUsingWith_ThrowsInvalidOperationException()
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("collection", new DynamicHandler().SetType(ResourceType.Collection));
            Service.Serve(Conn);
            Service.With("test.collection", r =>
            {
                Assert.Throws<InvalidOperationException>(() => r.ChangeEvent(new Dictionary<string, object>()));
                r.Event("foo");
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg().AssertSubject("event.test.collection.foo");
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
        public void AddEvent_ValidEvent_SendsAddEvent(object value, int idx, object expected)
        {
            Service.AddHandler("model", new DynamicHandler().SetCall(r =>
            {
                r.AddEvent(value, idx);
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject("event.test.model.add")
                .AssertPayload(expected);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Theory]
        [MemberData(nameof(GetAddEventTestData))]
        public void AddEvent_ValidEventUsingWith_SendsAddEvent(object value, int idx, object expected)
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler());
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                r.AddEvent(value, idx);
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg()
                .AssertSubject("event.test.model.add")
                .AssertPayload(expected);
        }

        [Fact]
        public void AddEvent_NegativeIdx_ThrowsArgumentException()
        {
            Service.AddHandler("model", new DynamicHandler().SetCall(r =>
            {
                Assert.Throws<ArgumentException>(() => r.AddEvent("foo", -1));
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Fact]
        public void AddEvent_NegativeIdxUsingWith_ThrowsArgumentException()
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler());
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                Assert.Throws<ArgumentException>(() => r.AddEvent("foo", -1));
                r.Event("foo");
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg().AssertSubject("event.test.model.foo");
        }

        [Fact]
        public void AddEvent_OnModel_ThrowsInvalidOperationException()
        {
            Service.AddHandler("model", new DynamicHandler()
                .SetType(ResourceType.Model)
                .SetCall(r =>
                {
                    Assert.Throws<InvalidOperationException>(() => r.AddEvent("foo", 0));
                    r.Ok();
                }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Fact]
        public void AddEvent_OnModelUsingWith_ThrowsInvalidOperationException()
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler().SetType(ResourceType.Model));
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                Assert.Throws<InvalidOperationException>(() => r.AddEvent("foo", 0));
                r.Event("foo");
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg().AssertSubject("event.test.model.foo");
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
        public void RemoveEvent_ValidEvent_SendsRemoveEvent(int idx, object expected)
        {
            Service.AddHandler("model", new DynamicHandler().SetCall(r =>
            {
                r.RemoveEvent(idx);
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject("event.test.model.remove")
                .AssertPayload(expected);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Theory]
        [MemberData(nameof(GetRemoveEventTestData))]
        public void RemoveEvent_ValidEventUsingWith_SendsRemoveEvent(int idx, object expected)
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler());
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                r.RemoveEvent(idx);
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg()
                .AssertSubject("event.test.model.remove")
                .AssertPayload(expected);
        }

        [Fact]
        public void RemoveEvent_NegativeIdx_ThrowsArgumentException()
        {
            Service.AddHandler("model", new DynamicHandler().SetCall(r =>
            {
                Assert.Throws<ArgumentException>(() => r.RemoveEvent(-1));
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Fact]
        public void RemoveEvent_NegativeIdxUsingWith_ThrowsArgumentException()
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler());
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                Assert.Throws<ArgumentException>(() => r.RemoveEvent(-1));
                r.Event("foo");
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg().AssertSubject("event.test.model.foo");
        }

        [Fact]
        public void RemoveEvent_OnModel_ThrowsInvalidOperationException()
        {
            Service.AddHandler("model", new DynamicHandler()
                .SetType(ResourceType.Model)
                .SetCall(r =>
                {
                    Assert.Throws<InvalidOperationException>(() => r.RemoveEvent(0));
                    r.Ok();
                }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Fact]
        public void RemoveEvent_OnModelUsingWith_ThrowsInvalidOperationException()
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler().SetType(ResourceType.Model));
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                Assert.Throws<InvalidOperationException>(() => r.RemoveEvent(0));
                r.Event("foo");
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg().AssertSubject("event.test.model.foo");
        }
        #endregion

        #region ReaccessEvent
        [Fact]
        public void ReaccessEvent_UsingRequest_SendsReaccessEvent()
        {
            Service.AddHandler("model", new DynamicHandler().SetCall(r =>
            {
                r.ReaccessEvent();
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject("event.test.model.reaccess")
                .AssertNoPayload();
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Fact]
        public void ReaccessEvent_UsingWith_SendsReaccessEvent()
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler());
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                r.ReaccessEvent();
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg()
                .AssertSubject("event.test.model.reaccess")
                .AssertNoPayload();
        }
        #endregion

        #region ResetEvent
        [Fact]
        public void ResetEvent_UsingRequest_SendsSystemResetEvent()
        {
            Service.AddHandler("model", new DynamicHandler().SetCall(r =>
            {
                r.ResetEvent();
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject("system.reset")
                .AssertPayload(new { resources = new[] { "test.model" } });
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Fact]
        public void ResetEvent_UsingWith_SendsSystemResetEvent()
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler());
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                r.ResetEvent();
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg()
                .AssertSubject("system.reset")
                .AssertPayload(new { resources = new[] { "test.model" } });
        }
        #endregion

        #region CreateEvent
        [Fact]
        public void CreateEvent_UsingRequest_SendsCreateEvent()
        {
            Service.AddHandler("model", new DynamicHandler().SetCall(r =>
            {
                r.CreateEvent(Test.Model);
                r.Ok();
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
        public void CreateEvent_UsingWith_SendsCreateEvent()
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler());
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                r.CreateEvent(Test.Model);
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg()
                .AssertSubject("event.test.model.create")
                .AssertNoPayload();
        }
        #endregion

        #region DeleteEvent
        [Fact]
        public void DeleteEvent_UsingRequest_SendsDeleteEvent()
        {
            Service.AddHandler("model", new DynamicHandler().SetCall(r =>
            {
                r.DeleteEvent();
                r.Ok();
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
        public void DeleteEvent_UsingWith_SendsDeleteEvent()
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler());
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                r.DeleteEvent();
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg()
                .AssertSubject("event.test.model.delete")
                .AssertNoPayload();
        }
        #endregion
    }
}
