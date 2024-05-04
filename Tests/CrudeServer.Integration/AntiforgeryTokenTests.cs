using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using CrudeServer.Integration.Commands;
using CrudeServer.Integration.Mocks;
using CrudeServer.Server.Contracts;

using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.Integration
{
    public class AntiforgeryTokenTests
    {
        [Test()]
        public async Task TokenIsGeneratedInCookiesOnAGetRequest()
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port, useAntiforgeryTokens: true);
            serverBuilder.AddCommand<DataFromRequestCommand>("/", Enums.HttpMethod.GET);

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
                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        Method = new HttpMethod("GET"),
                        RequestUri = new Uri($"http://localhost:{port}")
                    };

                    HttpResponseMessage response = await client.SendAsync(request);

                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

                    IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
                    string antiforgeryCookie = cookies.SingleOrDefault(cookie => cookie.Contains("XSRF-T"));

                    Assert.That(antiforgeryCookie, Is.Not.Null);
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
        public async Task TokenAlreadyExists_DoesNotRegenerate()
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port, useAntiforgeryTokens: true);
            serverBuilder.AddCommand<DataFromRequestCommand>("/", Enums.HttpMethod.GET);

            IServerRunner serverRunner = serverBuilder.Buid();

            try
            {
                serverRunner.Run();
                // Just to give time for everything to start
                await Task.Delay(250);

                // Act
                // Assert

                string antiforgeryCookie = null;

                // Get a cookie
                using (HttpClient client = new HttpClient())
                {
                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        Method = new HttpMethod("GET"),
                        RequestUri = new Uri($"http://localhost:{port}")
                    };

                    HttpResponseMessage response = await client.SendAsync(request);

                    IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;
                    antiforgeryCookie = cookies.SingleOrDefault(cookie => cookie.Contains("XSRF-T"));

                    Assert.That(antiforgeryCookie, Is.Not.Null);
                }

                using (HttpClient client = new HttpClient())
                {
                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        Method = new HttpMethod("GET"),
                        RequestUri = new Uri($"http://localhost:{port}")
                    };

                    request.Headers.TryAddWithoutValidation("Cookie", antiforgeryCookie);

                    HttpResponseMessage response = await client.SendAsync(request);

                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

                    IEnumerable<string> cookies = response.Headers.SingleOrDefault(header => header.Key == "Set-Cookie").Value;

                    Assert.That(cookies, Is.Null);
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
        public async Task RouteRequiresAntiforgery_NoToken_ReturnsBadRequest()
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port, useAntiforgeryTokens: true);
            serverBuilder
                .AddCommand<DataFromRequestCommand>("/", Enums.HttpMethod.POST)
                .RequireAntiforgeryToken();

            IServerRunner serverRunner = serverBuilder.Buid();

            try
            {
                serverRunner.Run();
                // Just to give time for everything to start
                await Task.Delay(250);

                // Act
                // Assert

                string antiforgeryCookie = null;

                // Get a cookie
                using (HttpClient client = new HttpClient())
                {
                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        Method = new HttpMethod("POST"),
                        RequestUri = new Uri($"http://localhost:{port}")
                    };

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
        public async Task RouteRequiresAntiforgery_NoCookie_ReturnsBadRequest()
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port, useAntiforgeryTokens: true);
            serverBuilder
                .AddCommand<DataFromRequestCommand>("/", Enums.HttpMethod.POST)
                .RequireAntiforgeryToken();

            IServerRunner serverRunner = serverBuilder.Buid();

            try
            {
                serverRunner.Run();
                // Just to give time for everything to start
                await Task.Delay(250);

                // Act
                // Assert

                string antiforgeryCookie = null;

                // Get a cookie
                using (HttpClient client = new HttpClient())
                using(MultipartFormDataContent content = new MultipartFormDataContent())
                {
                    content.Add(new StringContent("123456789", Encoding.UTF8, "text/plain"), "X-XSRF-T");

                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        Method = new HttpMethod("POST"),
                        RequestUri = new Uri($"http://localhost:{port}")
                    };

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
        public async Task RouteRequiresAntiforgery_CookieAndInputDoNotMatch_ReturnsBadRequest()
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port, useAntiforgeryTokens: true);
            serverBuilder
                .AddCommand<DataFromRequestCommand>("/", Enums.HttpMethod.POST)
                .RequireAntiforgeryToken();

            IServerRunner serverRunner = serverBuilder.Buid();

            try
            {
                serverRunner.Run();
                // Just to give time for everything to start
                await Task.Delay(250);

                // Act
                // Assert

                string antiforgeryCookie = null;

                // Get a cookie
                using (HttpClient client = new HttpClient())
                using (MultipartFormDataContent content = new MultipartFormDataContent())
                {
                    content.Add(new StringContent("123456789", Encoding.UTF8, "text/plain"), "X-XSRF-T");

                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        Method = new HttpMethod("POST"),
                        RequestUri = new Uri($"http://localhost:{port}")
                    };

                    request.Content = content;

                    request.Headers.TryAddWithoutValidation("Cookie", "XSRF-T=1234567890");


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
        public async Task RouteRequiresAntiforgery_CookieAndInputMatch_ReturnsOK()
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port, useAntiforgeryTokens: true);
            serverBuilder
                .AddCommand<DataFromRequestCommand>("/", Enums.HttpMethod.POST)
                .RequireAntiforgeryToken();

            IServerRunner serverRunner = serverBuilder.Buid();

            try
            {
                serverRunner.Run();
                // Just to give time for everything to start
                await Task.Delay(250);

                // Act
                // Assert

                string antiforgeryCookie = null;

                // Get a cookie
                using (HttpClient client = new HttpClient())
                using (MultipartFormDataContent content = new MultipartFormDataContent())
                {
                    content.Add(new StringContent("123456789", Encoding.UTF8, "text/plain"), "X-XSRF-T");

                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        Method = new HttpMethod("POST"),
                        RequestUri = new Uri($"http://localhost:{port}")
                    };

                    request.Content = content;

                    request.Headers.TryAddWithoutValidation("Cookie", "XSRF-T=123456789");


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