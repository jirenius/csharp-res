# RES Service for .NET : Synchronize Your Clients

Library used to create REST, real time, and RPC APIs, where all your reactive web clients are synchronized seamlessly through [Resgate](https://github.com/resgateio/resgate).

Visit [Resgate.io](https://resgate.io) for more information on the project.  
Visit [the GitHub repository](https://github.com/jirenius/csharp-res) for complete examples with both client and server.

## As easy as
```csharp
ResService service = new ResService("example");
service.AddHandler("model", new DynamicHandler()
    .SetGet(r => r.Model(new { message = "Hello, World!" }))
    .SetAccess(r => r.AccessGranted()));
service.Serve("nats://127.0.0.1:4222");
```

## Basic usage

#### Create a new service

```csharp
ResService service = new ResService("myservice");
```

#### Define a handler class for a model resource

```csharp
class MyModelHandler : ModelHandler
{
    private readonly object model = new {
        message = "Hello, .NET World!"
    }

    public override void Get(IModelRequest request)
    {
        request.Model(model);
    }
}
```

#### Define a handler class for a collection resource

```csharp
class MyModelHandler : CollectionHandler
{
    private readonly object[] collection = new object[]{
        "first", "second", "third"
    }

    public override void Get(ICollectionRequest request)
    {
        request.Collection(collection);
    }
}
```
#### Add handler for a resource
```csharp
service.AddHandler("model", new MyResourceHandler());
```

#### Add handlers for a collection resource

```csharp
mycollection := []string{"first", "second", "third"}
s.Handle("mycollection",
   res.Access(res.AccessGranted),
   res.GetCollection(func(r res.CollectionRequest) {
      r.Collection(mycollection)
   }),
)
```

#### Add handlers for parameterized resources

```csharp
service.AddHandler("article.$id", new DynamicHandler()
    .SetAccess(r => r.AccessGranted())
    .SetModelGet(r =>
    {
        if (DB.TryGetArticle(r.PathParams["id"], out Article article))
            r.Model(article);
        else
            r.NotFound();
    }));
```

#### Add handlers for method calls

```csharp
service.AddHandler("math", new DynamicHandler()
    .SetCallMethod("double", r =>
    {
        r.Ok(2 * (double)r.Params["value"]);
    }));
```

#### Send change event on model update
A change event will update the model on all subscribing clients.

```csharp
MyModel mymodel = new MyModel { Name = "foo" };
service.With("example.mymodel", resource =>
{
    mymodel.Name = "bar";
    resource.ChangeEvent(new Dictionary<string, object> { { "name", "bar" } });
});
```

#### Send add event on collection update:
An add event will update the collection for all subscribing clients.

```csharp
var mycollection = new List<string> { "first", "second" };
service.With("example.mycollection", resource =>
{
    resource.AddEvent("third", mycollection.Count);
    mycollection.Add("third");
});
```

#### Add handlers for authentication

```csharp
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
```

#### Add handlers for access control (with wildcard ">")

```csharp
service.AddHandler(">", new DynamicHandler()
    .SetAccess(r =>
    {
        if (r.Token != null && (string)r.Token["user"] == "admin")
            r.AccessGranted();
        else
            r.AccessDenied();
    }));
```

#### Start service

```csharp
service.Serve("nats://127.0.0.1:4222");
```

## Contributing

The ResgateIO.Service library is still under development, and minor versions may still contain breaking changes in the API. Any feedback on the package API or its implementation is highly appreciated!

If you find any issues, feel free to [report them](https://github.com/jirenius/csharp-res/issues/new) as an issue on GitHub.
