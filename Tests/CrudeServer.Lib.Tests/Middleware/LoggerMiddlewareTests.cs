using System;
using System.Threading.Tasks;

using CrudeServer.Enums;
using CrudeServer.Middleware;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

using Moq;

namespace CrudeServer.Lib.Tests.Middleware
{
    public class LoggerMiddlewareTests
    {
        [Test]
        public async Task Process_ShouldLogRequestDetails()
        {
            // Arrange
            Mock<ILoggerProvider> loggerProviderMock = new Mock<ILoggerProvider>();

            Mock<IRequestContext> context = new Mock<IRequestContext>();
            context.Setup(x => x.RequestUrl).Returns(new Uri("http://localhost:8080"));
            context.Setup(x => x.RequestHttpMethod).Returns(HttpMethod.GET);
            context.Setup(x => x.RequestHost).Returns("localhost");
            context.Setup(x => x.UserAgent).Returns("Mozilla/5.0");

            Func<Task> next = () => Task.CompletedTask;

            LoggerMiddleware loggerMiddleware = new LoggerMiddleware(loggerProviderMock.Object);

            // Act
            await loggerMiddleware.Process(context.Object, next);

            // Assert
            loggerProviderMock.Verify(x => x.Log(It.IsAny<string>()), Times.Exactly(1));
            loggerProviderMock.Verify(x => x.Log(It.IsAny<string>(), It.IsAny<object[]>()), Times.Exactly(1));
        }
    }
}
