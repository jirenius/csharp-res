using LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using ResgateIO.Service;
using System;
using System.IO;

namespace Search
{
    class Program
    {
        static void Main(string[] args)
        {
            // Start a simple webserver to serve the client.
            // This is only for the purpose of making the example easier to run.
            StartWebserver();

            var service = new ResService("search");
            
            // Open or create the database
            using (var db = new LiteDatabase(@"lite.db"))
            {
                // Add handlers for the resources
                service.AddHandler(new CustomerHandler(db));
                service.AddHandler(new CustomersHandler(db));
                service.AddHandler(new CountriesHandler());

                // Start the service
                service.Serve("nats://127.0.0.1:4222");

                // Wait for enter and then stop
                Console.ReadLine();
                Console.WriteLine("Trying to shut down");
                service.Shutdown();
            }
        }

        static void StartWebserver()
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseWebRoot(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
                .Configure(x => x.UseFileServer())
                .UseUrls("http://localhost:8083")
                .Build();
            host.RunAsync();
        }
    }
}
