using CrudeServer.HttpCommands.Responses;

namespace CrudeServer.Lib.Tests.HttpCommands.Responses
{
    public class UnauthorizedResponseTests
    {
        [Test]
        public void StatusCode_Returns401()
        {
            // Arrange
            UnauthorizedResponse unauthorizedResponse = new UnauthorizedResponse();

            // Act
            int statusCode = unauthorizedResponse.StatusCode;

            // Assert
            Assert.That(statusCode, Is.EqualTo(401));
        }
    }
}
