using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Enums;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Lib.Tests.Mocks;
using CrudeServer.Middleware;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers;
using CrudeServer.Providers.Contracts;
using CrudeServer.Providers.DataParser;

using Microsoft.Extensions.DependencyInjection;

using Moq;

namespace CrudeServer.Lib.Tests.Middleware
{
    public class CommandExecutorMiddlewareTests
    {
        [Test]
        public async Task NoCommandInRegistry_ReturnsNotFound()
        {
            // Arrange
            Mock<ICommandRegistry> commandRegistry = new Mock<ICommandRegistry>();
            commandRegistry
                .Setup(x => x.GetCommand(It.IsAny<string>(), It.IsAny<HttpMethod>()))
                .Returns((HttpCommandRegistration)null);

            CommandExecutorMiddleware middleware = new CommandExecutorMiddleware(
                commandRegistry.Object,
                null,
                null,
                Mock.Of<ILoggerProvider>()
            );

            Mock<ICommandContext> requestContext = new Mock<ICommandContext>();
            requestContext
                .Setup(x => x.RequestUrl)
                .Returns(new System.Uri("http://localhost:8080/"));

            requestContext
                .Setup(x => x.RequestHttpMethod)
                .Returns(HttpMethod.POST);

            // Act
            await middleware.Process(requestContext.Object, () => Task.CompletedTask);

            // Assert
            requestContext.VerifySet(x => x.Response = It.Is<IHttpResponse>(r => r is NotFoundResponse));
        }

        [Test]
        public async Task EndpointIsSecureAndUserIsNotSet_ReturnsUnauthorized()
        {
            // Arrange
            Mock<ICommandRegistry> commandRegistry = new Mock<ICommandRegistry>();
            commandRegistry
                .Setup(x => x.GetCommand(It.IsAny<string>(), It.IsAny<HttpMethod>()))
                .Returns(new HttpCommandRegistration()
                {
                    RequiresAuthentication = true
                });

            CommandExecutorMiddleware middleware = new CommandExecutorMiddleware(
                commandRegistry.Object,
                null,
                null,
                Mock.Of<ILoggerProvider>()
            );

            Mock<ICommandContext> requestContext = new Mock<ICommandContext>();
            requestContext
                .Setup(x => x.RequestUrl)
                .Returns(new System.Uri("http://localhost:8080/"));

            requestContext
                .Setup(x => x.RequestHttpMethod)
                .Returns(HttpMethod.POST);

            // Act
            await middleware.Process(requestContext.Object, () => Task.CompletedTask);

            // Assert
            requestContext.VerifySet(x => x.Response = It.Is<IHttpResponse>(r => r is UnauthorizedResponse));
        }

        [Test]
        public async Task EndpointIsSecureAndUserNotAuthenticated_ReturnsUnauthorized()
        {
            // Arrange
            Mock<ICommandRegistry> commandRegistry = new Mock<ICommandRegistry>();
            commandRegistry
                .Setup(x => x.GetCommand(It.IsAny<string>(), It.IsAny<HttpMethod>()))
                .Returns(new HttpCommandRegistration()
                {
                    RequiresAuthentication = true,
                    AuthenticationRoles = new List<string>() { "Admin" }
                });

            CommandExecutorMiddleware middleware = new CommandExecutorMiddleware(
                commandRegistry.Object,
                null,
                null,
                Mock.Of<ILoggerProvider>()
            );

            Mock<ICommandContext> requestContext = new Mock<ICommandContext>();
            requestContext
                .Setup(x => x.RequestUrl)
                .Returns(new System.Uri("http://localhost:8080/"));

            requestContext
                .Setup(x => x.RequestHttpMethod)
                .Returns(HttpMethod.POST);

            Mock<IPrincipal> principal = CreateUserIdentity(false, false);

            requestContext
                .Setup(x => x.User)
                .Returns(principal.Object);

            // Act
            await middleware.Process(requestContext.Object, () => Task.CompletedTask);

            // Assert
            requestContext.VerifySet(x => x.Response = It.Is<IHttpResponse>(r => r is UnauthorizedResponse));
        }

        [Test]
        public async Task EndpointIsSecureAndUserRoleIsNotCorrect_ReturnsUnauthorized()
        {
            // Arrange
            Mock<ICommandRegistry> commandRegistry = new Mock<ICommandRegistry>();
            commandRegistry
                .Setup(x => x.GetCommand(It.IsAny<string>(), It.IsAny<HttpMethod>()))
                .Returns(new HttpCommandRegistration()
                {
                    RequiresAuthentication = true,
                    AuthenticationRoles = new List<string>() { "Admin" }
                });

            CommandExecutorMiddleware middleware = new CommandExecutorMiddleware(
                commandRegistry.Object,
                null,
                null,
                Mock.Of<ILoggerProvider>()
            );

            Mock<ICommandContext> requestContext = new Mock<ICommandContext>();
            requestContext
                .Setup(x => x.RequestUrl)
                .Returns(new System.Uri("http://localhost:8080/"));

            requestContext
                .Setup(x => x.RequestHttpMethod)
                .Returns(HttpMethod.POST);

            Mock<IPrincipal> principal = CreateUserIdentity(true, false);

            requestContext
                .Setup(x => x.User)
                .Returns(principal.Object);

            // Act
            await middleware.Process(requestContext.Object, () => Task.CompletedTask);

            // Assert
            requestContext.VerifySet(x => x.Response = It.Is<IHttpResponse>(r => r is UnauthorizedResponse));
        }

        [Test]
        public async Task EndpointCanBeExecuted_ReturnsResponse()
        {
            // Arrange
            Mock<ICommandRegistry> commandRegistry = new Mock<ICommandRegistry>();
            commandRegistry
                .Setup(x => x.GetCommand(It.IsAny<string>(), It.IsAny<HttpMethod>()))
                .Returns(new HttpCommandRegistration()
                {
                    RequiresAuthentication = true,
                    AuthenticationRoles = new List<string>() { "Admin" },
                    Command = typeof(MockCommand)
                });

            Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(new MockCommand());

            Mock<IHttpRequestDataProvider> dataParser = new Mock<IHttpRequestDataProvider>();
            dataParser
                .Setup(x => x.GetDataFromRequest(It.IsAny<ICommandContext>()))
                .Returns(Task.FromResult(new HttpRequestData()));

            CommandExecutorMiddleware middleware = new CommandExecutorMiddleware(
                commandRegistry.Object,
                serviceProviderMock.Object,
                dataParser.Object,
                Mock.Of<ILoggerProvider>()
            );

            Mock<ICommandContext> requestContext = new Mock<ICommandContext>();
            requestContext
                .Setup(x => x.RequestUrl)
                .Returns(new System.Uri("http://localhost:8080/"));

            requestContext
                .Setup(x => x.RequestHttpMethod)
                .Returns(HttpMethod.POST);

            Mock<IPrincipal> principal = CreateUserIdentity(true, true);

            requestContext
                .Setup(x => x.User)
                .Returns(principal.Object);

            // Act
            await middleware.Process(requestContext.Object, () => Task.CompletedTask);

            // Assert
            requestContext.VerifySet(x => x.Response = It.Is<IHttpResponse>(r => r is MockHttpResponse));
        }

        private Mock<IPrincipal> CreateUserIdentity(bool isAuthenticated, bool isInRole)
        {
            Mock<IIdentity> identity = new Mock<IIdentity>();
            identity
                .Setup(x => x.IsAuthenticated)
                .Returns(isAuthenticated);

            Mock<IPrincipal> principal = new Mock<IPrincipal>();
            principal
                .Setup(x => x.Identity)
                .Returns(identity.Object);

            principal
                .Setup(x => x.IsInRole(It.IsAny<string>()))
                .Returns(isInRole);

            return principal;
        }
    }
}
