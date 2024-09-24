using System;
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
    public class RequestSizeLimitTests
    {
        [Test()]
        public async Task MaximumRequestSizeIsRespected()
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port, useRequestSizeLimiter: true);
            serverBuilder.AddCommand<DataFromRequestCommand>("/", Enums.HttpMethod.POST);

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
                        Method = new HttpMethod("POST"),
                        RequestUri = new Uri($"http://localhost:{port}")
                    };

                    string dummyData = new string('a', 2 * 1024 * 1024);

                    HttpContent content = new StringContent(dummyData, Encoding.UTF8, "text/plain");
                    request.Content = content;

                    HttpResponseMessage response = await client.SendAsync(request);

                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
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

        [Test()]
        public async Task MaximumRequestSizeIsRespectedOnChunckedEncoding()
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port, useRequestSizeLimiter: true);
            serverBuilder.AddCommand<DataFromRequestCommand>("/", Enums.HttpMethod.POST);

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
                        Method = new HttpMethod("POST"),
                        RequestUri = new Uri($"http://localhost:{port}")
                    };
                    request.Headers.TransferEncodingChunked = true;
                    
                    string dummyData = new string('a', 2 * 1024 * 1024);

                    StringContent content = new StringContent(dummyData, Encoding.UTF8, "text/plain");

                    request.Content = content;

                    HttpResponseMessage response = await client.SendAsync(request);

                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
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

        [Test()]
        public async Task RequestIsSmallerThanLimitReturnsOk()
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port, useRequestSizeLimiter: true);
            serverBuilder.AddCommand<DataFromRequestCommand>("/", Enums.HttpMethod.POST);

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
                        Method = new HttpMethod("POST"),
                        RequestUri = new Uri($"http://localhost:{port}")
                    };
                    request.Headers.TransferEncodingChunked = true;

                    string dummyData = new string('a', (int)(0.5 * 1024 * 1024));

                    StringContent content = new StringContent("{'a' = '" + dummyData + "'}", Encoding.UTF8, "application/json");

                    request.Content = content;

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