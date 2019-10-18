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
                .SetGet(r => r.Model(new
                {
                    message = "Hello, World!",
                }))
                .SetAccess(r => r.AccessGranted()));
            service.Serve("nats://127.0.0.1:4222");
            Console.ReadLine();
        }
    }
}
