using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace ResgateIO.Service.UnitTests
{
    public class IAuthRequestTests : TestsBase
    {
        public IAuthRequestTests(ITestOutputHelper output) : base(output) { }

        [Fact]
        public void Properties_ReturnsCorrectValue()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r =>
            {
                Assert.Equal("method", r.Method);
                Assert.Equal(Test.CID, r.CID);
                Assert.Equal(Test.TokenId, (int)r.Token["id"]);
                Assert.Equal(Test.TokenRole, (string)r.Token["role"]);
                Assert.Equal(Test.ParamNumber, (int)r.Params["number"]);
                Assert.Equal(Test.ParamText, (string)r.Params["text"]);
                Test.AssertJsonEqual(Test.Header, r.Header);
                Assert.Equal(Test.Host, r.Host);
                Assert.Equal(Test.RemoteAddr, r.RemoteAddr);
                Assert.Equal(Test.URI, r.URI);
                r.Ok(Test.Result);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.AuthRequest);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.Result);
        }


        [Fact]
        public void PropertiesTokenAndParams_WithNoTokenOrParams_ReturnsNull()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r =>
            {
                Assert.Equal("method", r.Method);
                Assert.Equal(Test.CID, r.CID);
                Assert.Null(r.Token);
                Assert.Null(r.Params);
                r.Ok(Test.Result);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.RequestWithoutToken);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.Result);
        }

        [Fact]
        public void Ok_WithNoResult_SendsNullResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r => r.Ok()));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Fact]
        public void Ok_WithResult_SendsResultInResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r => r.Ok(Test.Result)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.Result);
        }

        [Fact]
        public void Ok_WithNullResult_SendsNullResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r => r.Ok(null)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(null);
        }

        [Fact]
        public void Resource_WithValidResourceID_SendsResourceResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r => r.Resource("test.model")));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResource("test.model");
        }

        [Fact]
        public void Resource_WithInvalidResourceID_ThrowsArgumentException()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r => r.Resource("test..model")));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInternalError);
        }

        [Fact]
        public void Error_SendsErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r => r.Error(Test.CustomError)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(Test.CustomError);
        }

        [Fact]
        public void NotFound_SendsNotFoundErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r => r.NotFound()));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.NotFound);
        }

        [Fact]
        public void MethodNotFound_SendsMethodNotFoundErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r => r.MethodNotFound()));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.MethodNotFound);
        }

        [Fact]
        public void InvalidParams_WithoutMessage_SendsInvalidParamsErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r => r.InvalidParams()));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.RequestWithParams);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.InvalidParams);
        }

        [Fact]
        public void InvalidParams_WithMessage_SendsInvalidParamsErrorWithMessageResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r => r.InvalidParams(Test.ErrorMessage)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.RequestWithParams);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInvalidParams, Test.ErrorMessage);
        }

        [Fact]
        public void InvalidParams_WithMessageAndData_SendsInvalidParamsErrorWithMessageAndDataResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r => r.InvalidParams(Test.ErrorMessage, Test.ErrorData)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.RequestWithParams);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInvalidParams, Test.ErrorMessage, Test.ErrorData);
        }

        [Fact]
        public void InvalidQuery_WithoutMessage_SendsInvalidQueryErrorResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r => r.InvalidQuery()));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.RequestWithParams);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.InvalidQuery);
        }

        [Fact]
        public void InvalidQuery_WithMessage_SendsInvalidQueryErrorWithMessageResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r => r.InvalidQuery(Test.ErrorMessage)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.RequestWithParams);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInvalidQuery, Test.ErrorMessage);
        }

        [Fact]
        public void InvalidQuery_WithMessageAndData_SendsInvalidQueryErrorWithMessageAndDataResponse()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r => r.InvalidQuery(Test.ErrorMessage, Test.ErrorData)));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.RequestWithParams);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInvalidQuery, Test.ErrorMessage, Test.ErrorData);
        }

        [Fact]
        public void ParseParams_WithParams_ReturnsParsedParams()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r =>
            {
                ParamsDto p = r.ParseParams<ParamsDto>();
                Assert.Equal(Test.ParamNumber, p.Number);
                Assert.Equal(Test.ParamText, p.Text);
                r.Ok(Test.Result);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.RequestWithParams);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.Result);
        }

        [Fact]
        public void ParseParams_WithoutParams_ReturnsNull()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r =>
            {
                Assert.Null(r.ParseParams<ParamsDto>());
                r.Ok(Test.Result);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.Result);
        }

        [Fact]
        public void ParseToken_WithToken_ReturnsParsedToken()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r =>
            {
                TokenDto p = r.ParseToken<TokenDto>();
                Assert.Equal(Test.TokenId, p.Id);
                Assert.Equal(Test.TokenRole, p.Role);
                r.Ok(Test.Result);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.Result);
        }

        [Fact]
        public void ParseToken_WithoutToken_ReturnsNull()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r =>
            {
                Assert.Null(r.ParseToken<TokenDto>());
                r.Ok(Test.Result);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.RequestWithoutToken);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.Result);
        }

        [Fact]
        public void TokenEvent_WithToken_SendsTokenEvent()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r =>
            {
                r.TokenEvent(Test.Token);
                r.Ok(Test.Result);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject("conn." + Test.CID + ".token")
                .AssertPayload(new { token = Test.Token });
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.Result);
        }

        [Fact]
        public void TokenEvent_WithNullToken_SendsNullTokenEvent()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r =>
            {
                r.TokenEvent(null);
                r.Ok(Test.Result);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject("conn." + Test.CID + ".token")
                .AssertPayload(new { token = (object)null });
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.Result);
        }

        [Fact]
        public void TokenEvent_WithTokenID_SendsTokenEventWithID()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r =>
            {
                r.TokenEvent(Test.Token, Test.TID);
                r.Ok(Test.Result);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject("conn." + Test.CID + ".token")
                .AssertPayload(new { token = Test.Token, tid = Test.TID });
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.Result);
        }

        [Fact]
        public void Timeout_WithMilliseconds_SendsPreresponse()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r =>
            {
                r.Timeout(3000);
                r.Ok(Test.Result);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertPayload(Encoding.UTF8.GetBytes("timeout:\"3000\""));
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.Result);
        }

        [Fact]
        public void Timeout_WithTimespan_SendsPreresponse()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r =>
            {
                r.Timeout(new TimeSpan(0, 0, 4));
                r.Ok(Test.Result);
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.Request);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertPayload(Encoding.UTF8.GetBytes("timeout:\"4000\""));
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(Test.Result);
        }

        [Fact]
        public void Timeout_WithNegativeDuration_ThrowsException()
        {
            Service.AddHandler("model", new DynamicHandler().Auth(r =>
            {
                r.Timeout(-1);
                r.Ok();
            }));
            Service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.test.model.method", Test.EmptyRequest);
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInternalError);
        }
    }
}
