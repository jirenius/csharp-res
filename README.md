<p align="center"><a href="https://resgate.io" target="_blank" rel="noopener noreferrer"><img width="100" src="https://resgate.io/img/resgate-logo.png" alt="Resgate logo"></a></p>


<h2 align="center"><b>RES Service for .NET</b><br/>Synchronize Your Clients</h2>
</p>

<p align="center">
<a href="http://opensource.org/licenses/MIT"><img src="https://img.shields.io/badge/license-MIT-blue.svg" alt="License"></a>
<a href="https://www.nuget.org/packages/ResgateIO.Service"><img src="https://img.shields.io/nuget/v/ResgateIO.Service.svg" alt="NuGet"></a>
<a href="https://travis-ci.com/jirenius/csharp-res"><img src="https://travis-ci.com/jirenius/csharp-res.svg?branch=master" alt="Build Status"></a>
</p>

---

Library for .NET used to create next generation REST, real time, and RPC APIs, where all your reactive web clients are synchronized seamlessly through [Resgate](https://github.com/resgateio/resgate).

Visit [Resgate.io](https://resgate.io) for more information.

## As easy as
```csharp
ResService service = new ResService("example");
service.AddHandler("model", new DynamicHandler()
    .Get(r => r.Model(new {
        message = "Hello, World!"
    }))
    .Access(r => r.AccessGranted()));
service.Serve("nats://127.0.0.1:4222");
```

## Examples

| Example | Description
| --- | ---
| [Hello World](examples/01_HelloWorld/) | Smallest of services serving a static message.
| [Edit Text](examples/02_EditText/) | Single text field that is updated in real time.
| [Book Collection](examples/03_BookCollection/) | List of book titles & authors that can be edited by many.
| [Search](examples/04_Search/) | Make live queries against a large customer database.

## Basic usage

#### Create a new service

```csharp
ResService service = new ResService("myservice");
```

#### Define a handler class for a model resource

```csharp
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
```

#### Define a handler class for a collection resource

```csharp
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
```

#### Define methods on a handler class

```csharp
[ResourcePattern("math")]
class MyResourceHandler : BaseHandler
{
    [CallMethod("double")]
    public void Double(ICallRequest r)
    {
        r.Ok(2 * (double)r.Params["value"]);
    }
}
```

#### Add/register handler for a resource
```csharp
service.AddHandler(new MyResourceHandler());
```

#### Add handlers for parameterized resources

```csharp
service.AddHandler("article.$id", new DynamicHandler()
    .Access(r => r.AccessGranted())
    .ModelGet(r =>
    {
        if (DB.TryGetArticle(r.PathParams["id"], out Article article))
            r.Model(article);
        else
            r.NotFound();
    }));
```

#### Send change event on model update
A change event will update the model on all subscribing clients.

```csharp
MyModel mymodel = new MyModel { Name = "foo" };
MyModel mymodel = new MyModel { Name = "foo" };
service.With("example.mymodel", resource =>
{
    mymodel.Name = "bar";
    resource.ChangeEvent(new Dictionary<string, object> {
        { "name", "bar" }
    });
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
```

#### Add handlers for access control (with wildcard ">")

```csharp
service.AddHandler(">", new DynamicHandler()
    .Access(r =>
    {
        if (r.Token != null && (string)r.Token["user"] == "admin")
            r.AccessGranted();
        else
            r.AccessDenied();
    }));
```

#### Add async handler
```csharp
service.AddHandler("store.users", new DynamicHandler()
    .Get(async r =>
    {
        var users = await DB.QueryAsync("SELECT id FROM users");
        r.Collection(users.Select(u => new Ref("store.user." + u.Id)));
    }));
```

#### Start service

```csharp
service.Serve("nats://127.0.0.1:4222");
```

## Credits

Inspiration and support in development from [github.com/novagen](https://github.com/novagen), who wrote an initial .NET library for Resgate.

## Contributing

The .NET library is still under development, but the API is mostly settled. Any feedback on the library API or its implementation is highly appreciated!

Once the API is fully settled, the package will be moved to the [resgateio](https://github.com/resgateio/) GitHub organization.

If you find any issues, feel free to [report them](https://github.com/jirenius/csharp-res/issues/new) as an Issue.
