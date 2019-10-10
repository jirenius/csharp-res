using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.IO;

namespace WebClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseWebRoot(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"))
                .Configure(x => x.UseFileServer())
                .UseUrls("http://localhost:8081")
                .Build();
            host.RunAsync();
            Console.ReadLine();
        }
    }
}
