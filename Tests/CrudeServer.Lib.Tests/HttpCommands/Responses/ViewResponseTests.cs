using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CrudeServer.HttpCommands.Responses;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers.Contracts;

using Moq;

namespace CrudeServer.Lib.Tests.HttpCommands.Responses
{
    public class ViewResponseTests
    {
        [Test]
        public void TemplateReturnsEmpty_ThrowsException()
        {
            // Arrange
            Mock<ITemplatedViewProvider> mockTemplatedViewProvider = new Mock<ITemplatedViewProvider>();
            mockTemplatedViewProvider.Setup(x => x.GetTemplate(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<ICommandContext>())).ReturnsAsync(string.Empty);

            ViewResponse viewResponse = new ViewResponse(mockTemplatedViewProvider.Object);
            viewResponse.SetTemplatePath("emptyTemplate");
            viewResponse.ViewModel = new { };
            viewResponse.Items = new Dictionary<string, object>();

            // Act
            // Assert
            InvalidOperationException exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await viewResponse.ProcessResponse());
            Assert.That(exception.Message, Is.EqualTo("Could not parse template emptyTemplate"));
        }

        [Test]
        public async Task TemplateReturnsData_SetsToResponseData()
        {
            // Arrange
            Mock<ITemplatedViewProvider> mockTemplatedViewProvider = new Mock<ITemplatedViewProvider>();
            mockTemplatedViewProvider
                .Setup(x => x.GetTemplate(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<ICommandContext>()))
                .ReturnsAsync("Hello world!");

            ViewResponse viewResponse = new ViewResponse(mockTemplatedViewProvider.Object);
            viewResponse.SetTemplatePath("emptyTemplate");
            viewResponse.ViewModel = new { };
            viewResponse.Items = new Dictionary<string, object>();

            // Act
            await viewResponse.ProcessResponse();

            // Assert
            Assert.That(
                viewResponse.ResponseData, 
                Is.EqualTo(System.Text.Encoding.UTF8.GetBytes("Hello world!"))
            );
        }
    }
}
