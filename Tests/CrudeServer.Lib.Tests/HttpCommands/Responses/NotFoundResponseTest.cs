using CrudeServer.HttpCommands.Responses;

using NUnit.Framework.Internal;

namespace CrudeServer.Lib.Tests.HttpCommands.Responses
{
    public class NotFoundResponseTests
    {
        [Test]
        public void StatusCode_Returns404()
        {
            // Arrange
            NotFoundResponse notFoundResponse = new NotFoundResponse();

            // Act
            int statusCode = notFoundResponse.StatusCode;

            // Assert
            Assert.That(statusCode, Is.EqualTo(404));
        }
    }
}
