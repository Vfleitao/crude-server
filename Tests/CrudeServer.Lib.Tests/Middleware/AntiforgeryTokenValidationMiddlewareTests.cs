using System.Collections.Generic;
using System.Threading.Tasks;

using CrudeServer.Enums;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Middleware;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;

using Microsoft.Extensions.Options;

using Moq;

namespace CrudeServer.Lib.Tests.Middleware
{
    public class AntiforgeryTokenValidationMiddlewareTests
    {
        [TestCase(HttpMethod.DELETE)]
        [TestCase(HttpMethod.PUT)]
        [TestCase(HttpMethod.POST)]
        public async Task PostRequestDoesNotHaveInput_ReturnsBadRequest(HttpMethod httpMethod)
        {
            // Arrange
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
                .Returns(new Dictionary<string, object>());

            Mock<IOptions<ServerConfiguration>> options = new Mock<IOptions<ServerConfiguration>>();
            options
                .Setup(options => options.Value)
                .Returns(new ServerConfiguration() { AntiforgeryTokenInputName = "antiforgery" });

            AntiforgeryTokenValidationMiddleware antiforgeryTokenMiddleware = new AntiforgeryTokenValidationMiddleware(
                options.Object
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


            Mock<IOptions<ServerConfiguration>> options = new Mock<IOptions<ServerConfiguration>>();
            options
                .Setup(options => options.Value)
                .Returns(new ServerConfiguration() {
                    AntiforgeryTokenInputName = "antiforgery",
                    AntiforgeryTokenCookieName = "antiforgery_cookie"
                });

            AntiforgeryTokenValidationMiddleware antiforgeryTokenMiddleware = new AntiforgeryTokenValidationMiddleware(
                options.Object
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

            Mock<IOptions<ServerConfiguration>> options = new Mock<IOptions<ServerConfiguration>>();
            options
                .Setup(options => options.Value)
                .Returns(new ServerConfiguration() {
                    AntiforgeryTokenCookieName = "antiforgery_cookie",
                    AntiforgeryTokenInputName = "antiforgery"
                });

            AntiforgeryTokenValidationMiddleware antiforgeryTokenMiddleware = new AntiforgeryTokenValidationMiddleware(
                options.Object
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


            Mock<IOptions<ServerConfiguration>> options = new Mock<IOptions<ServerConfiguration>>();
            options
                .Setup(options => options.Value)
                .Returns(new ServerConfiguration()
                {
                    AntiforgeryTokenCookieName = "antiforgery_cookie",
                    AntiforgeryTokenInputName = "antiforgery"
                });

            AntiforgeryTokenValidationMiddleware antiforgeryTokenMiddleware = new AntiforgeryTokenValidationMiddleware(
                options.Object
            );

            bool nextCalled = false;

            // Act
            await antiforgeryTokenMiddleware.Process(context.Object, () =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            });

            // Assert
            Assert.That(nextCalled, Is.True);
        }
    }
}
