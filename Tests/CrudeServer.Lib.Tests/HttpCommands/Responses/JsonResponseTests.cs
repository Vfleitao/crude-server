using CrudeServer.HttpCommands.Responses;

using NUnit.Framework.Internal;

namespace CrudeServer.Lib.Tests.HttpCommands.Responses
{
    public class JsonResponseTests
    {
        [Test]
        public void StatusCode_Returns200()
        {
            // Arrange
            JsonResponse JsonResponse = new JsonResponse();

            // Act
            int statusCode = JsonResponse.StatusCode;

            // Assert
            Assert.That(statusCode, Is.EqualTo(200));
            Assert.That(JsonResponse.ContentType, Is.EqualTo("application/json"));
        }

        [Test]
        public void DataCanBeSetByValueType()
        {
            // Arrange
            JsonResponse JsonResponse = new JsonResponse();

            // Act
            JsonResponse.SetData("data");
            int statusCode = JsonResponse.StatusCode;

            // Assert
            Assert.That(statusCode, Is.EqualTo(200));
            Assert.That(
                JsonResponse.ResponseData,
                Is.EqualTo(System.Text.Encoding.UTF8.GetBytes("data"))
            );
            Assert.That(JsonResponse.ContentType, Is.EqualTo("application/json"));
        }

        [Test]
        public void DataCanBeSetByComplexType()
        {
            // Arrange
            JsonResponse JsonResponse = new JsonResponse();

            // Act
            JsonResponse.SetData(new int[] { 0, 1, 2 });
            int statusCode = JsonResponse.StatusCode;

            // Assert
            Assert.That(statusCode, Is.EqualTo(200));
            Assert.That(
                JsonResponse.ResponseData,
                Is.EqualTo(System.Text.Encoding.UTF8.GetBytes("[0,1,2]"))
            );
            Assert.That(JsonResponse.ContentType, Is.EqualTo("application/json"));
        }

        [Test]
        public void DataCanBeSetByComplexStream()
        {
            // Arrange
            JsonResponse JsonResponse = new JsonResponse();

            // Act
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(ms);
                sw.Write("data");
                sw.Flush();
                ms.Position = 0;

                JsonResponse.SetData(ms);
            }

            int statusCode = JsonResponse.StatusCode;

            // Assert
            Assert.That(statusCode, Is.EqualTo(200));
            Assert.That(
                JsonResponse.ResponseData,
                Is.EqualTo(System.Text.Encoding.UTF8.GetBytes("data"))
            );
            Assert.That(JsonResponse.ContentType, Is.EqualTo("application/json"));
        }
    }
}
