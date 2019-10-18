using ResgateIO.Service;
using System;

namespace TextService
{
    class Program
    {
        static void Main(string[] args)
        {
            // Create a new RES Service
            ResService service = new ResService("text");

            // Add handler for "text.mymodel" resource
            service.AddHandler("mymodel", new MyModelHandler());

            // Start the service
            service.Serve("nats://127.0.0.1:4222");

            // Wait for enter and then stop
            Console.ReadLine();
            service.Shutdown();
        }
    }
}
