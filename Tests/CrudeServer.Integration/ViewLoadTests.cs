using System.Collections.Concurrent;
using System.Net;

using CrudeServer.HttpCommands.Responses;
using CrudeServer.Integration.Commands;
using CrudeServer.Integration.Mocks;
using CrudeServer.Models;
using CrudeServer.Server;
using CrudeServer.Server.Contracts;

namespace CrudeServer.Integration
{
    public class ViewLoadTests
    {
        [Test]
        [Sequential]
        public async Task ViewCanBeLoaded()
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<MockViewHttpCommand>("/", Enums.HttpMethod.GET);

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
                    string content = await response.Content.ReadAsStringAsync();

                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    Assert.That(response.Content.Headers.Any(x => x.Key == "Content-Type"), Is.True);
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("text/html"));
                    Assert.That(content, Contains.Substring("Hello Vitor!"));
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
        [Sequential]
        public async Task CanLoadViewsAtSameTime()
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<MockGuidHttpCommand>("/", Enums.HttpMethod.GET);
            serverBuilder.AddCommand<AnotherMockGuidCommand>("/heya", Enums.HttpMethod.GET);

            IServerRunner serverRunner = serverBuilder.Buid();

            try
            {
                serverRunner.Run();
                // Just to give time for everything to start
                await Task.Delay(250);

                ConcurrentBag<Guid> requestIds = new ConcurrentBag<Guid>();
                ConcurrentBag<string> responses = new ConcurrentBag<string>();

                // Act
                // Assert
                using (HttpClient client = new HttpClient())
                {
                    IEnumerable<int> range = Enumerable.Range(0, 5000);
                    await Parallel.ForEachAsync(
                        range,
                        new ParallelOptions { MaxDegreeOfParallelism = 200 },
                        async (i, token) =>
                        {
                            Task<HttpResponseMessage> view_1 = client.GetAsync($"http://localhost:{port}/");
                            Task<HttpResponseMessage> view_2 = client.GetAsync($"http://localhost:{port}/heya");

                            Task.WaitAll(
                                   view_1,
                                   view_2
                            );

                            HttpResponseMessage view_1_response = await view_1;
                            HttpResponseMessage view_2_response = await view_2;

                            requestIds.Add(Guid.Parse(view_1_response.Headers.First(x => x.Key.ToLower() == "X-Request-ID".ToLower()).Value.First()));
                            requestIds.Add(Guid.Parse(view_2_response.Headers.First(x => x.Key.ToLower() == "X-Request-ID".ToLower()).Value.First()));

                            Assert.That(view_1_response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                            Assert.That(view_2_response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

                            string response_1_html = await view_1_response.Content.ReadAsStringAsync();
                            string response_2_html = await view_2_response.Content.ReadAsStringAsync();

                            Assert.That(response_1_html, Contains.Substring("Yoh "));
                            Assert.That(response_2_html, Contains.Substring("Heya "));

                            responses.Add(response_1_html);
                            responses.Add(response_2_html);
                        }
                    );
                }

                Assert.That(requestIds.Count, Is.EqualTo(10000));
                Assert.That(requestIds.Distinct().Count(), Is.EqualTo(10000));

                Assert.That(responses.Count, Is.EqualTo(10000));
                Assert.That(responses.Distinct().Count(), Is.EqualTo(10000));
            }
            catch (Exception ex)
            {
                Assert.Fail(Newtonsoft.Json.JsonConvert.SerializeObject(ex));
            }
            finally
            {
                // Clean up
                await serverRunner.Stop();
            }
        }
    }
}