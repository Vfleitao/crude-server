using System.Collections.Generic;
using System.Threading.Tasks;

using CrudeServer.Middleware;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

using Moq;

namespace CrudeServer.Lib.Tests.Middleware
{
    public class CommandDataRetrieverMiddlewareTests
    {
        [Test]
        public async Task CanRetrieveAndSetData()
        {
            // Arrange
            Mock<IHttpRequestDataProvider> httpRequestDataProvider = new Mock<IHttpRequestDataProvider>();
            httpRequestDataProvider
                .Setup(x => x.GetDataFromRequest(It.IsAny<ICommandContext>()))
                .ReturnsAsync(new HttpRequestData()
                {
                    Data = new Dictionary<string, object>()
                    {
                        { "key1", "value1" },
                        { "key2", "value2" }
                    },
                    Files = new List<HttpFile>()
                    {
                        new HttpFile()
                        {
                            Name = "file1",
                            Content = new byte[] { 1, 2, 3 }
                        }
                    }
                });

            Mock<ICommandContext> commandContext = new Mock<ICommandContext>();
            Dictionary<string, object> data = new Dictionary<string, object>();
            commandContext.Setup(x => x.Items).Returns(data);

            List<HttpFile> files = new List<HttpFile>();
            commandContext.Setup(x => x.Files).Returns(files);

            CommandDataRetrieverMiddleware middleware = new CommandDataRetrieverMiddleware(httpRequestDataProvider.Object);
            // Act
            await middleware.Process(commandContext.Object, () => Task.CompletedTask);

            // Assert

            Assert.That(data.Count, Is.EqualTo(2));
            Assert.That(data["key1"], Is.EqualTo("value1"));
            Assert.That(data["key2"], Is.EqualTo("value2"));

            Assert.That(files.Count, Is.EqualTo(1));
            Assert.That(files[0].Name, Is.EqualTo("file1"));
            Assert.That(files[0].Content, Is.EqualTo(new byte[] { 1, 2, 3 }));
        }
    }
}
