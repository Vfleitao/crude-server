using System.Threading.Tasks;

using CrudeServer.Providers;

namespace CrudeServer.Lib.Tests.Providers
{
    public class HandleBarsViewProviderTests
    {
        [Test]
        public async Task ViewSimpleCanBeRendered()
        {
            // Arrange
            HandleBarsViewProvider viewProvider = new HandleBarsViewProvider(
                GetType().Assembly,
                "files");

            // Act
            string result = await viewProvider.GetTemplate(
                "simpleView.html",
                new
                {
                    viewModel = new
                    {
                        name = "John Doe"
                    }
                }
            );

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Contains.Substring("Hello John Doe!"));
        }

        [Test]
        public async Task ViewWithLayoutCanBeRendered()
        {
            // Arrange
            HandleBarsViewProvider viewProvider = new HandleBarsViewProvider(
                GetType().Assembly,
                "files");

            // Act
            string result = await viewProvider.GetTemplate(
                "viewWithLayout.html",
                new
                {
                    viewModel = new
                    {
                        name = "John Doe"
                    }
                }
            );

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Contains.Substring("Hello John Doe! This comes from a template."));
        }
    }
}
