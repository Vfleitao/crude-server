using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using CrudeServer.Integration.Commands;
using CrudeServer.Integration.Mocks;
using CrudeServer.Server.Contracts;

namespace CrudeServer.Integration
{

    public class HostOverrideTests
    {
        [Test]
        [NonParallelizable]
        public async Task CanOverrideHost()
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);

            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port, useAntiforgeryTokens: true);
            serverBuilder.AddCommand<DataFromRequestCommand>("/", Enums.HttpMethod.GET);

            int newPort = RandomNumberGenerator.GetInt32(1000, 20000);
            while(newPort == port)
            {
                newPort = RandomNumberGenerator.GetInt32(1000, 20000);
            }

            port = newPort;

            serverBuilder.OverrideHosts($"http://localhost:{port}/");

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
                        Method = new HttpMethod("GET"),
                        RequestUri = new Uri($"http://localhost:{port}")
                    };

                    HttpResponseMessage response = await client.SendAsync(request);
                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
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