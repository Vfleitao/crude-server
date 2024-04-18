using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.CommandRegistration;

using Microsoft.Extensions.DependencyInjection;
using CrudeServer.Lib.Tests.Mocks;
using System;
using CrudeServer.Enums;
using System.Threading.Tasks;
using CrudeServer.Models;
using System.Text.RegularExpressions;
using System.Linq;

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
        public void PathHasModel_ConfiguresResgistrationCorrectly()
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
            Assert.That(registeredModel.PathRegex.ToString(), Is.EqualTo("^/\\d+/\\w+$"));
            Assert.That(registeredModel.UrlParameters.Select(x=>x.Key), Contains.Item("id"));
            Assert.That(registeredModel.UrlParameters[0].Value, Is.EqualTo("\\d+"));
            Assert.That(registeredModel.UrlParameters.Select(x=>x.Key), Contains.Item("action"));
            Assert.That(registeredModel.UrlParameters[1].Value, Is.EqualTo("\\w+"));
        }
    }
}