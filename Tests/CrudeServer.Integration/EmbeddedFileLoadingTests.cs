using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

using CrudeServer.Integration.Mocks;
using CrudeServer.Server.Contracts;

namespace CrudeServer.Integration
{
    public class EmbeddedFileLoadingTests
    {
        [Test]
        [Sequential]
        public async Task CanGetFiles()
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
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
                    HttpResponseMessage response = await client.GetAsync($"http://localhost:{port}/main.css");
                    string content = await response.Content.ReadAsStringAsync();

                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    Assert.That(response.Content.Headers.Any(x => x.Key == "Content-Type"), Is.True);
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("text/css"));
                    Assert.That(content, Is.EqualTo("body { background-color: #FF0000; }"));
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
        public async Task CanGetImages()
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
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
                    HttpResponseMessage response = await client.GetAsync($"http://localhost:{port}/image.jpg");

                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    Assert.That(response.Content.Headers.Any(x => x.Key == "Content-Type"), Is.True);
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("image/jpeg"));
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
        public async Task CanLoadItemAtSameTime()
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            IServerRunner serverRunner = serverBuilder.Buid();

            try
            {
                serverRunner.Run();
                // Just to give time for everything to start
                await Task.Delay(250);

                ConcurrentBag<Guid> requestIds = new ConcurrentBag<Guid>();

                // Act
                // Assert
                using (HttpClient client = new HttpClient())
                {
                    IEnumerable<int> range = Enumerable.Range(0, 50000);
                    await Parallel.ForEachAsync(
                        range,
                        new ParallelOptions { MaxDegreeOfParallelism = 200 },
                        async (i, token) =>
                        {
                            Task<HttpResponseMessage> cssTask = client.GetAsync($"http://localhost:{port}/main.css");
                            Task<HttpResponseMessage> imageTask = client.GetAsync($"http://localhost:{port}/image.jpg");

                            Task.WaitAll(
                                   cssTask,
                                   imageTask
                            );

                            HttpResponseMessage imageResponse = imageTask.Result;

                            requestIds.Add(Guid.Parse(imageResponse.Headers.First(x => x.Key.ToLower() == "X-Request-ID".ToLower()).Value.First()));

                            Assert.That(imageResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                            Assert.That(imageResponse.Content.Headers.Any(x => x.Key == "Content-Type"), Is.True);
                            Assert.That(imageResponse.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("image/jpeg"));

                            HttpResponseMessage cssResponse = cssTask.Result;

                            requestIds.Add(Guid.Parse(cssResponse.Headers.First(x => x.Key.ToLower() == "X-Request-ID".ToLower()).Value.First()));

                            string content = await cssResponse.Content.ReadAsStringAsync();
                            Assert.That(cssResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                            Assert.That(cssResponse.Content.Headers.Any(x => x.Key == "Content-Type"), Is.True);
                            Assert.That(cssResponse.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("text/css"));
                            Assert.That(content, Is.EqualTo("body { background-color: #FF0000; }"));
                        }
                    );
                }

                Assert.That(requestIds.Count, Is.EqualTo(100000));
                Assert.That(requestIds.Distinct().Count(), Is.EqualTo(100000));
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