using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.CommandRegistration;
using CrudeServer.Lib.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using CrudeServer.HttpCommands;
using System.Net;

namespace CrudeServer.Lib.Tests
{
    public class FileHttpCommandTests
    {
        [Test]
        public void FilesDoesNotExist_Returns404()
        {
            // Arrange
            FileHttpCommand fileHttpCommand = new FileHttpCommand(
                this.GetType().Assembly,
                "files"
            );

            //fileHttpCommand.SetContext(new HttpListenerContext());

            // Act

            // Assert
        }
    }
}
