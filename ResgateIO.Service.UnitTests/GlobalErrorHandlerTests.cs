using System.Threading;
using Xunit;
using Xunit.Abstractions;

namespace ResgateIO.Service.UnitTests
{
    public class GlobalErrorHandlerTests : TestsBase
    {
        public GlobalErrorHandlerTests(ITestOutputHelper output) : base(output)
        {
        }

        [ResourcePattern("handler")]
        class ErrorResourceHandler : BaseHandler
        {
            [CallMethod("action")]
            public void Action(ICallRequest _)
            {
                throw new System.Exception("An error occured");
            }
        }

        [Fact]
        public void Usage_DefineGlobalRequestErrorHandler()
        {
            var expectedResult = new { code = "TestCode", message = "Test message", data = "Test data" };
            ResService service = new ResService("error");

            service.SetGlobalRequestErrorHandler((e, r) =>
            {
                r.Error(new ResError(expectedResult.code, expectedResult.message, expectedResult.data));
            });

            service.AddHandler(new ErrorResourceHandler());
            service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.error.handler.action", new RequestDto { CID = Test.CID, Params = new { value = 7 } });
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(expectedResult.code, expectedResult.message, expectedResult.data);
        }

        [Fact]
        public void Usage_ExceptionInDefineGlobalRequestErrorHandler()
        {
            var expectedResult = new { code = "TestCode", message = "Test message", data = "Test data" };
            ResService service = new ResService("error");

            service.SetGlobalRequestErrorHandler((e, r) =>
            {
                throw new System.Exception("An error occured");
            });

            service.AddHandler(new ErrorResourceHandler());
            service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.error.handler.action", new RequestDto { CID = Test.CID, Params = new { value = 7 } });
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInternalError);
        }

        [Fact]
        public void Usage_ValidateGlobalRequestErrorHandlerParameters()
        {
            var expectedResult = new { code = "TestCode", message = "Test message", data = "Test data" };
            ResService service = new ResService("error");

            System.Exception exceptionParam = null;
            IRequest requestParam = null;
            var waitForErrorCallbackExecution = new EventWaitHandle(false, EventResetMode.AutoReset);

            service.SetGlobalRequestErrorHandler((e, r) =>
            {
                try
                {
                    exceptionParam = e;
                    requestParam = r;
                    new ResError(expectedResult.code, expectedResult.message, expectedResult.data);
                }
                finally { waitForErrorCallbackExecution.Set(); }
            });

            service.AddHandler(new ErrorResourceHandler());
            service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            Conn.NATSRequest("call.error.handler.action", new RequestDto { CID = Test.CID, Params = new { value = 7 } });

            waitForErrorCallbackExecution.WaitOne(3000);

            Assert.NotNull(exceptionParam);
            Assert.NotNull(requestParam);
        }
    }
}
