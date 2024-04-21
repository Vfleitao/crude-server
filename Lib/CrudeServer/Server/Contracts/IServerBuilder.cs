using System;
using System.Reflection;

using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Enums;
using CrudeServer.HttpCommands;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models;

using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.Server.Contracts
{
    public interface IServerBuilder
    {
        ICommandRegistry CommandRegistry { get; }
        IServiceCollection Services { get; }
        IServiceProvider ServiceProvider { get; }

        IServerBuilder AddAuthentication();
        HttpCommandRegistration AddCommand<T>(string path, HttpMethod httpMethod) where T : HttpCommand;
        IServerBuilder AddCommands();
        IServerBuilder AddFiles(string fileRoot, Assembly fileAssembly, long cacheDurationMinutes = 10);
        IServerBuilder AddMiddleware<T>() where T : IMiddleware;
        IServerBuilder AddRequestTagging();
        IServerBuilder AddViews(string viewRoot, Assembly viewAssembly, Type viewProvider = null);
        IServerRunner Buid();
        IServerBuilder SetConfiguration(ServerConfig config);
    }
}