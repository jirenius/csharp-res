using ResgateIO.Service;
using System;

namespace LibraryService
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a new RES Service
            ResService service = new ResService("library");

            // Add handler for "example.mymodel" resource
            service.AddHandler("book.$id", new BookModelHandler());
            service.AddHandler("books", new BookCollectionHandler());

            // Start the service
            service.Serve("nats://127.0.0.1:4222");

            // Wait for enter and then stop
            Console.ReadLine();
            service.Shutdown();
        }
    }
}
