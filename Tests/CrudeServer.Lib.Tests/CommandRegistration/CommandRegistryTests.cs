using System;
using System.Linq;
using System.Threading.Tasks;

using CrudeServer.CommandRegistration;
using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Enums;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Lib.Tests.Mocks;
using CrudeServer.Models;
using CrudeServer.Models.Contracts;

using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.Lib.Tests.CommandRegistration
{
    public class CommandRegistryTests
    {
        [Test]
        public void CanAddNewCommands()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            ICommandRegistry commandRegistry = new CommandRegistry(services);

            // Act
            commandRegistry.RegisterCommand<MockCommand>("/", HttpMethod.GET);
            commandRegistry.RegisterCommand<MockCommand>("/", HttpMethod.POST);

            // Assert
            Assert.That(commandRegistry.GetCommand("/", HttpMethod.GET), Is.Not.Null);

            Models.HttpCommandRegistration getRegistry = commandRegistry.GetCommand("/", HttpMethod.GET);
            Assert.That(getRegistry.Path, Is.EqualTo("/"));
            Assert.That(getRegistry.HttpMethod, Is.EqualTo(HttpMethod.GET));
            Assert.That(getRegistry.Command, Is.EqualTo(typeof(MockCommand)));

            Assert.That(commandRegistry.GetCommand("/", HttpMethod.POST), Is.Not.Null);

            Models.HttpCommandRegistration postRegistry = commandRegistry.GetCommand("/", HttpMethod.POST);
            Assert.That(postRegistry.Path, Is.EqualTo("/"));
            Assert.That(postRegistry.HttpMethod, Is.EqualTo(HttpMethod.POST));
            Assert.That(postRegistry.Command, Is.EqualTo(typeof(MockCommand)));
        }

        [Test]
        public void AddingMultipleTimesCommands_ThrowsException()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            ICommandRegistry commandRegistry = new CommandRegistry(services);

            // Act
            commandRegistry.RegisterCommand<MockCommand>("/", HttpMethod.GET);

            // Assert
            Assert.Throws<ArgumentException>(() => commandRegistry.RegisterCommand<MockCommand>("/", HttpMethod.GET));
        }

        [Test]
        public void PathHasModel_ConfiguresRegistrationCorrectly()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            ICommandRegistry commandRegistry = new CommandRegistry(services);

            // Act
            commandRegistry.RegisterCommand<MockCommand>("/{id:\\d+}/{action:\\w+}", HttpMethod.GET);

            // Assert
            HttpCommandRegistration registeredModel = commandRegistry.GetCommand("/9/test", HttpMethod.GET);
            Assert.That(registeredModel, Is.Not.Null);
            Assert.That(registeredModel.Path, Is.EqualTo("/{id:\\d+}/{action:\\w+}"));
            Assert.That(registeredModel.PathRegex.ToString(), Is.EqualTo("^/(\\d+)/(\\w+)$"));
            Assert.That(registeredModel.UrlParameters.Select(x => x.Key), Contains.Item("id"));
            Assert.That(registeredModel.UrlParameters[0].Value, Is.EqualTo("\\d+"));
            Assert.That(registeredModel.UrlParameters.Select(x => x.Key), Contains.Item("action"));
            Assert.That(registeredModel.UrlParameters[1].Value, Is.EqualTo("\\w+"));
        }

        [Test]
        public void PathHasComplexRegex_ConfiguresRegistrationCorrectly()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            ICommandRegistry commandRegistry = new CommandRegistry(services);

            // Act
            commandRegistry.RegisterCommand<MockCommand>("/{id:[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}}", HttpMethod.GET);

            // Assert
            HttpCommandRegistration registeredModel = commandRegistry.GetCommand("/60614ce3-191d-ef11-82c7-7404f1cc28cd", HttpMethod.GET);
            Assert.That(registeredModel, Is.Not.Null);
            Assert.That(registeredModel.Path, Is.EqualTo("/{id:[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}}"));
            Assert.That(registeredModel.PathRegex.ToString(), Is.EqualTo("^/([0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12})$"));
            Assert.That(registeredModel.UrlParameters.Select(x => x.Key), Contains.Item("id"));
            Assert.That(registeredModel.UrlParameters[0].Value, Is.EqualTo("[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}"));
        }

        [Test]
        public void CanRegisterCommandFunction()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            ICommandRegistry commandRegistry = new CommandRegistry(services);

            // Act
            commandRegistry.RegisterCommandFunction("/", HttpMethod.GET, (ICommandContext context) =>
            {
                return Task.FromResult<IHttpResponse>(new StatusCodeResponse()
                {
                    StatusCode = 200
                });
            });

            // Assert
            HttpCommandRegistration registeredModel = commandRegistry.GetCommand("/", HttpMethod.GET);
            Assert.That(registeredModel, Is.Not.Null);
            Assert.That(registeredModel.Path, Is.EqualTo("/"));
            Assert.That(registeredModel.PathRegex.ToString(), Is.EqualTo("^/$"));
        }
    }
}