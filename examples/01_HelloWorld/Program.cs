using ResgateIO.Service;
using System;

namespace HelloWorld
{
    class Program
    {
        static void Main(string[] args)
        {
            ResService service = new ResService("example");
            service.AddHandler("model", new DynamicHandler()
                .Get(r => r.Model(new
                {
                    message = "Hello, World!",
                }))
                .Access(r => r.AccessGranted()));
            service.Serve("nats://127.0.0.1:4222");
            Console.ReadLine();
        }
    }
}
