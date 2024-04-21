using CrudeServer.Models;
using CrudeServer.Server;
using CrudeServer.Server.Contracts;

namespace CrudeServer.Integration.Mocks
{
    public static class ServerBuilderCreator
    {
        public static IServerBuilder CreateTestServerBuilder(int port)
        {
            IServerBuilder serverBuilder = new ServerBuilder();
            serverBuilder
                .AddRequestTagging()
                .AddAuthentication()
                .AddFiles("wwwroot", typeof(ServerBuilderCreator).Assembly)
                .AddViews("views", typeof(ServerBuilderCreator).Assembly)
                .SetConfiguration(new ServerConfig()
                {
                    Hosts = new List<string>() { "http://localhost:" + port.ToString() },
                    AuthenticationPath = "/login"
                });

            return serverBuilder;
        }
    }
}
