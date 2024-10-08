﻿using System;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

using CrudeServer.Attributes;
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

            IServerRunner serverRunner = serverBuilder.Build();

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
                Assert.That(context, Is.Not.Null);

                return new StatusCodeResponse()
                {
                    ContentType = "text/html",
                    StatusCode = 418
                };
            });

            IServerRunner serverRunner = serverBuilder.Build();

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
        public async Task CommandFunctionCanHaveInjectedContentFromRequest()
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);

            serverBuilder.AddCommandFunction(
                "/{id:\\d+}/{page:\\w+}",
                Enums.HttpMethod.GET,
                async (ICommandContext context, [FromRequest] TestModel model) =>
            {

                Assert.That(context, Is.Not.Null);
                Assert.That(model, Is.Not.Null);
                Assert.That(model.Id, Is.EqualTo(10));
                Assert.That(model.Page, Is.EqualTo("test"));

                return new StatusCodeResponse()
                {
                    ContentType = "text/html",
                    StatusCode = 418
                };
            });

            IServerRunner serverRunner = serverBuilder.Build();

            try
            {
                serverRunner.Run();
                // Just to give time for everything to start
                await Task.Delay(250);

                // Act
                // Assert
                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage response = await client.GetAsync($"http://localhost:{port}/10/test");

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

        private class TestModel
        {
            public int Id { get; set; }
            public string Page { get; set; }
        }
    }
}
