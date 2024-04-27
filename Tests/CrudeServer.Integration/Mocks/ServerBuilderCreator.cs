using System.Collections.Generic;
using System.IO;
using System.Reflection;

using CrudeServer.Models;
using CrudeServer.Server;
using CrudeServer.Server.Contracts;

namespace CrudeServer.Integration.Mocks
{
    public static class ServerBuilderCreator
    {
        public static IServerBuilder CreateTestServerBuilder(int port, bool useEmbeddedFiles = true)
        {
            IServerBuilder serverBuilder = new ServerBuilder();
            serverBuilder
                .SetConfiguration(new ServerConfig()
                {
                    Hosts = new List<string>() { "http://localhost:" + port.ToString() + "/" },
                    AuthenticationPath = "/login",
                    EnableServerFileCache = false
                })
                .AddRequestTagging()
                .AddAuthentication()
                .AddCommands()
                .AddViews("views", typeof(ServerBuilderCreator).Assembly);

            if (useEmbeddedFiles)
            {
                serverBuilder.AddEmbeddedFiles("wwwroot", typeof(ServerBuilderCreator).Assembly);
            }
            else
            {
                string assemblyPath = Assembly.GetExecutingAssembly().Location;
                string assemblyDir = Path.GetDirectoryName(assemblyPath);

                string fileRoot = Path.Combine(assemblyDir, "wwwroot");

                serverBuilder.AddFiles(fileRoot);
            }

            return serverBuilder;
        }

    }
}
