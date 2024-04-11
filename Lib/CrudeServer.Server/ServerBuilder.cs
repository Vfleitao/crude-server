using System;

using CrudeServer.CommandRegistration;
using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Models;
using CrudeServer.Server.Contracts;

using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.Server
{
    public class ServerBuilder : IServerBuilder
    {
        private ServerConfig configuration;

        public IServiceCollection ServiceCollection { get; }
        public IServiceProvider ServiceProvider { get; private set; }
        public ICommandRegistry CommandRegistry { get; }

        public ServerBuilder()
        {
            ServiceCollection = new ServiceCollection();
            CommandRegistry = new CommandRegistry(ServiceCollection);

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
            this.ServiceProvider = ServiceCollection.BuildServiceProvider(true);

            return new ServerRunner(ServiceProvider, CommandRegistry, configuration);
        }
    }
}
