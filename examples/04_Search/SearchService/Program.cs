using LiteDB;
using ResgateIO.Service;
using System;

namespace SearchService
{
    class Program
    {
        static void Main(string[] args)
        {
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
                service.Shutdown();
            }
        }
    }
}
