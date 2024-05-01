using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;

using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Enums;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Middleware;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

using Moq;

namespace CrudeServer.Lib.Tests.Middleware
{
    public class CommandRetrieverMiddlewareTests
    {
        [Test]
        public async Task NoCommandInRegistry_ReturnsNotFound()
        {
            // Arrange
            Mock<ICommandRegistry> commandRegistry = new Mock<ICommandRegistry>();
            commandRegistry
                .Setup(x => x.GetCommand(It.IsAny<string>(), It.IsAny<HttpMethod>()))
                .Returns((HttpCommandRegistration)null);

            CommandRetrieverMiddleware middleware = new CommandRetrieverMiddleware(
                commandRegistry.Object,
                Mock.Of<ILogger>()
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

            CommandRetrieverMiddleware middleware = new CommandRetrieverMiddleware(
                commandRegistry.Object,
                Mock.Of<ILogger>()
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

            CommandRetrieverMiddleware middleware = new CommandRetrieverMiddleware(
                commandRegistry.Object,
                Mock.Of<ILogger>()
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

            CommandRetrieverMiddleware middleware = new CommandRetrieverMiddleware(
                commandRegistry.Object,
                Mock.Of<ILogger>()
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
