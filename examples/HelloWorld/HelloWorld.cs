using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using ResgateIO.Service;
using System;
using System.IO;

namespace HelloWorld
{
    class HelloWorld
    {
        static void Main(string[] args)
        {
            // Start a simple webserver to serve the client.
            // This is only for the purpose of making the example easier to run.
            StartWebserver();

            // Create a new RES Service
            ResService service = new ResService("example");

            // Add handler for "example.mymodel" resource
            service.AddHandler("mymodel", new MyModelHandler());

            // Start the service
            service.Serve("nats://127.0.0.1:4222");

            // Wait for enter and then stop
            Console.ReadLine();
            service.Shutdown();
        }

        static void StartWebserver()
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseWebRoot(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
                .Configure(x => x.UseFileServer())
                .UseUrls("http://localhost:8081")
                .Build();
            host.RunAsync();
        }
    }
}
