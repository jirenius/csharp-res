using System;
using Xunit;
using NATS.Client;

namespace ResgateIO.Service.UnitTests
{
    public class CreateService
    {
        [Fact]
        public void Serve()
        {
            ResService service = new ResService("test");

            ConnectionFactory cf = new ConnectionFactory();
            IConnection conn = cf.CreateConnection();

            service.Serve(conn);
        }
    }
}
