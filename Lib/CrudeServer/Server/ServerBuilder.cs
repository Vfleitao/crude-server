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
        private IServerConfig __ServerConfiguration;
        public IServiceCollection ServiceCollection { get; private set; }
        public IServiceProvider ServiceProvider { get; private set; }
        public ICommandRegistry CommandRegistry { get; private set; }
        public IMiddlewareRegistry MiddlewareRegistry { get; private set; }

        public ServerBuilder()
        {
            this.RegisterBaseIOCItems();

            this.__ServerConfiguration = new ServerConfig
            {
                Host = "http://localhost",
                Port = "8000"
            };
        }

        public IServerBuilder SetConfiguration(ServerConfig config)
        {
            this.__ServerConfiguration = config;

            return this;
        }

        public IServerBuilder AddLogs()
        {
            this.ServiceCollection.AddScoped<ILoggerProvider, LoggerProvider>();
            this.MiddlewareRegistry.AddMiddleware<LoggerMiddleware>();

            return this;
        }

        public IServerBuilder AddAuthentication()
        {
            this.MiddlewareRegistry.AddMiddleware<AuthenticatorMiddleware>();
            this.ServiceCollection.AddScoped<IAuthenticationProvider, JTWAuthenticationProvider>();

            return this;
        }

        public IServerBuilder AddFiles(
            string fileRoot,
            Assembly fileAssembly,
            long cacheDurationMinutes = 10
        )
        {
            this.__ServerConfiguration.CachedDurationMinutes = cacheDurationMinutes;
            this.CommandRegistry.RegisterCommand<FileHttpCommand>(".*\\.\\w+", HttpMethod.GET);

            this.ServiceCollection.AddKeyedSingleton<string>(ServerConstants.FILE_ROOT, fileRoot);
            this.ServiceCollection.AddKeyedSingleton<Assembly>(ServerConstants.FILE_ASSEMBLY, fileAssembly);

            return this;
        }

        public IServerBuilder AddViews(
            string viewRoot,
            Assembly viewAssembly,
            Type viewProvider = null
        )
        {
            if (viewProvider == null)
            {
                viewProvider = typeof(HandleBarsViewProvider);
            }

            if (!typeof(ITemplatedViewProvider).IsAssignableFrom(viewProvider))
            {
                throw new ArgumentException("View provider must implement ITemplatedViewProvider");
            }

            this.ServiceCollection.AddKeyedSingleton<string>(ServerConstants.VIEW_ROOT, viewRoot);
            this.ServiceCollection.AddKeyedSingleton<Assembly>(ServerConstants.VIEW_ASSEMBLY, viewAssembly);

            this.ServiceCollection.AddSingleton(typeof(ITemplatedViewProvider), viewProvider);
            this.ServiceCollection.AddTransient<IHttpViewResponse, ViewResponse>();

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
            this.ServiceCollection.AddSingleton<IServerConfig>(this.__ServerConfiguration);
            this.MiddlewareRegistry.AddMiddleware<CommandExecutorMiddleware>();
            this.MiddlewareRegistry.AddMiddleware<ResponseProcessorMiddleware>();

            this.ServiceProvider = ServiceCollection.BuildServiceProvider(true);

            return this.ServiceProvider.GetService<IServerRunner>();
        }

        private void RegisterBaseIOCItems()
        {
            this.ServiceCollection = new ServiceCollection();
            this.ServiceCollection.AddSingleton<IServiceCollection>(ServiceCollection);

            this.CommandRegistry = new CommandRegistry(ServiceCollection);
            this.ServiceCollection.AddSingleton<ICommandRegistry>(CommandRegistry);

            this.MiddlewareRegistry = new MiddlewareRegistry(ServiceCollection);
            this.ServiceCollection.AddSingleton<IMiddlewareRegistry>(MiddlewareRegistry);

            this.ServiceCollection.AddSingleton((IServiceProvider provider) => this.ServiceProvider);
            this.ServiceCollection.AddSingleton<IServerRunner, ServerRunner>();

            this.ServiceCollection.AddScoped<IHttpRequestExecutor, HttpRequestExecutor>();
            this.ServiceCollection.AddScoped<IHttpRequestDataProvider, HttpRequestDataProvider>();

            this.ServiceCollection.AddKeyedScoped<IRequestDataParser, UrlDataParser>("dataparser_urlDataParser");
            this.ServiceCollection.AddKeyedScoped<IRequestDataParser, JsonDataParser>("dataparser_application/json");
            this.ServiceCollection.AddKeyedScoped<IRequestDataParser, MultiPartFormDataParser>("dataparser_multipart/form-data");
        }
    }
}
