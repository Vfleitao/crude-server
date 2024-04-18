using System;
using System.Threading.Tasks;
using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Models.Contracts;
using Moq;

namespace CrudeServer.Lib.Tests.HttpCommands
{
    public class FileHttpCommandTests
    {
        [Test]
        public async Task FilesDoesNotExist_Returns404()
        {
            // Arrange
            FileHttpCommand fileHttpCommand = new FileHttpCommand(
                GetType().Assembly,
                "files"
            );

            Mock<ICommandContext> requestContext = new Mock<ICommandContext>();
            requestContext.SetupGet(rc => rc.RequestUrl).Returns(new Uri("http://localhost:8080/files/doesnotexist.txt"));

            fileHttpCommand.SetContext(requestContext.Object);

            // Act
            IHttpResponse response = await fileHttpCommand.ExecuteRequest();

            // Assert
            Assert.That(response, Is.InstanceOf<NotFoundResponse>());
            Assert.That(response.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task FilesExists_Returns200WithData()
        {
            // Arrange
            FileHttpCommand fileHttpCommand = new FileHttpCommand(
                GetType().Assembly,
                "files"
            );

            Mock<ICommandContext> requestContext = new Mock<ICommandContext>();
            requestContext.SetupGet(rc => rc.RequestUrl).Returns(new Uri("http://localhost:8080/demo.json"));

            fileHttpCommand.SetContext(requestContext.Object);

            // Act
            IHttpResponse response = await fileHttpCommand.ExecuteRequest();

            // Assert
            Assert.That(response, Is.InstanceOf<OkResponse>());
            Assert.That(response.StatusCode, Is.EqualTo(200));
            Assert.That(response.ResponseData, Is.Not.Null);
            Assert.That(response.ResponseData.Length, Is.GreaterThan(0));
            Assert.That(response.ContentType, Is.EqualTo("application/json"));
        }
    }
}
