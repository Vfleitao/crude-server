﻿using System;
using System.Collections.Generic;
using System.Reflection;

using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Enums;
using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.Server.Contracts
{
    public interface IServerBuilder
    {
        ICommandRegistry CommandRegistry { get; }
        IServiceCollection Services { get; }
        IServiceProvider ServiceProvider { get; }
        IConfiguration Configuration { get; }
        ConfigurationBuilder ConfigurationBuilder { get; }

        IServerBuilder AddAntiforgeryTokens();
        IServerBuilder AddAuthentication(Type authenticationProvider = null);
        HttpCommandRegistration AddCommand<T>(string path, HttpMethod httpMethod) where T : HttpCommand;
        IServerBuilder AddCommandExecutor();
        IServerBuilder AddCommandRetriever();
        IServerBuilder AddEmbeddedFiles(string fileRoot, Assembly fileAssembly, long cacheDurationMinutes = 10);
        IServerBuilder AddEncryption();
        IServerBuilder AddFiles(string fileRoot, long cacheDurationMinutes = 10);
        IServerBuilder AddMiddleware<T>() where T : IMiddleware;
        IServerBuilder AddRequestDataRetriever();
        IServerBuilder AddRequestTagging();
        IServerBuilder AddViews(string viewRoot, Assembly viewAssembly = null, Type viewProvider = null);
        IServerRunner Build();
        IServerBuilder AddRequestSizeLimit();
        IServerBuilder ReplaceDefaultResponses<T>(DefaultStatusCodes defaultStatus) where T : IHttpResponse;
        IEnumerable<HttpCommandRegistration> AddCommands(Assembly assembly);
        HttpCommandRegistration AddCommandFunction(string path, HttpMethod httpMethod, Delegate delegateFunction);
        IServerBuilder OverrideHosts(params string[] hosts);
    }
}