using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ResgateIO.Service.UnitTests
{
    public class QueryEventTests : TestsBase
    {
        public QueryEventTests(ITestOutputHelper output) : base(output) { }

        #region QueryEvent
        [Fact]
        public void QueryEvent_UsingRequest_SendsQueryEvent()
        {
            Service.AddHandler("model", new DynamicHandler().Call(r =>
            {
                r.QueryEvent(qr => { });
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Assert.True(Conn.GetMsg()
                .AssertSubject("event.test.model.query")
                .TryGetPath("subject", out JToken subject), "no subject property in query event");
            Assert.Equal(JTokenType.String, subject.Type);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Fact]
        public void QueryEvent_UsingWith_SendsQueryEvent()
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler());
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                r.QueryEvent(qr => { });
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Assert.True(Conn.GetMsg()
                .AssertSubject("event.test.model.query")
                .TryGetPath("subject", out JToken subject), "no subject property in query event");
            Assert.Equal(JTokenType.String, subject.Type);
        }

        [Fact]
        public void QueryEvent_ExpiredTimeout_CallsCallbackWithNull()
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler());
            Service.SetQueryDuration(TimeSpan.FromMilliseconds(1));
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                r.QueryEvent(qr => {
                    Assert.Null(qr);
                    ev.Set();
                });
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg().AssertSubject("event.test.model.query");
        }

        [Fact]
        public void QueryEvent_FailedSubscription_CallsCallbackWithNull()
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler());
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                Conn.FailNextSubscription();
                r.QueryEvent(qr => {
                    Assert.Null(qr);
                    ev.Set();
                });
                r.Event("foo");
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Conn.GetMsg().AssertSubject("event.test.model.foo");
        }
        #endregion

        #region QueryRequest
        public static IEnumerable<object[]> GetQueryRequestTestData()
        {
            yield return new object[] {
                "foo=none",
                (Action<IQueryRequest>)(r => Assert.Equal("foo=none", r.Query)),
                JToken.Parse("{\"events\":[]}")
            };
            yield return new object[] {
                "foo=change",
                (Action<IQueryRequest>)(r => r?.ChangeEvent(new Dictionary<string, object>{ { "foo", "bar" } })),
                JToken.Parse("{\"events\":[{\"event\":\"change\",\"data\":{\"values\":{\"foo\":\"bar\"}}}]}")
            };
            yield return new object[] {
                "foo=change_with_empty",
                (Action<IQueryRequest>)(r => r?.ChangeEvent(new Dictionary<string, object>{ })),
                JToken.Parse("{\"events\":[]}")
            };
            yield return new object[] {
                "foo=add",
                (Action<IQueryRequest>)(r => r?.AddEvent("bar", 2)),
                JToken.Parse("{\"events\":[{\"event\":\"add\",\"data\":{\"value\":\"bar\",\"idx\":2}}]}")
            };
            yield return new object[] {
                "foo=invalid_add",
                (Action<IQueryRequest>)(r => r?.AddEvent("bar", -1)),
                ResError.CodeInternalError
            };
            yield return new object[] {
                "foo=remove",
                (Action<IQueryRequest>)(r => r?.RemoveEvent(3)),
                JToken.Parse("{\"events\":[{\"event\":\"remove\",\"data\":{\"idx\":3}}]}")
            };
            yield return new object[] {
                "foo=invalid_remove",
                (Action<IQueryRequest>)(r => r?.RemoveEvent(-1)),
                ResError.CodeInternalError
            };
            yield return new object[] {
                "foo=remove_add",
                (Action<IQueryRequest>)(r => { r?.RemoveEvent(3); r?.AddEvent("bar", 2); }),
                JToken.Parse("{\"events\":[{\"event\":\"remove\",\"data\":{\"idx\":3}},{\"event\":\"add\",\"data\":{\"value\":\"bar\",\"idx\":2}}]}")
            };
            yield return new object[] {
                "foo=remove_add",
                (Action<IQueryRequest>)(r => { r?.AddEvent("bar", 2); r?.RemoveEvent(3); }),
                JToken.Parse("{\"events\":[{\"event\":\"add\",\"data\":{\"value\":\"bar\",\"idx\":2}},{\"event\":\"remove\",\"data\":{\"idx\":3}}]}")
            };
            yield return new object[] {
                "foo=notFound",
                (Action<IQueryRequest>)(r => r?.NotFound()),
                ResError.NotFound
            };
            yield return new object[] {
                "foo=invalidQuery",
                (Action<IQueryRequest>)(r => r?.InvalidQuery()),
                ResError.InvalidQuery
            };
            yield return new object[] {
                "foo=invalidQuery_with_message",
                (Action<IQueryRequest>)(r => r?.InvalidQuery(Test.ErrorMessage)),
                new ResError(ResError.CodeInvalidQuery, Test.ErrorMessage)
            };
            yield return new object[] {
                "foo=invalidQuery_with_message_and_data",
                (Action<IQueryRequest>)(r => r?.InvalidQuery(Test.ErrorMessage, Test.ErrorData)),
                new ResError(ResError.CodeInvalidQuery, Test.ErrorMessage, Test.ErrorData)
            };
            yield return new object[] {
                "foo=error",
                (Action<IQueryRequest>)(r => r?.Error(Test.CustomError)),
                Test.CustomError
            };
            yield return new object[] {
                "foo=resException",
                (Action<IQueryRequest>)(r => { if (r != null) throw new ResException(Test.CustomError); }),
                Test.CustomError
            };
            yield return new object[] {
                "foo=resException",
                (Action<IQueryRequest>)(r => { if (r != null) throw new Exception(Test.ErrorMessage); }),
                ResError.CodeInternalError
            };
            yield return new object[] {
                "foo=error_after_event",
                (Action<IQueryRequest>)(r =>
                {
                    if (r != null)
                    {
                        r.ChangeEvent(new Dictionary<string, object>{ { "foo", "bar" } });
                        r.Error(Test.CustomError);
                    }
                }),
                Test.CustomError
            };
            yield return new object[] {
                "foo=exception_after_event",
                (Action<IQueryRequest>)(r =>
                {
                    if (r != null)
                    {
                        r.ChangeEvent(new Dictionary<string, object>{ { "foo", "bar" } });
                        throw new ResException(Test.CustomError);
                    }
                }),
                Test.CustomError
            };
            yield return new object[] {
                "foo=multiple_error",
                (Action<IQueryRequest>)(r =>
                {
                    if (r != null)
                    {
                        r.NotFound();
                        r.Error(Test.CustomError);
                    }
                }),
                ResError.NotFound
            };
            yield return new object[] {
                "foo=error_and_exception",
                (Action<IQueryRequest>)(r =>
                {
                    if (r != null)
                    {
                        r.NotFound();
                        throw new ResException(Test.CustomError);
                    }
                }),
                ResError.NotFound
            };
        }

        [Theory]
        [MemberData(nameof(GetQueryRequestTestData))]
        public void QueryRequest_UsingRequest_SendsCorrectResponse(string query, Action<IQueryRequest> callback, object expected)
        {
            Service.AddHandler("model", new DynamicHandler().Call(r =>
            {
                r.QueryEvent(callback);
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", new RequestDto { CID = Test.CID, Token = Test.Token, Params = new { query } });
            Assert.True(Conn.GetMsg()
                .AssertSubject("event.test.model.query")
                .TryGetPath("subject", out JToken subject), "no subject property in query event");
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
            string inboxQR = Conn.NATSRequest((string)subject, new { query });
            var msg = Conn.GetMsg().AssertSubject(inboxQR);
            if (expected is string expectedString)
            {
                msg.AssertError(expectedString);
            }
            else if (expected is ResError expectedError)
            {
                msg.AssertError(expectedError);
            }
            else
            {
                msg.AssertResult(expected);
            }
        }

        [Theory]
        [MemberData(nameof(GetQueryRequestTestData))]
        public void QueryRequest_UsingWith_SendsCorrectResponse(string query, Action<IQueryRequest> callback, object expected)
        {
            AutoResetEvent ev = new AutoResetEvent(false);
            Service.AddHandler("model", new DynamicHandler());
            Service.Serve(Conn);
            Service.With("test.model", r =>
            {
                r.QueryEvent(callback);
                ev.Set();
            });
            Assert.True(ev.WaitOne(Test.TimeoutDuration), "callback was not called before timeout");
            Assert.True(Conn.GetMsg()
                .AssertSubject("event.test.model.query")
                .TryGetPath("subject", out JToken subject), "no subject property in query event");
            string inboxQR = Conn.NATSRequest((string)subject, new { query });
            var msg = Conn.GetMsg().AssertSubject(inboxQR);
            if (expected is string expectedString)
            {
                msg.AssertError(expectedString);
            }
            else if (expected is ResError expectedError)
            {
                msg.AssertError(expectedError);
            }
            else
            {
                msg.AssertResult(expected);
            }
        }

        [Fact]
        public void QueryRequest_CallingTimeoutWithTimespan_SendsPreresponse()
        {
            Service.AddHandler("model", new DynamicHandler().Call(r =>
            {
                r.QueryEvent(qr => {
                    qr.Timeout(TimeSpan.FromSeconds(42));
                });
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Assert.True(Conn.GetMsg()
                .AssertSubject("event.test.model.query")
                .TryGetPath("subject", out JToken subject), "no subject property in query event");
            string inboxQR = Conn.NATSRequest((string)subject, new { query = "foo=bar" });
            Conn.GetMsg().AssertSubject(inbox).AssertResult(null);
            Conn.GetMsg().AssertSubject(inboxQR).AssertPayload(Encoding.UTF8.GetBytes("timeout:\"42000\""));
            Conn.GetMsg().AssertSubject(inboxQR).AssertResult(JToken.Parse("{\"events\":[]}"));
        }

        [Fact]
        public void QueryRequest_CallingTimeoutWithMilliseconds_SendsPreresponse()
        {
            Service.AddHandler("model", new DynamicHandler().Call(r =>
            {
                r.QueryEvent(qr => {
                    qr.Timeout(42000);
                });
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Assert.True(Conn.GetMsg()
                .AssertSubject("event.test.model.query")
                .TryGetPath("subject", out JToken subject), "no subject property in query event");
            string inboxQR = Conn.NATSRequest((string)subject, new { query = "foo=bar" });
            Conn.GetMsg().AssertSubject(inbox).AssertResult(null);
            Conn.GetMsg().AssertSubject(inboxQR).AssertPayload(Encoding.UTF8.GetBytes("timeout:\"42000\""));
            Conn.GetMsg().AssertSubject(inboxQR).AssertResult(JToken.Parse("{\"events\":[]}"));
        }

        [Fact]
        public void QueryRequest_CallingTimeoutWithInvalidDuration_SendsInternalErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Call(r =>
            {
                r.QueryEvent(qr => {
                    qr.Timeout(TimeSpan.FromSeconds(-1));
                });
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.method", Test.Request);
            Assert.True(Conn.GetMsg()
                .AssertSubject("event.test.model.query")
                .TryGetPath("subject", out JToken subject), "no subject property in query event");
            string inboxQR = Conn.NATSRequest((string)subject, new { query = "foo=bar" });
            Conn.GetMsg().AssertSubject(inbox).AssertResult(null);
            Conn.GetMsg().AssertSubject(inboxQR).AssertError(ResError.CodeInternalError);
        }
        #endregion
    }
}
