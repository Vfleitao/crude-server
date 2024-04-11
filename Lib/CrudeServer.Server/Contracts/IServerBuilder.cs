using System;
using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Models;

using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.Server.Contracts
{
    public interface IServerBuilder
    {
        ICommandRegistry CommandRegistry { get; }
        IServiceCollection ServiceCollection { get; }
        IServiceProvider ServiceProvider { get; }

        IServerRunner Buid();
        void SetConfiguration(ServerConfig config);
    }
}