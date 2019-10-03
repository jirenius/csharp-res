using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ResgateIO.Service.UnitTests
{
    public class INewRequestTests : TestsBase
    {
        public INewRequestTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Properties_ReturnsCorrectValue()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r =>
            {
                Assert.Equal(Test.CID, r.CID);
                Assert.Equal(Test.TokenId, (int)r.Token["id"]);
                Assert.Equal(Test.TokenRole, (string)r.Token["role"]);
                Assert.Equal(Test.ParamNumber, (int)r.Params["number"]);
                Assert.Equal(Test.ParamText, (string)r.Params["text"]);
                r.New(Test.NewRef);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.RequestWithParams);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.NewRef);
        }


        [Fact]
        public void PropertiesTokenAndParams_WithNoTokenOrParams_ReturnsNull()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r =>
            {
                Assert.Equal(Test.CID, r.CID);
                Assert.Null(r.Token);
                Assert.Null(r.Params);
                r.New(Test.NewRef);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.RequestWithoutToken);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.NewRef);
        }

        [Fact]
        public void New_WithNull_SendsInternalErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r => r.New(null)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInternalError);
        }

        [Fact]
        public void New_WithRef_SendsRefInResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r => r.New(Test.NewRef)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.NewRef);
        }

        [Fact]
        public void New_WithInvalidRef_SendsInternalErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r => r.New(new Ref("*"))));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInternalError);
        }

        [Fact]
        public void Error_SendsErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r => r.Error(Test.CustomError)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(Test.CustomError);
        }

        [Fact]
        public void NotFound_SendsNotFoundErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r => r.NotFound()));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.NotFound);
        }

        [Fact]
        public void MethodNotFound_SendsMethodNotFoundErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r => r.MethodNotFound()));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.MethodNotFound);
        }

        [Fact]
        public void InvalidParams_WithoutMessage_SendsInvalidParamsErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r => r.InvalidParams()));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.RequestWithParams);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.InvalidParams);
        }

        [Fact]
        public void InvalidParams_WithMessage_SendsInvalidParamsErrorWithMessageResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r => r.InvalidParams(Test.ErrorMessage)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.RequestWithParams);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInvalidParams, Test.ErrorMessage);
        }

        [Fact]
        public void InvalidParams_WithMessageAndData_SendsInvalidParamsErrorWithMessageAndDataResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r => r.InvalidParams(Test.ErrorMessage, Test.ErrorData)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.RequestWithParams);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInvalidParams, Test.ErrorMessage, Test.ErrorData);
        }

        [Fact]
        public void InvalidQuery_WithoutMessage_SendsInvalidQueryErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r => r.InvalidQuery()));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.RequestWithParams);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.InvalidQuery);
        }

        [Fact]
        public void InvalidQuery_WithMessage_SendsInvalidQueryErrorWithMessageResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r => r.InvalidQuery(Test.ErrorMessage)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.RequestWithParams);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInvalidQuery, Test.ErrorMessage);
        }

        [Fact]
        public void InvalidQuery_WithMessageAndData_SendsInvalidQueryErrorWithMessageAndDataResponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r => r.InvalidQuery(Test.ErrorMessage, Test.ErrorData)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.RequestWithParams);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInvalidQuery, Test.ErrorMessage, Test.ErrorData);
        }

        [Fact]
        public void ParseParams_WithParams_ReturnsParsedParams()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r =>
            {
                ParamsDto p = r.ParseParams<ParamsDto>();
                Assert.Equal(Test.ParamNumber, p.Number);
                Assert.Equal(Test.ParamText, p.Text);
                r.New(Test.NewRef);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.RequestWithParams);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.NewRef);
        }

        [Fact]
        public void ParseParams_WithoutParams_ReturnsNull()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r =>
            {
                Assert.Null(r.ParseParams<ParamsDto>());
                r.New(Test.NewRef);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.NewRef);
        }

        [Fact]
        public void ParseToken_WithToken_ReturnsParsedToken()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r =>
            {
                TokenDto p = r.ParseToken<TokenDto>();
                Assert.Equal(Test.TokenId, p.Id);
                Assert.Equal(Test.TokenRole, p.Role);
                r.New(Test.NewRef);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.NewRef);
        }

        [Fact]
        public void ParseToken_WithoutToken_ReturnsNull()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r =>
            {
                Assert.Null(r.ParseToken<TokenDto>());
                r.New(Test.NewRef);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.RequestWithoutToken);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.NewRef);
        }

        [Fact]
        public void Timeout_WithMilliseconds_SendsPreresponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r =>
            {
                r.Timeout(3000);
                r.New(Test.NewRef);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertPayload(Encoding.UTF8.GetBytes("timeout:\"3000\""));
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.NewRef);
        }

        [Fact]
        public void Timeout_WithTimespan_SendsPreresponse()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r =>
            {
                r.Timeout(new TimeSpan(0, 0, 4));
                r.New(Test.NewRef);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertPayload(Encoding.UTF8.GetBytes("timeout:\"4000\""));
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.NewRef);
        }

        [Fact]
        public void Timeout_WithNegativeDuration_ThrowsException()
        {
            Service.AddHandler("model", new DynamicHandler().SetNew(r =>
            {
                r.Timeout(-1);
                r.New(Test.NewRef);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.test.model.new", Test.EmptyRequest);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInternalError);
        }
    }
}
