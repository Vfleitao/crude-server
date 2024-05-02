using System.Security.Principal;
using System.Threading.Tasks;

using CrudeServer.Middleware;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

using Moq;

namespace CrudeServer.Lib.Tests.Middleware
{
    public class AuthenticatorMiddlewareTests
    {
        [Test]
        public async Task AuthenticationProviderReturnsIPrincipalFromHeaders_SetsUser()
        {
            // Arrange
            Mock<IAuthenticationProvider> authenticationProvider = new Mock<IAuthenticationProvider>();
            authenticationProvider
                .Setup(ap => ap.GetUserFromHeaders(It.IsAny<ICommandContext>()))
                .ReturnsAsync(new Mock<IPrincipal>().Object);

            Mock<ICommandContext> requestContext = new Mock<ICommandContext>();

            AuthenticatorMiddleware authenticator = new AuthenticatorMiddleware(authenticationProvider.Object);

            // Act
            await authenticator.Process(requestContext.Object, () => Task.CompletedTask);

            // Assert
            authenticationProvider.Verify(ap => ap.GetUserFromHeaders(It.IsAny<ICommandContext>()), Times.Once);
            requestContext.VerifySet(rc => rc.User = It.IsAny<IPrincipal>(), Times.Once);
        }

        [Test]
        public async Task AuthenticationProviderReturnsIPrincipalFromCookies_SetsUser()
        {
            // Arrange
            Mock<IAuthenticationProvider> authenticationProvider = new Mock<IAuthenticationProvider>();
            authenticationProvider
               .Setup(ap => ap.GetUserFromHeaders(It.IsAny<ICommandContext>()))
               .ReturnsAsync((IPrincipal)null);
            authenticationProvider
                .Setup(ap => ap.GetUserFromCookies(It.IsAny<ICommandContext>()))
                .ReturnsAsync(new Mock<IPrincipal>().Object);

            Mock<ICommandContext> requestContext = new Mock<ICommandContext>();

            AuthenticatorMiddleware authenticator = new AuthenticatorMiddleware(authenticationProvider.Object);

            // Act
            await authenticator.Process(requestContext.Object, () => Task.CompletedTask);

            // Assert
            authenticationProvider.Verify(ap => ap.GetUserFromHeaders(It.IsAny<ICommandContext>()), Times.Once);
            requestContext.VerifySet(rc => rc.User = It.IsAny<IPrincipal>(), Times.Once);
        }

        [Test]
        public async Task AuthenticationProviderDoesNotHaveIPrincipal_DoesNotSetUser()
        {
            // Arrange
            Mock<IAuthenticationProvider> authenticationProvider = new Mock<IAuthenticationProvider>();
            authenticationProvider
                .Setup(ap => ap.GetUserFromHeaders(It.IsAny<ICommandContext>()))
                .ReturnsAsync((IPrincipal)null);

            authenticationProvider
                .Setup(ap => ap.GetUserFromCookies(It.IsAny<ICommandContext>()))
                .ReturnsAsync((IPrincipal)null);

            Mock<ICommandContext> requestContext = new Mock<ICommandContext>();

            AuthenticatorMiddleware authenticator = new AuthenticatorMiddleware(authenticationProvider.Object);

            // Act
            await authenticator.Process(requestContext.Object, () => Task.CompletedTask);

            // Assert
            authenticationProvider.Verify(ap => ap.GetUserFromHeaders(It.IsAny<ICommandContext>()), Times.Once);
            authenticationProvider.Verify(ap => ap.GetUserFromCookies(It.IsAny<ICommandContext>()), Times.Once);

            requestContext.VerifySet(rc => rc.User = null, Times.Once);
        }
    }
}
