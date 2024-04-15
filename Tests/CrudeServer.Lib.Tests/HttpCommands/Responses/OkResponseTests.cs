using CrudeServer.HttpCommands.Responses;

using NUnit.Framework.Internal;

namespace CrudeServer.Lib.Tests.HttpCommands.Responses
{
    public class OkResponseTests
    {
        [Test]
        public void StatusCode_Returns200()
        {
            // Arrange
            OkResponse okResponse = new OkResponse();

            // Act
            int statusCode = okResponse.StatusCode;

            // Assert
            Assert.That(statusCode, Is.EqualTo(200));
        }

        [Test]
        public void DataCanBeSetByValueType()
        {
            // Arrange
            OkResponse okResponse = new OkResponse();

            // Act
            okResponse.SetData("data");
            int statusCode = okResponse.StatusCode;

            // Assert
            Assert.That(statusCode, Is.EqualTo(200));
            Assert.That(
            okResponse.ResponseData,
               Is.EqualTo(System.Text.Encoding.UTF8.GetBytes("data"))
           );
        }

        [Test]
        public void DataCanBeSetByComplexType()
        {
            // Arrange
            OkResponse okResponse = new OkResponse();

            // Act
            okResponse.SetData(new int[] { 0, 1, 2 });
            int statusCode = okResponse.StatusCode;

            // Assert
            Assert.That(statusCode, Is.EqualTo(200));
            Assert.That(
            okResponse.ResponseData,
               Is.EqualTo(System.Text.Encoding.UTF8.GetBytes("[0,1,2]"))
           );
        }

        [Test]
        public void DataCanBeSetByComplexStream()
        {
            // Arrange
            OkResponse okResponse = new OkResponse();

            // Act
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(ms);
                sw.Write("data");
                sw.Flush();
                ms.Position = 0;

                okResponse.SetData(ms);
            }

            int statusCode = okResponse.StatusCode;

            // Assert
            Assert.That(statusCode, Is.EqualTo(200));
            Assert.That(
            okResponse.ResponseData,
               Is.EqualTo(System.Text.Encoding.UTF8.GetBytes("data"))
           );
        }
    }
}
