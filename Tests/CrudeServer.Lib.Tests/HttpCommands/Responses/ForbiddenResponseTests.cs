using CrudeServer.HttpCommands.Responses;

namespace CrudeServer.Lib.Tests.HttpCommands.Responses
{
    public class ForbiddenResponseTests
    {
        [Test]
        public void StatusCode_Returns403()
        {
            // Arrange
            ForbiddenResponse unauthorizedResponse = new ForbiddenResponse();

            // Act
            int statusCode = unauthorizedResponse.StatusCode;

            // Assert
            Assert.That(statusCode, Is.EqualTo(403));
        }
    }
}
