using System;

using CrudeServer.CommandRegistration;
using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Middleware;
using CrudeServer.MiddlewareRegistration;
using CrudeServer.MiddlewareRegistration.Contracts;
using CrudeServer.Models;
using CrudeServer.Server.Contracts;

using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.Server
{
    public class ServerBuilder : IServerBuilder
    {
        private ServerConfig configuration;

        public IServiceCollection ServiceCollection { get; private set; }
        public IServiceProvider ServiceProvider { get; private set; }
        public ICommandRegistry CommandRegistry { get; private set; }
        public IMiddlewareRegistry MiddlewareRegistry { get; private set; }

        public ServerBuilder()
        {
            this.RegisterItems();

            this.configuration = new ServerConfig
            {
                Host = "http://localhost",
                Port = "8000"
            };
        }

        /// <summary>
        /// Allows the user to set the configuration of the server in case they need a different host or port
        /// </summary>
        /// <param name="config"></param>
        public void SetConfiguration(ServerConfig config)
        {
            this.configuration = config;
        }

        public IServerRunner Buid()
        {
            this.ServiceCollection.AddSingleton<ServerConfig>(this.configuration);
            
            this.MiddlewareRegistry.AddMiddleware<CommandExecutorMiddleware>();

            this.ServiceProvider = ServiceCollection.BuildServiceProvider(true);

            return this.ServiceProvider.GetService<IServerRunner>();
        }

        private void RegisterItems()
        {
            this.ServiceCollection = new ServiceCollection();
            this.ServiceCollection.AddSingleton<IServiceCollection>(ServiceCollection);

            this.CommandRegistry = new CommandRegistry(ServiceCollection);
            this.ServiceCollection.AddSingleton<ICommandRegistry>(CommandRegistry);

            this.MiddlewareRegistry = new MiddlewareRegistry(ServiceCollection);
            this.ServiceCollection.AddSingleton<IMiddlewareRegistry>(MiddlewareRegistry);

            this.ServiceCollection.AddSingleton((IServiceProvider provider) => this.ServiceProvider);

            this.ServiceCollection.AddSingleton<IServerRunner, ServerRunner>();

            this.MiddlewareRegistry.AddMiddleware<LoggerMiddleware>();
        }
    }
}
