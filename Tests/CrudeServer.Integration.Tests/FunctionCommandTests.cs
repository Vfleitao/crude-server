using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

using CrudeServer.HttpCommands.Responses;
using CrudeServer.Integration.Mocks;
using CrudeServer.Models.Contracts;
using CrudeServer.Server.Contracts;

namespace CrudeServer.Integration.Tests
{
    public class FunctionCommandTests
    {
        [Test]
        public async Task CanCallCommandFunctions()
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);

            serverBuilder.AddCommandFunction("/", Enums.HttpMethod.GET, async () =>
            {
                return new StatusCodeResponse()
                {
                    ContentType = "text/html",
                    StatusCode = 418
                };
            });

            IServerRunner serverRunner = serverBuilder.Buid();

            try
            {
                serverRunner.Run();
                // Just to give time for everything to start
                await Task.Delay(250);

                // Act
                // Assert
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"http://localhost:{port}/");

                    Assert.That((int)response.StatusCode, Is.EqualTo(418));
                    Assert.That(response.Content.Headers.Any(x => x.Key == "Content-Type"), Is.True);
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("text/html"));
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                // Clean up
                await serverRunner.Stop();
            }
        }

        [Test]
        public async Task CommandFunctionCanHaveInjectedContent()
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);

            serverBuilder.AddCommandFunction("/", Enums.HttpMethod.GET, async (ICommandContext context) =>
            {
                if(context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }
                
                return new StatusCodeResponse()
                {
                    ContentType = "text/html",
                    StatusCode = 418
                };
            });

            IServerRunner serverRunner = serverBuilder.Buid();

            try
            {
                serverRunner.Run();
                // Just to give time for everything to start
                await Task.Delay(250);

                // Act
                // Assert
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"http://localhost:{port}/");

                    Assert.That((int)response.StatusCode, Is.EqualTo(418));
                    Assert.That(response.Content.Headers.Any(x => x.Key == "Content-Type"), Is.True);
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("text/html"));
                }
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.Message);
            }
            finally
            {
                // Clean up
                await serverRunner.Stop();
            }
        }
    }
}
