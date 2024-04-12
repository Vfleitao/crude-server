using System;
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
            serverBuilder
                .AddLogs()
                .AddAuthentication()
                .AddFiles("wwwroot", typeof(Program).Assembly)
                .SetConfiguration(new ServerConfig()
                {
                    Host = "http://localhost",
                    Port = "9000",
                    AuthenticationPath = "/login"
                });

            serverBuilder.CommandRegistry.RegisterCommand<DemoHttpCommand>("/", HttpMethod.GET);
            serverBuilder.CommandRegistry
                .RegisterCommand<LoginCommand>("/login", HttpMethod.GET);
            serverBuilder.CommandRegistry
                .RegisterCommand<AccountCommand>("/account", HttpMethod.GET)
                .RequireAuthentication();
            serverBuilder.CommandRegistry
                .RegisterCommand<DemoGetAPICommand>("/api", HttpMethod.GET);
            serverBuilder.CommandRegistry
                .RegisterCommand<DemoPostAPICommand>("/api", HttpMethod.POST);

            IServerRunner server = serverBuilder.Buid();
            await server.Run();
        }

        private class DemoHttpCommand : HttpCommand
        {
            private static int pageViews = 0;
            private static Random random = new Random();

            private const string pageData =
            @"<!DOCTYPE>
                <html>
                    <head>
                        <title>Here is a demo command</title>
                        <link rel=""stylesheet"" type=""text/css"" href=""main.css"">
                    </head>
                    <body>
                    <p>HELLO WORLD. I have been called {0} times.</p>
                    </body>
                </html>";

            protected override async Task<IHttpResponse> Process()
            {
                pageViews++;

                OkResponse response = new OkResponse();
                response.ResponseData = Encoding.UTF8.GetBytes(string.Format(pageData, pageViews));

                return response;
            }
        }

        private class LoginCommand : HttpCommand
        {
            private const string pageData =
            @"<!DOCTYPE>
                <html>
                    <head>
                    <title>Here is a demo login page</title>
                    </head>
                    <body>
                    <p>LOGIN PAGE HERE</p>
                    </body>
                </html>";

            protected override async Task<IHttpResponse> Process()
            {
                OkResponse response = new OkResponse();
                response.ResponseData = Encoding.UTF8.GetBytes(pageData);

                return response;
            }
        }

        private class AccountCommand : HttpCommand
        {
            private const string pageData =
            @"<!DOCTYPE>
                <html>
                    <head>
                    <title>Here is a demo account page</title>
                    </head>
                    <body>
                    <p>Acount PAGE HERE</p>
                    </body>
                </html>";

            protected override async Task<IHttpResponse> Process()
            {
                OkResponse response = new OkResponse();
                response.ResponseData = Encoding.UTF8.GetBytes(pageData);

                return response;
            }
        }

        private class DemoGetAPICommand : HttpCommand
        {
            protected override async Task<IHttpResponse> Process()
            {
                OkResponse response = new JsonResponse();
                response.SetData(new { message = "Hello ", to = "world" });

                return response;
            }
        }

        private class DemoPostAPICommand : HttpCommand
        {
            protected override async Task<IHttpResponse> Process()
            {
                OkResponse response = new JsonResponse();
                response.SetData(new { message = "Hello ", to = "world", from = "POST" });

                return response;
            }
        }
    }
}
