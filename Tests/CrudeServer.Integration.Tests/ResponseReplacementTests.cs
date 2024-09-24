using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using CrudeServer.Enums;
using CrudeServer.Integration.Commands;
using CrudeServer.Integration.Mocks;
using CrudeServer.Server.Contracts;

namespace CrudeServer.Integration
{
    public class ResponseReplacementTests
    {
        [Test()]
        public async Task TokenIsGeneratedInCookiesOnAGetRequest()
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/", Enums.HttpMethod.GET);

            serverBuilder.ReplaceDefaultResponses<MockNotFoundResponse>(DefaultStatusCodes.NotFound);

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
                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        Method = new System.Net.Http.HttpMethod("GET"),
                        RequestUri = new Uri($"http://localhost:{port}/this-is-not-found")
                    };

                    HttpResponseMessage response = await client.SendAsync(request);

                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));

                    string content = await response.Content.ReadAsStringAsync();
                    Assert.That(content, Is.EqualTo("THIS IS A CUSTOM NOT FOUND PAGE"));

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