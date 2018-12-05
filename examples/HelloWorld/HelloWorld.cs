using Microsoft.AspNetCore.Hosting;
using ResgateIO.Service;

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
            service.MapHandler("mymodel", new MyModelHandler());

            // Start the service
            service.Serve("nats://127.0.0.1:4222");
        }

        static void StartWebserver()
        {
            IWebHost host = new WebHostBuilder()
               .UseKestrel()
               .UseStartup<Startup>()
               .UseUrls("http://localhost:8081")
               .Build();
            host.RunAsync();
        }
    }
}
