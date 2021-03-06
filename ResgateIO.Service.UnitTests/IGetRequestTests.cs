using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ResgateIO.Service.UnitTests
{
    public class IGetRequestTests : TestsBase
    {
        public IGetRequestTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Model_WithPrimitiveModel_SendsModelResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Get(r => r.Model(Test.Model)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("get.test.model", Test.EmptyRequest);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(new { model = Test.Model });
        }

        [Fact]
        public void Collection_WithPrimitiveCollection_SendsCollectionResponse()
        {
            Service.AddHandler("collection", new DynamicHandler().Get(r => r.Collection(Test.Collection)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("get.test.collection", Test.EmptyRequest);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(new { collection = Test.Collection });
        }

        [Fact]
        public void Error_SingleResponse_SendsErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Get(r => r.Error(Test.CustomError)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("get.test.model", Test.EmptyRequest);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(Test.CustomError);
        }

        [Fact]
        public void NotFound_SingleResponse_SendsNotFoundErrorResponse()
        {
            bool called = false;
            Service.AddHandler("model", new DynamicHandler().Get(r =>
            {
                called = true;
                r.NotFound();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("get.test.model", Test.EmptyRequest);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.NotFound);
            Assert.True(called);
        }

        [Fact]
        public void InvalidQuery_WithoutMessage_SendsInvalidQueryErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Get(r => r.InvalidQuery()));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("get.test.model", Test.EmptyRequest);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.InvalidQuery);
        }

        [Fact]
        public void InvalidQuery_WithMessage_SendsInvalidQueryErrorWithMessageResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Get(r => r.InvalidQuery(Test.ErrorMessage)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("get.test.model", Test.EmptyRequest);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInvalidQuery, Test.ErrorMessage);
        }

        [Fact]
        public void InvalidQuery_WithMessageAndData_SendsInvalidQueryErrorWithMessageAndDataResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Get(r => r.InvalidQuery(Test.ErrorMessage, Test.ErrorData)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("get.test.model", Test.EmptyRequest);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInvalidQuery, Test.ErrorMessage, Test.ErrorData);
        }

        [Fact]
        public void Timeout_WithMilliseconds_SendsPreresponse()
        {
            Service.AddHandler("model", new DynamicHandler().Get(r =>
            {
                r.Timeout(3000);
                r.NotFound();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("get.test.model", Test.EmptyRequest);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertPayload(Encoding.UTF8.GetBytes("timeout:\"3000\""));
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.NotFound);
        }

        [Fact]
        public void Timeout_WithTimespan_SendsPreresponse()
        {
            Service.AddHandler("model", new DynamicHandler().Get(r =>
            {
                r.Timeout(new TimeSpan(0, 0, 4));
                r.NotFound();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("get.test.model", null);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertPayload(Encoding.UTF8.GetBytes("timeout:\"4000\""));
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.NotFound);
        }

        [Fact]
        public void Timeout_WithNegativeDuration_ThrowsException()
        {
            Service.AddHandler("model", new DynamicHandler().Get(r =>
            {
                r.Timeout(-1);
                r.Model(Test.Model);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("get.test.model", Test.EmptyRequest);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInternalError);
        }

        [Fact]
        public void GetRequest_ThrownException_SendsInternalErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler()
                .Get(r => throw new Exception(Test.ErrorMessage)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("get.test.model", null);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInternalError, Test.ErrorMessage);
        }

        [Fact]
        public void GetRequest_ThrownResException_SendsErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler()
                .Get(r => throw new ResException(Test.CustomError)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("get.test.model", null);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(Test.CustomError);
        }

        [Fact]
        public void GetRequest_MultipleRequests_RespondedInOrder()
        {
            const int requestCount = 100;
            Service.AddHandler("model", new DynamicHandler().Get(r => r.Error(Test.CustomError)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");

            string[] inboxes = new string[requestCount];
            for (int i = 0; i < requestCount; i++)
            {
                inboxes[i] = Conn.NATSRequest("get.test.model", Test.EmptyRequest);
            }
            for (int i = 0; i < requestCount; i++)
            {
                Conn.GetMsg()
                    .AssertSubject(inboxes[i])
                    .AssertError(Test.CustomError);
            }
        }

        [Fact]
        public void GetRequest_MultipleRequestsOnDifferentResources_RespondedInOrderWithinResource()
        {
            const int requestCount = 50;
            Service.AddHandler("model", new DynamicHandler().Get(r => r.Model(Test.Model)));
            Service.AddHandler("collection", new DynamicHandler().Get(r => r.Collection(Test.Collection)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");

            string[] mInboxes = new string[requestCount];
            string[] cInboxes = new string[requestCount];
            for (int i = 0; i < requestCount; i++)
            {
                mInboxes[i] = Conn.NATSRequest("get.test.model", Test.EmptyRequest);
                cInboxes[i] = Conn.NATSRequest("get.test.collection", Test.EmptyRequest);
            }
            int mi = 0;
            int ci = 0;
            for (int i = 0; i < requestCount * 2; i++)
            {
                var msg = Conn.GetMsg();
                if (mi < requestCount && mInboxes[mi] == msg.Subject)
                {
                    msg.AssertResult(new { model = Test.Model });
                    mi++;
                }
                else if (ci< requestCount && cInboxes[ci] == msg.Subject)
                {
                    msg.AssertResult(new { collection = Test.Collection });
                    ci++;
                }
                else
                {
                    throw new Exception("Expected message subject to be a for a collection or model request, but got: " + msg.Subject);
                }
            }
        }

        [Fact]
        public void GetRequest_MultipleRequestsSameGroup_RespondedInOrder()
        {
            const int requestCount = 50;
            Service.AddHandler("model.a", "model", new DynamicHandler().Get(r => r.Error(Test.CustomError)));
            Service.AddHandler("model.b", "model", new DynamicHandler().Get(r => r.Error(Test.CustomError)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");

            string[] inboxes = new string[requestCount * 2];
            for (int i = 0; i < requestCount; i++)
            {
                inboxes[i * 2] = Conn.NATSRequest("get.test.model.a", Test.EmptyRequest);
                inboxes[i * 2 + 1] = Conn.NATSRequest("get.test.model.b", Test.EmptyRequest);
            }
            for (int i = 0; i < requestCount * 2; i++)
            {
                Conn.GetMsg()
                    .AssertSubject(inboxes[i])
                    .AssertError(Test.CustomError);
            }
        }

        [Fact]
        public void GetRequest_MultipleModelCalls_SendsSingleModelResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Get(r =>
            {
                r.Model(Test.Model);
                r.Model(Test.Model);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("get.test.model", null);
            Conn.GetMsg().AssertSubject(inbox);
            string inbox2 = Conn.NATSRequest("get.test.model", null);
            Conn.GetMsg().AssertSubject(inbox2);
        }

        [Fact]
        public void ModelQuery_WithQueryModel_SendsQueryModelResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Get(r => r.Model(Test.Model, Test.NormalizedQuery)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("get.test.model", Test.QueryRequest);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(new { model = Test.Model, query = Test.NormalizedQuery });
        }

        [Fact]
        public void CollectionQuery_WithQueryCollection_SendsQueryCollectionResponse()
        {
            Service.AddHandler("collection", new DynamicHandler().Get(r => r.Collection(Test.Collection, Test.NormalizedQuery)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("get.test.collection", Test.QueryRequest);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(new { collection = Test.Collection, query = Test.NormalizedQuery });
        }

        [Fact]
        public void ModelQuery_WithoutQueryModel_SendsQueryModelResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Get(r => r.Model(Test.Model, Test.NormalizedQuery)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("get.test.model", Test.EmptyRequest);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(new { model = Test.Model, query = Test.NormalizedQuery });
        }

        [Fact]
        public void CollectionQuery_WithoutQueryCollection_SendsQueryCollectionResponse()
        {
            Service.AddHandler("collection", new DynamicHandler().Get(r => r.Collection(Test.Collection, Test.NormalizedQuery)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("get.test.collection", Test.EmptyRequest);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(new { collection = Test.Collection, query = Test.NormalizedQuery });
        }
    }
}
