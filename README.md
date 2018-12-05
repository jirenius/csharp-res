# RES service library for C#

A C# library implementing the RES-Service protocol for [Resgate - Real-time API Gateway](https://github.com/jirenius/resgate).  
When you want to create stateless REST API services but need to have all your resources updated in real time on your reactive web clients.

All resources and methods served by RES services are made accessible through [Resgate](https://github.com/jirenius/resgate) in two ways:
* Ordinary HTTP requests
* Over WebSocket using [ResClient](https://www.npmjs.com/package/resclient)

With ResClient, all resources will be updated in real time, without having to write a single line of client code to handle specific events. It just works.

## Credits

Inspiration and support in development from [github.com/novagen](https://github.com/novagen), who wrote an initial C# library for Resgate.

## Contributing

The csharp-res library is still under development, and commits may still contain breaking changes. It should only be used for educational purpose. Any feedback on the package API or its implementation is highly appreciated!

If you find any issues, feel free to [report them](https://github.com/jirenius/csharp-res/issues/new) as an Issue.
