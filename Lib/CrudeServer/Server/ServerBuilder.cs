using System;
using System.Reflection;

using CrudeServer.CommandRegistration;
using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Consts;
using CrudeServer.Enums;
using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Middleware;
using CrudeServer.Middleware.Registration;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers;
using CrudeServer.Providers.Contracts;
using CrudeServer.Providers.DataParser;
using CrudeServer.Server.Contracts;

using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.Server
{
    public class ServerBuilder : IServerBuilder
    {
        private IServerConfig ServerConfiguration;

        public IServiceCollection Services { get; private set; }
        public IServiceProvider ServiceProvider { get; private set; }
        public ICommandRegistry CommandRegistry { get; private set; }
        public IMiddlewareRegistry MiddlewareRegistry { get; private set; }

        public ServerBuilder()
        {
            this.RegisterBaseIOCItems();

            this.ServerConfiguration = new ServerConfig() { };
        }

        public IServerBuilder SetConfiguration(ServerConfig config)
        {
            this.ServerConfiguration = config;

            return this;
        }

        public IServerBuilder AddAuthentication()
        {
            this.MiddlewareRegistry.AddMiddleware<AuthenticatorMiddleware>();
            this.Services.AddScoped<IAuthenticationProvider, JTWAuthenticationProvider>();

            return this;
        }

        public IServerBuilder AddEmbeddedFiles(
            string fileRoot,
            Assembly fileAssembly,
            long cacheDurationMinutes = 10
        )
        {
            this.ServerConfiguration.CachedDurationMinutes = cacheDurationMinutes;
            this.CommandRegistry.RegisterCommand<EmbeddedFileHttpCommand>(".*\\.\\w+", HttpMethod.GET);

            this.Services.AddKeyedSingleton<string>(ServerConstants.FILE_ROOT, fileRoot);
            this.Services.AddKeyedSingleton<Assembly>(ServerConstants.FILE_ASSEMBLY, fileAssembly);

            return this;
        }

        public IServerBuilder AddFiles(
            string fileRoot,
            long cacheDurationMinutes = 10
        )
        {
            this.ServerConfiguration.CachedDurationMinutes = cacheDurationMinutes;
            this.CommandRegistry.RegisterCommand<FileHttpCommand>(".*\\.\\w+", HttpMethod.GET);

            this.Services.AddKeyedSingleton<string>(ServerConstants.FILE_ROOT, fileRoot);

            return this;
        }

        public IServerBuilder AddViews(
            string viewRoot,
            Assembly viewAssembly = null,
            Type viewProvider = null
        )
        {
            if (viewProvider == null)
            {
                viewProvider = typeof(EmbeddedHandleBarsViewProvider);
            }

            if (!typeof(ITemplatedViewProvider).IsAssignableFrom(viewProvider))
            {
                throw new ArgumentException("View provider must implement ITemplatedViewProvider");
            }

            this.Services.AddKeyedSingleton<string>(ServerConstants.VIEW_ROOT, viewRoot);

            if (viewAssembly != null)
            {
                this.Services.AddKeyedSingleton<Assembly>(ServerConstants.VIEW_ASSEMBLY, viewAssembly);
            }

            this.Services.AddSingleton(typeof(ITemplatedViewProvider), viewProvider);
            this.Services.AddTransient<IHttpViewResponse, ViewResponse>();

            return this;
        }

        public HttpCommandRegistration AddCommand<T>(string path, HttpMethod httpMethod) where T : HttpCommand
        {
            return this.CommandRegistry.RegisterCommand<T>(path, httpMethod);
        }

        public IServerBuilder AddMiddleware<T>() where T : IMiddleware
        {
            this.MiddlewareRegistry.AddMiddleware<T>();

            return this;
        }

        public IServerBuilder AddRequestTagging()
        {
            this.MiddlewareRegistry.AddMiddleware<RequestTaggerMiddleware>();

            return this;
        }


        public IServerRunner Buid()
        {
            this.MiddlewareRegistry.AddMiddleware<ResponseProcessorMiddleware>();
            this.Services.AddSingleton<IServerConfig>(this.ServerConfiguration);
            this.ServiceProvider = Services.BuildServiceProvider(true);

            return this.ServiceProvider.GetService<IServerRunner>();
        }

        public IServerBuilder AddCommands()
        {
            this.MiddlewareRegistry.AddMiddleware<CommandExecutorMiddleware>();

            return this;
        }

        private void RegisterBaseIOCItems()
        {
            this.Services = new ServiceCollection();
            this.Services.AddSingleton<IServiceCollection>(Services);

            this.CommandRegistry = new CommandRegistry(Services);
            this.Services.AddSingleton<ICommandRegistry>(CommandRegistry);

            this.MiddlewareRegistry = new MiddlewareRegistry(Services);
            this.Services.AddSingleton<IMiddlewareRegistry>(MiddlewareRegistry);

            this.Services.AddSingleton((IServiceProvider provider) => this.ServiceProvider);
            this.Services.AddSingleton<IServerRunner, ServerRunner>();

            this.Services.AddScoped<IHttpRequestExecutor, HttpRequestExecutor>();
            this.Services.AddScoped<IHttpRequestDataProvider, HttpRequestDataProvider>();

            this.Services.AddKeyedScoped<IRequestDataParser, UrlDataParser>("dataparser_urlDataParser");
            this.Services.AddKeyedScoped<IRequestDataParser, JsonDataParser>("dataparser_application/json");
            this.Services.AddKeyedScoped<IRequestDataParser, MultiPartFormDataParser>("dataparser_multipart/form-data");

            this.Services.AddScoped<ILogger, ConsoleLogger>();
            this.MiddlewareRegistry.AddMiddleware<LoggerMiddleware>();
        }
    }
}
