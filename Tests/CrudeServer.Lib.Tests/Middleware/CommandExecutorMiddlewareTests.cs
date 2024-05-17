using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading.Tasks;

using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Enums;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.Lib.Tests.Mocks;
using CrudeServer.Middleware;
using CrudeServer.Models;
using CrudeServer.Models.Authentication;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

using Moq;

namespace CrudeServer.Lib.Tests.Middleware
{
    public class CommandExecutorMiddlewareTests
    {
        [Test]
        public async Task EndpointCanBeExecuted_ReturnsResponse()
        {
            // Arrange
            Mock<IServiceProvider> serviceProviderMock = new Mock<IServiceProvider>();
            serviceProviderMock
                .Setup(x => x.GetService(It.IsAny<Type>()))
                .Returns(new MockCommand(Mock.Of<ICommandContext>()));

            CommandExecutorMiddleware middleware = new CommandExecutorMiddleware(
                serviceProviderMock.Object,
                Mock.Of<ILogger>()
            );

            Mock<ICommandContext> requestContext = new Mock<ICommandContext>();
            requestContext
                .Setup(x => x.RequestUrl)
                .Returns(new System.Uri("http://localhost:8080/"));

            requestContext.Setup(x => x.HttpRegistration)
                .Returns(new HttpCommandRegistration()
                {
                    RequiresAuthentication = true,
                    AuthenticationRoles = new List<string>() { "Admin" },
                    Command = typeof(MockCommand)
                });

            requestContext
                .Setup(x => x.RequestHttpMethod)
                .Returns(HttpMethod.POST);

            Mock<IPrincipal> principal = CreateUserIdentity(true, true);

            requestContext
                .Setup(x => x.User)
                .Returns(new UserWrapper(principal.Object));

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
