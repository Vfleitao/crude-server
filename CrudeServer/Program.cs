using System.Text;
using System.Threading.Tasks;

using CrudeServer.Enums;
using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Models;
using CrudeServer.Server;
using CrudeServer.Server.Contracts;

namespace CrudeServer
{
    public class Program
    {

        public static async Task Main(string[] args)
        {
            IServerBuilder serverBuilder = new ServerBuilder();
            serverBuilder.CommandRegistry.RegisterCommand<DemoHttpCommand>("/", HttpMethod.GET);
            serverBuilder.SetConfiguration(new ServerConfig
            {
                Host = "http://localhost",
                Port = "9000"
            });

            IServerRunner server = serverBuilder.Buid();
            await server.Run();
        }

        private class DemoHttpCommand : HttpCommand
        {
            private static int pageViews = 0;

            private const string pageData =
            @"<!DOCTYPE>
                <html>
                    <head>
                    <title>Here is a demo command</title>
                    </head>
                    <body>
                    <p>HELLO WORLD. I have been called {0} times.</p>
                    </body>
                </html>";

            public override Task<IHttpResponse> Process()
            {
                pageViews++;

                OkResponse response = new OkResponse();
                response.ResponseData = Encoding.UTF8.GetBytes(string.Format(pageData, pageViews));

                return Task.FromResult<IHttpResponse>(response);
            }
        }
    }
}
