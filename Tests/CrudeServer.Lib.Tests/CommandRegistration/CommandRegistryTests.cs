using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.CommandRegistration;

using Microsoft.Extensions.DependencyInjection;
using CrudeServer.Lib.Tests.Mocks;
using System;
using CrudeServer.Enums;

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
    }
}