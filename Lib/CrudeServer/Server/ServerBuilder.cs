using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;

using CrudeServer.Attributes;
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
using CrudeServer.Providers.Responses;
using CrudeServer.Server.Contracts;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.Server
{
    public class ServerBuilder : IServerBuilder
    {
        private bool hasAntiforgeryTokens;

        private bool hasAuthentication;
        private Type authenticationProviderType;


        public ConfigurationBuilder ConfigurationBuilder { get; private set; }
        public IConfiguration Configuration { get; private set; }
        public IServiceCollection Services { get; private set; }
        public IServiceProvider ServiceProvider { get; private set; }
        public ICommandRegistry CommandRegistry { get; private set; }
        public IMiddlewareRegistry MiddlewareRegistry { get; private set; }
        public IStandardResponseRegistry StandardResponseRegistry { get; private set; }

        public ServerBuilder()
        {
            this.RegisterBaseIOCItems();
        }

        public IServerBuilder AddAuthentication(Type authenticationProvider = null)
        {
            if (authenticationProvider == null)
            {
                authenticationProvider = typeof(JTWAuthenticationProvider);
            }

            if (!typeof(IAuthenticationProvider).IsAssignableFrom(authenticationProvider))
            {
                throw new ArgumentException("Provider must implement IAuthenticationProvider");
            }

            this.hasAuthentication = true;
            this.authenticationProviderType = authenticationProvider;



            return this;
        }

        public IServerBuilder AddEmbeddedFiles(
            string fileRoot,
            Assembly fileAssembly,
            long cacheDurationMinutes = 10
        )
        {
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
            this.CommandRegistry
                .RegisterCommand<FileHttpCommand>(".*\\.\\w+", HttpMethod.GET)
                .SkipAuthenticationFetch();

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

            this.Services.AddScoped(typeof(ITemplatedViewProvider), viewProvider);
            this.Services.AddTransient<IHttpViewResponse, ViewResponse>();

            return this;
        }

        public IServerBuilder AddEncryption()
        {
            if (!this.Services.Any(x => x.ServiceType == typeof(IEncryptionProvider)))
            {
                this.Services.AddScoped<IEncryptionProvider, EncryptionProvider>();
            }

            return this;
        }

        public HttpCommandRegistration AddCommand<T>(string path, HttpMethod httpMethod) where T : HttpCommand
        {
            return this.CommandRegistry.RegisterCommand<T>(path, httpMethod);
        }

        public IEnumerable<HttpCommandRegistration> AddCommands(Assembly assembly)
        {
            IEnumerable<Type> commandTypes = assembly
                .GetTypes()
                .Where(x => x.GetCustomAttributes<CommandAttribute>().Any());

            List<HttpCommandRegistration> registrations = new List<HttpCommandRegistration>();
            foreach (Type commandType in commandTypes)
            {
                CommandAttribute commandAttribute = commandType.GetCustomAttribute<CommandAttribute>();

                if (commandAttribute == null)
                {
                    continue;
                }

                HttpCommandRegistration registration = this.CommandRegistry.RegisterCommand(commandType, commandAttribute.PathRegex, commandAttribute.Method);

                if (commandType.GetCustomAttributes<RequiresAntiforgeryAttribute>().Any())
                {
                    registration.RequireAntiforgeryToken();
                }

                if (commandType.GetCustomAttributes<RequiresAuthorizationAttribute>().Any())
                {
                    RequiresAuthorizationAttribute authAttribute = commandType.GetCustomAttribute<RequiresAuthorizationAttribute>();
                    registration.RequireAuthentication(authAttribute.Roles);
                }

                registrations.Add(registration);
            }

            return registrations;
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
            this.Services.AddKeyedScoped<IMiddleware, ResponseProcessorMiddleware>(ServerConstants.RESPONSE_PROCESSOR);

            this.Configuration = this.ConfigurationBuilder.Build();

            IConfigurationSection section = this.Configuration.GetSection("ServerConfiguration");

            this.Services.AddSingleton<IConfiguration>(this.Configuration);
            this.Services.Configure<ServerConfiguration>(section);

            this.Services.AddScoped<BadRequestResponse>();
            this.Services.AddScoped<UnauthorizedResponse>();
            this.Services.AddScoped<ForbiddenResponse>();
            this.Services.AddScoped<NotFoundResponse>();
            this.Services.AddScoped<InternalErrorResponse>();

            this.ServiceProvider = Services.BuildServiceProvider(true);

            return this.ServiceProvider.GetService<IServerRunner>();
        }

        public IServerBuilder AddRequestDataRetriever()
        {
            this.MiddlewareRegistry.AddMiddleware<CommandDataRetrieverMiddleware>();

            return this;
        }

        public IServerBuilder AddCommandRetriever()
        {
            this.MiddlewareRegistry.AddMiddleware<CommandRegistrationRetrieverMiddleware>();

            if (this.hasAuthentication)
            {
                this.Services.AddScoped(typeof(IAuthenticationProvider), this.authenticationProviderType);
                this.AddEncryption();
                this.MiddlewareRegistry.AddMiddleware<AuthenticatorMiddleware>();
            }

            this.MiddlewareRegistry.AddMiddleware<CommandValidationRetrieverMiddleware>();

            return this;
        }

        public IServerBuilder AddRequestSizeLimit()
        {
            this.MiddlewareRegistry.AddMiddleware<RequestSizeLimiterMiddleware>();

            return this;
        }

        public IServerBuilder AddCommandExecutor()
        {
            if (this.hasAntiforgeryTokens)
            {
                this.MiddlewareRegistry.AddMiddleware<AntiforgeryTokenValidationMiddleware>();
            }

            this.MiddlewareRegistry.AddMiddleware<CommandExecutorMiddleware>();
            this.MiddlewareRegistry.AddMiddleware<DefaultCommandResponseRedirectionMiddleware>();

            return this;
        }

        public IServerBuilder AddAntiforgeryTokens()
        {
            this.AddMiddleware<AntiforgeryTokenGenerationMiddleware>();
            this.hasAntiforgeryTokens = true;

            return this;
        }

        public IServerBuilder ReplaceDefaultResponses<T>(DefaultStatusCodes defaultStatus) where T : IHttpResponse
        {
            this.StandardResponseRegistry.ReplaceResponseInstance<T>(defaultStatus);
            this.Services.AddScoped(typeof(T));

            return this;
        }

        private void RegisterBaseIOCItems()
        {
            this.ConfigurationBuilder = new ConfigurationBuilder();
            this.ConfigurationBuilder.AddJsonFile("appsettings.json");
#if !DEBUG
            this.ConfigurationBuilder.AddJsonFile("appsettings.release.json", true);
#endif
            this.Services = new ServiceCollection();
            this.Services.AddSingleton<IServiceCollection>(Services);

            this.CommandRegistry = new CommandRegistry(Services);
            this.Services.AddSingleton<ICommandRegistry>(CommandRegistry);

            this.MiddlewareRegistry = new MiddlewareRegistry(Services);
            this.Services.AddSingleton<IMiddlewareRegistry>(MiddlewareRegistry);

            this.Services.AddSingleton<IServerRunner, ServerRunner>();

            this.Services.AddScoped<IHttpRequestExecutor, HttpRequestExecutor>();
            this.Services.AddScoped<IHttpRequestDataProvider, HttpRequestDataProvider>();

            this.Services.AddKeyedScoped<IRequestDataParser, UrlDataParser>("dataparser_urlDataParser");
            this.Services.AddKeyedScoped<IRequestDataParser, JsonDataParser>("dataparser_application/json");
            this.Services.AddKeyedScoped<IRequestDataParser, MultiPartFormDataParser>("dataparser_multipart/form-data");
            this.Services.AddKeyedScoped<IRequestDataParser, FormUrlEncoded>("dataparser_application/x-www-form-urlencoded");

            this.Services.AddScoped<ILogger, ConsoleLogger>();
            this.MiddlewareRegistry.AddMiddleware<LoggerMiddleware>();

            this.StandardResponseRegistry = new StandardResponseRegistry();
            this.Services.AddScoped<IStandardResponseRegistry>((_) => StandardResponseRegistry);

            this.Services.AddScoped<ICommandContext, CommandContext>();
        }
    }
}
