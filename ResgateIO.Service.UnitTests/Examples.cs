using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace ResgateIO.Service.UnitTests
{
    /// <summary>
    /// Tests that are the Usage examples used in the README.md file.
    /// This is to make sure the example code is valid.
    /// </summary>
    public class Examples : TestsBase
    {
        public Examples(ITestOutputHelper output) : base(output, "example")
        {
        }

        public class Article { }

        public class User
        {
            public string Id;
        }

        public static class DB
        {
            public static bool TryGetArticle(string id, out Article article)
            {
                article = null; return false;
            }
            public static Task<List<User>> QueryAsync(string _)
            {
                return Task.FromResult(new List<User> { new User { Id = "foo" } });
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
                .Get(r => r.Model(new {
                    message = "Hello, World!"
                }))
                .Access(r => r.AccessGranted()));
            service.Serve("nats://127.0.0.1:4222");
            // ---
            service.Shutdown();
        }

        [ResourcePattern("mymodel")]
        class MyModelHandler : BaseHandler
        {
            private readonly object model = new
            {
                message = "Hello, .NET World!"
            };

            public void Get(IModelRequest request)
            {
                request.Model(model);
            }
        }
        // ---
        [Fact]
        public void Usage_DefineAHandlerClassForAModelResource()
        {
            ResService service = new ResService("example");
            // ---
            service.AddHandler(new MyModelHandler());
        }

        [ResourcePattern("mycollection")]
        class MyCollectionHandler : BaseHandler
        {
            private readonly object[] collection = new object[]{
                "first", "second", "third"
            };

            public void Get(ICollectionRequest request)
            {
                request.Collection(collection);
            }
        }
        // ---
        [Fact]
        public void Usage_DefineAHandlerClassForACollectionResource()
        {
            ResService service = new ResService("example");
            // ---
            service.AddHandler(new MyCollectionHandler());
        }

        [ResourcePattern("math")]
        class MyResourceHandler : BaseHandler
        {
            [CallMethod("double")]
            public void Double(ICallRequest r)
            {
                r.Ok(2 * (double)r.Params["value"]);
            }
        }
        // ---
        [Fact]
        public void Usage_DefineMethodsOnAHandlerClass()
        {
            ResService service = new ResService("example");
            service.AddHandler(new MyResourceHandler());
            service.Serve(Conn);
            Conn.GetMsg().AssertSubject("system.reset");
            string inbox = Conn.NATSRequest("call.example.math.double", new RequestDto { CID = Test.CID, Params = new { value = 7 } });
            Conn.GetMsg()
                .AssertSubject(inbox)
                .AssertResult(14.0);
        }

        [Fact]
        public void Usage_AddHandlerForAResource()
        {
            ResService service = new ResService("example");
            // ---
            service.AddHandler(new MyResourceHandler());
        }

        [Fact]
        public void Usage_AddHandlersForParameterizedResources()
        {
            ResService service = new ResService("example");
            // ---
            service.AddHandler("article.$id", new DynamicHandler()
                .Access(r => r.AccessGranted())
                .ModelGet(r =>
                {
                    if (DB.TryGetArticle(r.PathParams["id"], out Article article))
                        r.Model(article);
                    else
                        r.NotFound();
                }));
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
                resource.ChangeEvent(new Dictionary<string, object> {
                    { "name", "bar" }
                });
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
            .AuthMethod("login", r =>
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
                .Access(r =>
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

        [Fact]
        public void Usage_AddAsyncHandler()
        {
            ResService service = new ResService("example");
            // ---
            service.AddHandler("store.users", new DynamicHandler()
                .Get(async r =>
                {
                    var users = await DB.QueryAsync("SELECT id FROM users");
                    r.Collection(users.Select(u => new Ref("store.user." + u.Id)));
                }));
        }
    }
}