using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

using CrudeServer.Enums;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Middleware;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;

using Moq;

namespace CrudeServer.Lib.Tests.Middleware
{
    public class AntiforgeryTokenValidationMiddlewareTests
    {
        [Test]
        public void NoHttpRegistration_ThrowsException()
        {
            // Arrange

            AntiforgeryTokenValidationMiddleware antiforgeryTokenMiddleware = new AntiforgeryTokenValidationMiddleware(
                null
            );

            // Act
            // Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await antiforgeryTokenMiddleware.Process(Mock.Of<ICommandContext>(), () => Task.CompletedTask);
            });
        }

        [TestCase(HttpMethod.DELETE)]
        [TestCase(HttpMethod.PUT)]
        [TestCase(HttpMethod.POST)]
        public async Task PostRequestDoesNotHaveInput_ReturnsBadRequest(HttpMethod httpMethod)
        {
            // Arrange
            Mock<IServerConfig> serverConfig = new Mock<IServerConfig>();
            serverConfig
                .Setup(serverConfig => serverConfig.AntiforgeryTokenInputName)
                .Returns("antiforgery");

            Mock<ICommandContext> context = new Mock<ICommandContext>();
            context
                .Setup(context => context.HttpRegistration)
                .Returns(new HttpCommandRegistration() {
                    RequiresAntiforgeryToken = true
                });

            context.Setup(context => context.RequestHttpMethod)
                .Returns(httpMethod);

            context
                .Setup(x => x.Items)
                .Returns(new Dictionary<string, object>());

            AntiforgeryTokenValidationMiddleware antiforgeryTokenMiddleware = new AntiforgeryTokenValidationMiddleware(
                serverConfig.Object
            );

            bool nextCalled = false;

            // Act
            await antiforgeryTokenMiddleware.Process(context.Object, () =>
            {
                nextCalled = true
                ;
                return Task.CompletedTask;
            });

            // Assert
            Assert.That(nextCalled, Is.False);
            context.VerifySet(x => x.Response = It.IsAny<BadRequestResponse>(), Times.Once);
        }

        [TestCase(HttpMethod.DELETE)]
        [TestCase(HttpMethod.PUT)]
        [TestCase(HttpMethod.POST)]
        public async Task PostRequestDoesNotHaveCookie_ReturnsBadRequest(HttpMethod httpMethod)
        {
            // Arrange
            Mock<IServerConfig> serverConfig = new Mock<IServerConfig>();
            serverConfig
                .Setup(serverConfig => serverConfig.AntiforgeryTokenInputName)
                .Returns("antiforgery");

            serverConfig
                .Setup(serverConfig => serverConfig.AntiforgeryTokenCookieName)
                .Returns("antiforgery_cookie");

            Mock<ICommandContext> context = new Mock<ICommandContext>();
            context
                .Setup(context => context.HttpRegistration)
                .Returns(new HttpCommandRegistration()
                {
                    RequiresAntiforgeryToken = true
                });

            context.Setup(context => context.RequestHttpMethod)
                .Returns(httpMethod);

            context
                .Setup(x => x.Items)
                .Returns(new Dictionary<string, object>() {
                    {"antiforgery", "test1234" }
                });

            AntiforgeryTokenValidationMiddleware antiforgeryTokenMiddleware = new AntiforgeryTokenValidationMiddleware(
                serverConfig.Object
            );

            bool nextCalled = false;

            // Act
            await antiforgeryTokenMiddleware.Process(context.Object, () =>
            {
                nextCalled = true
                ;
                return Task.CompletedTask;
            });

            // Assert
            Assert.That(nextCalled, Is.False);
            context.VerifySet(x => x.Response = It.IsAny<BadRequestResponse>(), Times.Once);
        }

        [TestCase(HttpMethod.DELETE)]
        [TestCase(HttpMethod.PUT)]
        [TestCase(HttpMethod.POST)]
        public async Task CookieAndInputDoNotMatch_ReturnsBadRequest(HttpMethod httpMethod)
        {
            // Arrange
            Mock<IServerConfig> serverConfig = new Mock<IServerConfig>();
            serverConfig
                .Setup(serverConfig => serverConfig.AntiforgeryTokenInputName)
                .Returns("antiforgery");

            serverConfig
                .Setup(serverConfig => serverConfig.AntiforgeryTokenCookieName)
                .Returns("antiforgery_cookie");

            Mock<ICommandContext> context = new Mock<ICommandContext>();
            context
                .Setup(context => context.HttpRegistration)
                .Returns(new HttpCommandRegistration()
                {
                    RequiresAntiforgeryToken = true
                });

            context.Setup(context => context.RequestHttpMethod)
                .Returns(httpMethod);

            context
                .Setup(x => x.Items)
                .Returns(new Dictionary<string, object>() {
                    {"antiforgery", "test1234" }
                });

            context
                .Setup(x => x.RequestCookies)
                .Returns(new List<HttpCookie>() {
                    new HttpCookie(){
                        Name = "antiforgery_cookie",
                        Value = "test12345"
                    }
                });

            AntiforgeryTokenValidationMiddleware antiforgeryTokenMiddleware = new AntiforgeryTokenValidationMiddleware(
                serverConfig.Object
            );

            bool nextCalled = false;

            // Act
            await antiforgeryTokenMiddleware.Process(context.Object, () =>
            {
                nextCalled = true
                ;
                return Task.CompletedTask;
            });

            // Assert
            Assert.That(nextCalled, Is.False);
            context.VerifySet(x => x.Response = It.IsAny<BadRequestResponse>(), Times.Once);
        }

        [TestCase(HttpMethod.DELETE)]
        [TestCase(HttpMethod.PUT)]
        [TestCase(HttpMethod.POST)]
        public async Task CookieAndInputDoMatch_ExecutedNext(HttpMethod httpMethod)
        {
            // Arrange
            Mock<IServerConfig> serverConfig = new Mock<IServerConfig>();
            serverConfig
                .Setup(serverConfig => serverConfig.AntiforgeryTokenInputName)
                .Returns("antiforgery");

            serverConfig
                .Setup(serverConfig => serverConfig.AntiforgeryTokenCookieName)
                .Returns("antiforgery_cookie");

            Mock<ICommandContext> context = new Mock<ICommandContext>();
            context
                .Setup(context => context.HttpRegistration)
                .Returns(new HttpCommandRegistration()
                {
                    RequiresAntiforgeryToken = true
                });

            context.Setup(context => context.RequestHttpMethod)
                .Returns(httpMethod);

            context
                .Setup(x => x.Items)
                .Returns(new Dictionary<string, object>() {
                    {"antiforgery", "test12345" }
                });

            context
                .Setup(x => x.RequestCookies)
                .Returns(new List<HttpCookie>() {
                    new HttpCookie(){
                        Name = "antiforgery_cookie",
                        Value = "test12345"
                    }
                });

            AntiforgeryTokenValidationMiddleware antiforgeryTokenMiddleware = new AntiforgeryTokenValidationMiddleware(
                serverConfig.Object
            );

            bool nextCalled = false;

            // Act
            await antiforgeryTokenMiddleware.Process(context.Object, () =>
            {
                nextCalled = true
                ;
                return Task.CompletedTask;
            });

            // Assert
            Assert.That(nextCalled, Is.True);
        }
    }
}
