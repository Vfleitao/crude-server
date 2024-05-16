using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using CrudeServer.Middleware;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;
using CrudeServer.Server;
using CrudeServer.Server.Contracts;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace CrudeServer.Integration.Mocks
{
    public static class ServerBuilderCreator
    {
        public static IServerBuilder CreateTestServerBuilder(
            int port,
            bool useEmbeddedFiles = true,
            bool useAntiforgeryTokens = false,
            bool useRequestSizeLimiter = false
        )
        {
            IServerBuilder serverBuilder = new ServerBuilder();
            serverBuilder
                .AddRequestTagging()
            .AddAuthentication()
            .AddCommandRetriever();

            serverBuilder.ConfigurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ServerConfiguration:Hosts:0"] = $"http://localhost:{port}/"
            });

            if (useRequestSizeLimiter)
            {
                serverBuilder.AddRequestSizeLimit();
            }

            serverBuilder.AddRequestDataRetriever();

            if (useAntiforgeryTokens)
            {
                serverBuilder.AddAntiforgeryTokens();
            }

            serverBuilder
                .AddCommandExecutor()
                .AddViews("views", typeof(ServerBuilderCreator).Assembly);

            serverBuilder.Services.Remove(
                serverBuilder.Services.First(x => x.ServiceType == typeof(LoggerMiddleware))
            );

            serverBuilder.Services.AddScoped<LoggerMiddleware>(x => new MockMiddleware(null));

            serverBuilder.Services.Remove(
               serverBuilder.Services.First(x => x.ServiceType == typeof(ILogger))
            );

            serverBuilder.Services.AddScoped<ILogger>(x => Mock.Of<ILogger>());

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

    internal class MockMiddleware : LoggerMiddleware
    {
        public MockMiddleware(ILogger loggerProvider) : base(loggerProvider)
        {
        }

        public override Task Process(ICommandContext context, Func<Task> next)
        {
            return next();
        }
    }
}
