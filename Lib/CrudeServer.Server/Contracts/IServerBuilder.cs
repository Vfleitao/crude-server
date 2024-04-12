using System;
using System.Reflection;

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

        IServerBuilder AddAuthentication();
        IServerBuilder AddFiles(string fileRoot, Assembly fileAssembly);
        IServerBuilder AddLogs();
        IServerRunner Buid();
        IServerBuilder SetConfiguration(ServerConfig config);
    }
}