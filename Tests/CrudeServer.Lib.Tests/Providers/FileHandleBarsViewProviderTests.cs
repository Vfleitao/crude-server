using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

using CrudeServer.Models;
using CrudeServer.Models.Contracts;
using CrudeServer.Providers;

using Microsoft.Extensions.Options;

using Moq;

namespace CrudeServer.Lib.Tests.Providers
{
    public class FileHandleBarsViewProviderTests
    {
        [Test]
        public async Task ViewSimpleCanBeRendered()
        {
            // Arrange
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);

            string fileRoot = Path.Combine(assemblyDir, "files");

            Mock<IOptions<ServerConfiguration>> options = new Mock<IOptions<ServerConfiguration>>();
            options
                .Setup(options => options.Value)
                .Returns(new ServerConfiguration());

            FileHandleBarsViewProvider viewProvider = new FileHandleBarsViewProvider(
                fileRoot,
                options.Object
            );

            // Act
            string result = await viewProvider.GetTemplate(
                "simpleView.html",
                new
                {
                    viewModel = new
                    {
                        name = "John Doe"
                    }
                },
                Mock.Of<ICommandContext>()
            );

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Contains.Substring("Hello John Doe!"));
        }

        [Test]
        public async Task ViewWithLayoutCanBeRendered()
        {
            // Arrange
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);

            string fileRoot = Path.Combine(assemblyDir, "files");

            Mock<IOptions<ServerConfiguration>> options = new Mock<IOptions<ServerConfiguration>>();
            options
                .Setup(options => options.Value)
                .Returns(new ServerConfiguration());

            FileHandleBarsViewProvider viewProvider = new FileHandleBarsViewProvider(
                fileRoot,
                options.Object
            );

            // Act
            string result = await viewProvider.GetTemplate(
                "viewWithLayout.html",
                new
                {
                    viewModel = new
                    {
                        name = "John Doe"
                    }
                },
                Mock.Of<ICommandContext>()
            );

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Contains.Substring("Hello John Doe! This comes from a template."));
        }

        [Test]
        public async Task ViewWithPartialsCanBeRendered()
        {
            // Arrange
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);

            string fileRoot = Path.Combine(assemblyDir, "files");

            Mock<IOptions<ServerConfiguration>> options = new Mock<IOptions<ServerConfiguration>>();
            options
                .Setup(options => options.Value)
                .Returns(new ServerConfiguration());

            FileHandleBarsViewProvider viewProvider = new FileHandleBarsViewProvider(
                fileRoot,
                options.Object
            );

            // Act
            string result = await viewProvider.GetTemplate(
                "viewWithLayoutAndPartial.html",
                new
                {
                    viewModel = new
                    {
                        name = "John Doe"
                    }
                },
                Mock.Of<ICommandContext>()
            );

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Contains.Substring("Hello John Doe! This comes from a template."));
            Assert.That(result, Contains.Substring("HELLO I AM A PARTIAL"));
            Assert.That(result, Contains.Substring("AND I AM A PARTIAL AS WELL"));
        }

        [Test]
        public async Task ViewWithTokenCanBeRendered()
        {
            // Arrange
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyDir = Path.GetDirectoryName(assemblyPath);
            string fileRoot = Path.Combine(assemblyDir, "files");

            string antiforgeryTokenCookieValue = Guid.NewGuid().ToString();

            ServerConfiguration serverConfig = new ServerConfiguration()
            {
                AntiforgeryTokenCookieName = "XSRF-T",
                AntiforgeryTokenInputName = "X-XSRF-T",
                EnableServerFileCache = true
            };

            Mock<IOptions<ServerConfiguration>> options = new Mock<IOptions<ServerConfiguration>>();
            options
                .Setup(options => options.Value)
                .Returns(serverConfig);

            FileHandleBarsViewProvider viewProvider = new FileHandleBarsViewProvider(
                fileRoot,
                options.Object
            );

            Mock<ICommandContext> mockCommandContext = new Mock<ICommandContext>();
            mockCommandContext
                .Setup(x => x.GetCookie(serverConfig.AntiforgeryTokenCookieName))
                .Returns(antiforgeryTokenCookieValue);

            // Act
            string result = await viewProvider.GetTemplate(
                "antiforgeryView.html",
                new { },
                mockCommandContext.Object
            );

            // Assert
            string expectedInput = $"<input type=\"hidden\" name=\"{serverConfig.AntiforgeryTokenInputName}\" value=\"{antiforgeryTokenCookieValue}\" />";

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.EqualTo(expectedInput));
        }
    }
}
