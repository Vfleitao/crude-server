using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

using CrudeServer.Enums;
using CrudeServer.Middleware;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;

using Moq;

namespace CrudeServer.Lib.Tests.Middleware
{
    public class AntiforgeryTokenGenerationMiddlewareTests
    {

        [TestCase(HttpMethod.OPTIONS)]
        [TestCase(HttpMethod.GET)]
        [TestCase(HttpMethod.HEAD)]
        public async Task RequestIsGetAndCookieDoesNotExist_SetsCookie(HttpMethod httpMethod) {
            // Arrange
            Mock<ICommandContext> context = new Mock<ICommandContext>();
            context
                .Setup(context => context.HttpRegistration)
                .Returns(new HttpCommandRegistration());
            context
                .Setup(context => context.RequestHttpMethod)
                .Returns(httpMethod);

            IList<HttpCookie> cookies = new List<HttpCookie>();
            context
                .Setup(context => context.RequestCookies)
                .Returns(cookies);
            context
                .Setup(context => context.ResponseCookies)
                .Returns(cookies);

            Mock<IServerConfig> serverConfig = new Mock<IServerConfig>();
            serverConfig
                .Setup(serverConfig => serverConfig.AntiforgeryTokenCookieName)
                .Returns("antiforgery");

            AntiforgeryTokenGenerationMiddleware antiforgeryTokenMiddleware = new AntiforgeryTokenGenerationMiddleware(
                serverConfig.Object
            );

            // Act
            await antiforgeryTokenMiddleware.Process(context.Object, () => Task.CompletedTask);

            // Assert
            Assert.That(cookies.Count, Is.EqualTo(1));
            Assert.That(cookies[0].Name, Is.EqualTo("antiforgery"));
        }

        [TestCase(HttpMethod.OPTIONS)]
        [TestCase(HttpMethod.GET)]
        [TestCase(HttpMethod.HEAD)]
        public async Task RequestIsGetAndCookieExists_DoesNotSetsCookie(HttpMethod httpMethod)
        {
            // Arrange
            Mock<ICommandContext> context = new Mock<ICommandContext>();
            context
                .Setup(context => context.HttpRegistration)
                .Returns(new HttpCommandRegistration());
            context
                .Setup(context => context.RequestHttpMethod)
                .Returns(httpMethod);

            IList<HttpCookie> cookies = new List<HttpCookie>();
            cookies.Add(new HttpCookie() { Name = "antiforgery", Value = "test1234" });

            context
                .Setup(context => context.RequestCookies)
                .Returns(cookies);
            context
                .Setup(context => context.ResponseCookies)
                .Returns(cookies);

            Mock<IServerConfig> serverConfig = new Mock<IServerConfig>();
            serverConfig
                .Setup(serverConfig => serverConfig.AntiforgeryTokenCookieName)
                .Returns("antiforgery");

            AntiforgeryTokenGenerationMiddleware antiforgeryTokenMiddleware = new AntiforgeryTokenGenerationMiddleware(
                serverConfig.Object
            );

            // Act
            await antiforgeryTokenMiddleware.Process(context.Object, () => Task.CompletedTask);

            // Assert
            Assert.That(cookies.Count, Is.EqualTo(1));
            Assert.That(cookies[0].Name, Is.EqualTo("antiforgery"));
            Assert.That(cookies[0].Value, Is.EqualTo("test1234"));
        }

        [Test]
        public void NoHttpRegistration_ThrowsException()
        {
            // Arrange

            AntiforgeryTokenGenerationMiddleware antiforgeryTokenMiddleware = new AntiforgeryTokenGenerationMiddleware(
                null
            );

            // Act
            // Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await antiforgeryTokenMiddleware.Process(Mock.Of<ICommandContext>(), () => Task.CompletedTask);
            });
        }

    }
}
