using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using ResgateIO.Service;
using System;
using System.IO;

namespace BookCollection
{
    class BookCollection
    {
        static void Main(string[] args)
        {
            // Start a simple webserver to serve the client.
            // This is only for the purpose of making the example easier to run.
            var host = StartWebserver();

            // Create a new RES Service
            ResService service = new ResService("library");

            // Add handler for "example.mymodel" resource
            service.MapHandler("book.$id", new BookModelHandler());
            service.MapHandler("books", new BookCollectionHandler());

            // Start the service
            service.Serve("nats://127.0.0.1:4222");

            // Wait for enter and then stop
            Console.ReadLine();
            service.Shutdown();
            host.StopAsync();
        }

        static IWebHost StartWebserver()
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseWebRoot(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
                .Configure(x => x.UseFileServer())
                .UseUrls("http://localhost:8082")
                .Build();
            host.RunAsync();
            return host;
        }
    }
}
