using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.CommandRegistration;
using CrudeServer.Lib.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using System;
using CrudeServer.Middleware.Registration.Contracts;
using CrudeServer.Middleware.Registration;
using System.Collections.Generic;
using System.Linq;

namespace CrudeServer.Lib.Tests.Middleware.Registry
{
    public class MiddlewareRegistryTests
    {
        [Test]
        public void CanAddNewMiddlewares()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IMiddlewareRegistry commandRegistry = new MiddlewareRegistry(services);

            // Act
            commandRegistry.AddMiddleware<MockMiddleware>();

            // Assert
            IEnumerable<Type> middlewares = commandRegistry.GetMiddlewares();

            Assert.That(middlewares, Is.Not.Null);
            Assert.That(middlewares.Count(), Is.EqualTo(1));
            Assert.That(middlewares.First(), Is.EqualTo(typeof(MockMiddleware)));
        }

        [Test]
        public void AddingMultipleTimesCommands_ThrowsException()
        {
            // Arrange
            IServiceCollection services = new ServiceCollection();
            IMiddlewareRegistry commandRegistry = new MiddlewareRegistry(services);

            // Act
            commandRegistry.AddMiddleware<MockMiddleware>();

            // Assert
            Assert.Throws<InvalidOperationException>(() => commandRegistry.AddMiddleware<MockMiddleware>());
        }
    }
}
