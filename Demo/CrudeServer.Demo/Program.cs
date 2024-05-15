using System.IO;
using System.Threading.Tasks;

using CrudeServer.Demo.Responses;
using CrudeServer.Enums;
using CrudeServer.Providers;
using CrudeServer.Server;
using CrudeServer.Server.Contracts;

namespace CrudeServer
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            string host = "http://localhost:9000/";
            if (args.Length > 0)
            {
                host = args[0];
            }

            string assemblyPath = System.AppContext.BaseDirectory;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);

#if DEBUG
            string fileparent = Path.Combine(assemblyDir, "../../../");
#else
            string fileparent = assemblyDir;
#endif

            string fileRoot = Path.Combine(fileparent, "wwwroot");
            string viewRoot = Path.Combine(fileparent, "views");

            IServerBuilder serverBuilder = new ServerBuilder();
            serverBuilder
                .AddRequestTagging()
                .AddCommandRetriever()
                .AddRequestDataRetriever()
                .AddCommandExecutor()
                .AddFiles(fileRoot, 60 * 24 * 30)
                .AddViews(viewRoot, null, typeof(FileHandleBarsViewProvider));

            serverBuilder.AddCommand<HomeCommand>("/", HttpMethod.GET);
            serverBuilder.AddCommand<NotFoundCommand>("/not-found", HttpMethod.GET);
            serverBuilder.AddCommand<InDepthRedirectCommand>("/in-depth", HttpMethod.GET);
            serverBuilder.AddCommand<InDepthPageCommand>("/in-depth/{page:\\w+}", HttpMethod.GET);

            serverBuilder.ReplaceDefaultResponses<NotFoundResponse>(DefaultStatusCodes.NotFound);

            IServerRunner server = serverBuilder.Buid();
            await server.Run();
        }
    }
}
