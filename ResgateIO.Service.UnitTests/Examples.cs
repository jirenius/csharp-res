using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace ResgateIO.Service.UnitTests
{
    public class Examples : TestsBase
    {
        public Examples(ITestOutputHelper output) : base(output, "example")
        {
        }

        public class Article { }

        public static class DB
        {
            public static bool TryGetArticle(string id, out Article article)
            {
                article = null; return false;
            }
        }

        public class MyModel {
            public string Name;
        }

        public class LoginParams
        {
            public string Password;
        }

        #pragma warning disable xUnit1013 // The test requires a running NATS server
        public void Usage_HelloWorld()
        #pragma warning restore xUnit1013
        {
            ResService service = new ResService("example");
            service.AddHandler("model", new DynamicHandler()
                .SetGet(r => r.Model(new {
                    message = "Hello, World!"
                }))
                .SetAccess(r => r.AccessGranted()));
            service.Serve("nats://127.0.0.1:4222");
            // ---
            service.Shutdown();
        }

        [Fact]
        public void Usage_AddHandlersForParameterizedResources()
        {
            ResService service = new ResService("example");
            // ---
            service.AddHandler("article.$id", new DynamicHandler()
                .SetAccess(r => r.AccessGranted())
                .SetModelGet(r =>
                {
                    if (DB.TryGetArticle(r.PathParams["id"], out Article article))
                        r.Model(article);
                    else
                        r.NotFound();
                }));
        }

        [Fact]
        public void Usage_AddHandlersForMethodCalls()
        {
            service.AddHandler("math", new DynamicHandler()
                .SetCallMethod("double", r =>
                {
                    r.Ok(2 * (double)r.Params["value"]);
                }));
            // --
            service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.example.math.double", new RequestDto { CID = Test.CID, Params = new { value = 7 } });
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(14.0);
        }

        [Fact]
        public void Usage_SendChangeEventOnModelUpdate()
        {
            service.AddHandler("mymodel", new DynamicHandler().SetType(ResourceType.Model));
            service.Serve(Conn);
            // --
            MyModel mymodel = new MyModel { Name = "foo" };
            service.With("example.mymodel", resource =>
            {
                mymodel.Name = "bar";
                resource.ChangeEvent(new Dictionary<string, object> { { "name", "bar" } });
            });
            // --
            Conn.GetMsg().AssertSubject("event.example.mymodel.change");
        }

        [Fact]
        public void Usage_SendAddEventOnCollectionUpdate()
        {
            service.AddHandler("mycollection", new DynamicHandler().SetType(ResourceType.Collection));
            service.Serve(Conn);
            // --
            var mycollection = new List<string> { "first", "second" };
            service.With("example.mycollection", resource =>
            {
                resource.AddEvent("third", mycollection.Count);
                mycollection.Add("third");
            });
            // --
            Conn.GetMsg().AssertSubject("event.example.mycollection.add");
        }

        [Fact]
        public void Usage_AddHandlersForAuthentication()
        {
        service.AddHandler("myauth", new DynamicHandler()
            .SetAuthMethod("login", r =>
            {
                if ((string)r.Params["password"] == "mysecret")
                {
                    r.TokenEvent(new { user = "admin" });
                    r.Ok();
                }
                else
                {
                    r.InvalidParams("Wrong password");
                }
            }));
            // --
            service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("auth.example.myauth.login", new RequestDto { CID = Test.CID, Params = new { password = "wrongsecret" } });
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertError(ResError.CodeInvalidParams);
        }

        [Fact]
        public void Usage_AddHandlersForAccessControl_WithWildcard()
        {
            service.AddHandler(">", new DynamicHandler()
                .SetAccess(r =>
                {
                    if (r.Token != null && (string)r.Token["user"] == "admin")
                        r.AccessGranted();
                    else
                        r.AccessDenied();
                }));
            // --
            service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("access.example.something", new RequestDto { CID = Test.CID, Params = new { user = "admin" } });
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult();
        }
    }
}