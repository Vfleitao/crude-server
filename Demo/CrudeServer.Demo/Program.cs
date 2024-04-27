using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using CrudeServer.Demo.Middleware;
using CrudeServer.Enums;
using CrudeServer.Models;
using CrudeServer.Providers;
using CrudeServer.Server;
using CrudeServer.Server.Contracts;

namespace CrudeServer
{
    public partial class Program
    {

        public static async Task Main(string[] args)
        {
            string assemblyPath = System.AppContext.BaseDirectory;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);

#if DEBUG
            string fileparent = assemblyDir;
#else
            string fileparent = Path.Combine(assemblyDir, "..\\..\\..\\");
#endif

            string fileRoot = Path.Combine(fileparent, "wwwroot");
            string viewRoot = Path.Combine(fileparent, "views");

            IServerBuilder serverBuilder = new ServerBuilder();
            serverBuilder
                .SetConfiguration(new ServerConfig()
                {
                    Hosts = new List<string> { "http://localhost:9000/" },
                    AuthenticationPath = "/login",
                    NotFoundPath = "/not-found",
                    EnableServerFileCache = true,
                })
                .AddRequestTagging()
                .AddCommands()
                .AddFiles(fileRoot, 60 * 24 * 30)
                .AddViews(viewRoot, viewProvider: typeof(FileHandleBarsViewProvider));

            serverBuilder.AddCommand<HomeCommand>("/", HttpMethod.GET);
            serverBuilder.AddCommand<NotFoundCommand>("/not-found", HttpMethod.GET);
            serverBuilder.AddCommand<InDepthRedirectCommand>("/in-depth", HttpMethod.GET);
            serverBuilder.AddCommand<InDepthPageCommand>("/in-depth/{page:\\w+}", HttpMethod.GET);

            IServerRunner server = serverBuilder.Buid();
            await server.Run();
        }
    }
}
