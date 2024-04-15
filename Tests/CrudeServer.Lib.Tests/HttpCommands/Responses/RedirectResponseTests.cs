using CrudeServer.HttpCommands.Responses;

using NUnit.Framework.Internal;

namespace CrudeServer.Lib.Tests.HttpCommands.Responses
{
    public class RedirectResponseTests
    {
        [Test]
        public void StatusCode_Returns302()
        {
            // Arrange
            RedirectResponse redirectResponse = new RedirectResponse("test");

            // Act
            int statusCode = redirectResponse.StatusCode;

            // Assert
            Assert.That(statusCode, Is.EqualTo(302));
            Assert.That(
            redirectResponse.ResponseData,
               Is.EqualTo(System.Text.Encoding.UTF8.GetBytes("test"))
           );
        }

        [Test]
        public void StatusCode_Returns301()
        {
            // Arrange
            RedirectResponse redirectResponse = new RedirectResponse("test", 301);

            // Act
            int statusCode = redirectResponse.StatusCode;

            // Assert
            Assert.That(statusCode, Is.EqualTo(301));
            Assert.That(
            redirectResponse.ResponseData,
               Is.EqualTo(System.Text.Encoding.UTF8.GetBytes("test"))
           );
        }
    }
}
