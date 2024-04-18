using System.Net;
using System.Text;

using CrudeServer.Integration.Commands;
using CrudeServer.Integration.Mocks;
using CrudeServer.Server.Contracts;

using Microsoft.Extensions.DependencyInjection;

using Newtonsoft.Json;

namespace CrudeServer.Integration
{
    public class CommandDataTests
    {
        [Sequential]
        [TestCase(Enums.HttpMethod.GET)]
        [TestCase(Enums.HttpMethod.POST)]
        [TestCase(Enums.HttpMethod.PUT)]
        [TestCase(Enums.HttpMethod.DELETE)]
        [TestCase(Enums.HttpMethod.OPTIONS)]
        [TestCase(Enums.HttpMethod.HEAD)]
        public async Task CanGetDataFromUrlRequest(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

            DataFromRequestCommand commandInstance = new DataFromRequestCommand();

            serverBuilder.ServiceCollection.Remove(
                serverBuilder.ServiceCollection.First(x => x.ServiceType == typeof(DataFromRequestCommand))
            );
            serverBuilder.ServiceCollection.AddScoped<DataFromRequestCommand>((s) =>
            {
                return commandInstance;
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
                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        Method = new HttpMethod(httpMethod.ToString()),
                        RequestUri = new Uri($"http://localhost:{port}/99")
                    };

                    HttpResponseMessage response = await client.SendAsync(request);

                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    Assert.That(response.Content.Headers.Any(x => x.Key == "Content-Type"), Is.True);
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("text/html"));

                    Assert.That(commandInstance.RequestContext.Items, Is.Not.Null);
                    Assert.That(commandInstance.RequestContext.Items, Contains.Key("id"));
                    Assert.That(commandInstance.RequestContext.Items["id"], Is.EqualTo("99"));
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

        [TestCase(Enums.HttpMethod.POST)]
        [TestCase(Enums.HttpMethod.PUT)]
        [Sequential]
        public async Task CanGetDataFromJsonPostRequest(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

            DataFromRequestCommand commandInstance = new DataFromRequestCommand();

            serverBuilder.ServiceCollection.Remove(
                serverBuilder.ServiceCollection.First(x => x.ServiceType == typeof(DataFromRequestCommand))
            );
            serverBuilder.ServiceCollection.AddScoped<DataFromRequestCommand>((s) =>
            {
                return commandInstance;
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
                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        Method = new HttpMethod(httpMethod.ToString()),
                        RequestUri = new Uri($"http://localhost:{port}/99"),
                        Content = new StringContent("{\"name\":\"John\"}", Encoding.UTF8, "application/json")
                    };

                    HttpResponseMessage response = await client.SendAsync(request);

                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    Assert.That(response.Content.Headers.Any(x => x.Key == "Content-Type"), Is.True);
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("text/html"));

                    Assert.That(commandInstance.RequestContext.Items, Is.Not.Null);

                    Assert.That(commandInstance.RequestContext.Items, Contains.Key("id"));
                    Assert.That(commandInstance.RequestContext.Items["id"], Is.EqualTo("99"));

                    Assert.That(commandInstance.RequestContext.Items, Contains.Key("name"));
                    Assert.That(commandInstance.RequestContext.Items["name"], Is.EqualTo("John"));
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

        [TestCase(Enums.HttpMethod.POST)]
        [TestCase(Enums.HttpMethod.PUT)]
        [Sequential]
        public async Task ComplexJsonDataCanBeSet(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

            DataFromRequestCommand commandInstance = new DataFromRequestCommand();

            serverBuilder.ServiceCollection.Remove(
                serverBuilder.ServiceCollection.First(x => x.ServiceType == typeof(DataFromRequestCommand))
            );
            serverBuilder.ServiceCollection.AddScoped<DataFromRequestCommand>((s) =>
            {
                return commandInstance;
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
                    string expectedData = JsonConvert.SerializeObject(
                                                    new
                                                    {
                                                        name = "John",
                                                        age = 25,
                                                        address = new
                                                        {
                                                            street = "123 Main St",
                                                            city = "Anytown",
                                                            doors = new string[]
                                                            {
                                                                "Front", "Back"
                                                            }
                                                        },
                                                        scores = new int[] {
                                                            1,2,3
                                                        }
                                                    }
                                                );
                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        Method = new HttpMethod(httpMethod.ToString()),
                        RequestUri = new Uri($"http://localhost:{port}/99"),
                        Content = new StringContent(
                            expectedData,
                            Encoding.UTF8,
                            "application/json"
                        ),
                    };

                    HttpResponseMessage response = await client.SendAsync(request);

                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    Assert.That(response.Content.Headers.Any(x => x.Key == "Content-Type"), Is.True);
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("text/html"));

                    Assert.That(commandInstance.RequestContext.Items, Is.Not.Null);

                    Assert.That(commandInstance.RequestContext.Items, Contains.Key("id"));
                    Assert.That(commandInstance.RequestContext.Items["id"], Is.EqualTo("99"));

                    Assert.That(commandInstance.RequestContext.Items, Contains.Key("name"));
                    Assert.That(commandInstance.RequestContext.Items["name"], Is.EqualTo("John"));

                    Assert.That(commandInstance.RequestContext.Items, Contains.Key("address"));
                    Assert.That(commandInstance.RequestContext.Items["address"], Is.InstanceOf<Dictionary<string, object>>());

                    Dictionary<string, object> address = (Dictionary<string, object>)commandInstance.RequestContext.Items["address"];

                    Assert.That(address, Contains.Key("street"));
                    Assert.That(address, Contains.Key("city"));
                    Assert.That(address, Contains.Key("doors"));

                    Assert.That(address["street"], Is.EqualTo("123 Main St"));
                    Assert.That(address["city"], Is.EqualTo("Anytown"));

                    Assert.That(address["doors"], Is.InstanceOf<List<object>>());
                    List<object> doors = (List<object>)address["doors"];

                    Assert.That(doors[0], Is.EqualTo("Front"));
                    Assert.That(doors[1], Is.EqualTo("Back"));

                    Assert.That(commandInstance.RequestContext.Items, Contains.Key("scores"));
                    Assert.That(commandInstance.RequestContext.Items["scores"], Is.InstanceOf<List<object>>());

                    List<object> list = (List<object>)commandInstance.RequestContext.Items["scores"];

                    Assert.That(list[0], Is.EqualTo(1));
                    Assert.That(list[1], Is.EqualTo(2));
                    Assert.That(list[2], Is.EqualTo(3));
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