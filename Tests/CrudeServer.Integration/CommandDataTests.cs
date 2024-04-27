using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

using CrudeServer.Integration.Commands;
using CrudeServer.Integration.Mocks;
using CrudeServer.Models.Contracts;
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
            serverBuilder.AddCommand<DataFromRequestCommand>("/path/{id:\\d+}/{page:\\w+}", httpMethod);

            DataFromRequestCommand commandInstance = new DataFromRequestCommand();

            serverBuilder.Services.Remove(
                serverBuilder.Services.First(x => x.ServiceType == typeof(DataFromRequestCommand))
            );
            serverBuilder.Services.AddScoped<DataFromRequestCommand>((s) =>
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
                        RequestUri = new Uri($"http://localhost:{port}/path/99/test")
                    };

                    HttpResponseMessage response = await client.SendAsync(request);

                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    Assert.That(response.Content.Headers.Any(x => x.Key == "Content-Type"), Is.True);
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("text/html"));

                    Assert.That(commandInstance.RequestContext.Items, Is.Not.Null);
                    Assert.That(commandInstance.RequestContext.Items, Contains.Key("id"));
                    Assert.That(commandInstance.RequestContext.Items["id"], Is.EqualTo("99"));
                    Assert.That(commandInstance.RequestContext.Items, Contains.Key("page"));
                    Assert.That(commandInstance.RequestContext.Items["page"], Is.EqualTo("test"));
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

            serverBuilder.Services.Remove(
                serverBuilder.Services.First(x => x.ServiceType == typeof(DataFromRequestCommand))
            );
            serverBuilder.Services.AddScoped<DataFromRequestCommand>((s) =>
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

            serverBuilder.Services.Remove(
                serverBuilder.Services.First(x => x.ServiceType == typeof(DataFromRequestCommand))
            );
            serverBuilder.Services.AddScoped<DataFromRequestCommand>((s) =>
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

        [TestCase(Enums.HttpMethod.POST)]
        [TestCase(Enums.HttpMethod.PUT)]
        [Sequential]
        public async Task MultiPartDataFormFilesCanBeRetrieved(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

            DataFromRequestCommand commandInstance = new DataFromRequestCommand();

            serverBuilder.Services.Remove(
                serverBuilder.Services.First(x => x.ServiceType == typeof(DataFromRequestCommand))
            );
            serverBuilder.Services.AddScoped<DataFromRequestCommand>((s) =>
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
                string filePath = "wwwroot/testfile.txt";
                using (HttpClient client = new HttpClient())
                using (MultipartFormDataContent multipartContent = new MultipartFormDataContent())
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    StreamContent fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    multipartContent.Add(fileContent, "file", Path.GetFileName(filePath));

                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        Method = new HttpMethod(httpMethod.ToString()),
                        RequestUri = new Uri($"http://localhost:{port}/99"),
                        Content = multipartContent
                    };

                    HttpResponseMessage response = await client.SendAsync(request);

                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    Assert.That(response.Content.Headers.Any(x => x.Key == "Content-Type"), Is.True);
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("text/html"));

                    Assert.That(commandInstance.RequestContext.Files, Is.Not.Null);
                    Assert.That(commandInstance.RequestContext.Files, Has.Count.EqualTo(1));

                    HttpFile file = commandInstance.RequestContext.Files.First();
                    Assert.That(file.Name, Is.EqualTo("testfile.txt"));
                    Assert.That(file.Content, Is.Not.Null);

                    string commandFileContent = Encoding.UTF8.GetString(file.Content);
                    string originalFileContent = Encoding.UTF8.GetString(File.ReadAllBytes(filePath));

                    Assert.That(commandFileContent, Is.EqualTo(originalFileContent));
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
        public async Task MultiPartDataFormMultipleFilesCanBeRetrieved(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

            DataFromRequestCommand commandInstance = new DataFromRequestCommand();

            serverBuilder.Services.Remove(
                serverBuilder.Services.First(x => x.ServiceType == typeof(DataFromRequestCommand))
            );
            serverBuilder.Services.AddScoped<DataFromRequestCommand>((s) =>
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
                string filePath = "wwwroot/testfile.txt";
                string filePath2 = "wwwroot/testfile2.txt";

                using (HttpClient client = new HttpClient())
                using (MultipartFormDataContent multipartContent = new MultipartFormDataContent())
                using (FileStream fileStream = File.OpenRead(filePath))
                using (FileStream fileStream2 = File.OpenRead(filePath2))
                {
                    StreamContent fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    multipartContent.Add(fileContent, "file", Path.GetFileName(filePath));

                    StreamContent fileContent2 = new StreamContent(fileStream2);
                    fileContent2.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    multipartContent.Add(fileContent2, "file2", Path.GetFileName(filePath2));

                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        Method = new HttpMethod(httpMethod.ToString()),
                        RequestUri = new Uri($"http://localhost:{port}/99"),
                        Content = multipartContent
                    };

                    HttpResponseMessage response = await client.SendAsync(request);

                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    Assert.That(response.Content.Headers.Any(x => x.Key == "Content-Type"), Is.True);
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("text/html"));

                    Assert.That(commandInstance.RequestContext.Files, Is.Not.Null);
                    Assert.That(commandInstance.RequestContext.Files, Has.Count.EqualTo(2));

                    HttpFile file1 = commandInstance.RequestContext.Files.First();
                    Assert.That(file1.Name, Is.EqualTo("testfile.txt"));
                    Assert.That(file1.Content, Is.Not.Null);

                    string commandFileContent = Encoding.UTF8.GetString(file1.Content);
                    string originalFileContent = Encoding.UTF8.GetString(File.ReadAllBytes(filePath));

                    Assert.That(commandFileContent, Is.EqualTo(originalFileContent));

                    HttpFile file2 = commandInstance.RequestContext.Files.Last();
                    Assert.That(file2.Name, Is.EqualTo("testfile2.txt"));
                    Assert.That(file2.Content, Is.Not.Null);

                    commandFileContent = Encoding.UTF8.GetString(file2.Content);
                    originalFileContent = Encoding.UTF8.GetString(File.ReadAllBytes(filePath2));

                    Assert.That(commandFileContent, Is.EqualTo(originalFileContent));
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
        public async Task MultiPartDataFormJsonCanBeRetrieved(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

            DataFromRequestCommand commandInstance = new DataFromRequestCommand();

            serverBuilder.Services.Remove(
                serverBuilder.Services.First(x => x.ServiceType == typeof(DataFromRequestCommand))
            );
            serverBuilder.Services.AddScoped<DataFromRequestCommand>((s) =>
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
                using (MultipartFormDataContent multipartContent = new MultipartFormDataContent())
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
                                        doors = new string[] { "Front", "Back" }
                                    },
                                    scores = new int[] { 1, 2, 3 }
                                }
                            );

                    multipartContent.Add(new StringContent(
                            expectedData,
                            Encoding.UTF8,
                            "application/json"
                    ));

                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        Method = new HttpMethod(httpMethod.ToString()),
                        RequestUri = new Uri($"http://localhost:{port}/99"),
                        Content = multipartContent
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

        [TestCase(Enums.HttpMethod.POST)]
        [TestCase(Enums.HttpMethod.PUT)]
        [Sequential]
        public async Task MultiPartDataFormFilesAndJsonCanBeRetrievedAtSameTime(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

            DataFromRequestCommand commandInstance = new DataFromRequestCommand();

            serverBuilder.Services.Remove(
                serverBuilder.Services.First(x => x.ServiceType == typeof(DataFromRequestCommand))
            );
            serverBuilder.Services.AddScoped<DataFromRequestCommand>((s) =>
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
                string filePath = "wwwroot/testfile.txt";
                using (HttpClient client = new HttpClient())
                using (MultipartFormDataContent multipartContent = new MultipartFormDataContent())
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    StreamContent fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    multipartContent.Add(fileContent, "file", Path.GetFileName(filePath));

                    string expectedData = JsonConvert.SerializeObject(
                                new
                                {
                                    name = "John",
                                    age = 25,
                                    address = new
                                    {
                                        street = "123 Main St",
                                        city = "Anytown",
                                        doors = new string[] { "Front", "Back" }
                                    },
                                    scores = new int[] { 1, 2, 3 }
                                }
                            );

                    multipartContent.Add(new StringContent(
                            expectedData,
                            Encoding.UTF8,
                            "application/json"
                    ));

                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        Method = new HttpMethod(httpMethod.ToString()),
                        RequestUri = new Uri($"http://localhost:{port}/99"),
                        Content = multipartContent
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

                    Assert.That(commandInstance.RequestContext.Files, Is.Not.Null);
                    Assert.That(commandInstance.RequestContext.Files, Has.Count.EqualTo(1));

                    HttpFile file = commandInstance.RequestContext.Files.First();
                    Assert.That(file.Name, Is.EqualTo("testfile.txt"));
                    Assert.That(file.Content, Is.Not.Null);

                    string commandFileContent = Encoding.UTF8.GetString(file.Content);
                    string originalFileContent = Encoding.UTF8.GetString(File.ReadAllBytes(filePath));

                    Assert.That(commandFileContent, Is.EqualTo(originalFileContent));
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
        public async Task MultiPartDataFormFieldsCanBeRetrievedAtSameTime(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

            DataFromRequestCommand commandInstance = new DataFromRequestCommand();

            serverBuilder.Services.Remove(
                serverBuilder.Services.First(x => x.ServiceType == typeof(DataFromRequestCommand))
            );
            serverBuilder.Services.AddScoped<DataFromRequestCommand>((s) =>
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
                using (MultipartFormDataContent multipartContent = new MultipartFormDataContent())
                {
                    multipartContent.Add(new StringContent("John", Encoding.UTF8, "text/plain"), "name");
                    multipartContent.Add(new StringContent("99", Encoding.UTF8, "text/plain"), "age");

                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        Method = new HttpMethod(httpMethod.ToString()),
                        RequestUri = new Uri($"http://localhost:{port}/99"),
                        Content = multipartContent
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

                    Assert.That(commandInstance.RequestContext.Items, Contains.Key("age"));
                    Assert.That(commandInstance.RequestContext.Items["age"], Is.EqualTo("99"));
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
        public async Task MultiPartDataFormComplexFieldsCanBeRetrievedAtSameTime(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

            DataFromRequestCommand commandInstance = new DataFromRequestCommand();

            serverBuilder.Services.Remove(
                serverBuilder.Services.First(x => x.ServiceType == typeof(DataFromRequestCommand))
            );
            serverBuilder.Services.AddScoped<DataFromRequestCommand>((s) =>
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
                using (MultipartFormDataContent multipartContent = new MultipartFormDataContent())
                {

                    multipartContent.Add(new StringContent("John", Encoding.UTF8, "text/plain"), "name");
                    multipartContent.Add(new StringContent("123 Main St", Encoding.UTF8, "text/plain"), "address.street");
                    multipartContent.Add(new StringContent("Anytown", Encoding.UTF8, "text/plain"), "address.city");
                    multipartContent.Add(new StringContent("Front", Encoding.UTF8, "text/plain"), "address.doors[0]");
                    multipartContent.Add(new StringContent("Back", Encoding.UTF8, "text/plain"), "address.doors[1]");

                    multipartContent.Add(new StringContent("Doe", Encoding.UTF8, "text/plain"), "persons[0].name");
                    multipartContent.Add(new StringContent("Joe", Encoding.UTF8, "text/plain"), "persons[1].name");

                    multipartContent.Add(new StringContent("Doe", Encoding.UTF8, "text/plain"), "[0]");
                    multipartContent.Add(new StringContent("Joe", Encoding.UTF8, "text/plain"), "[1]");

                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        Method = new HttpMethod(httpMethod.ToString()),
                        RequestUri = new Uri($"http://localhost:{port}/99"),
                        Content = multipartContent
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

                    Assert.That(commandInstance.RequestContext.Items, Contains.Key("persons"));
                    Assert.That(commandInstance.RequestContext.Items["persons"], Is.InstanceOf<List<object>>());

                    Assert.That(commandInstance.RequestContext.Items, Contains.Key("__array__"));
                    Assert.That(commandInstance.RequestContext.Items["__array__"], Is.InstanceOf<List<object>>());

                    Dictionary<string, object> address = (Dictionary<string, object>)commandInstance.RequestContext.Items["address"];
                    Assert.That(address, Contains.Key("street"));
                    Assert.That(address, Contains.Key("city"));

                    Assert.That(address["street"], Is.EqualTo("123 Main St"));
                    Assert.That(address["city"], Is.EqualTo("Anytown"));

                    Assert.That(address["doors"], Is.InstanceOf<List<object>>());
                    List<object> doors = (List<object>)address["doors"];
                    Assert.That(doors[0], Is.EqualTo("Front"));
                    Assert.That(doors[1], Is.EqualTo("Back"));

                    Assert.That(commandInstance.RequestContext.Items["persons"], Is.InstanceOf<List<object>>());
                    List<object> persons = (List<object>)commandInstance.RequestContext.Items["persons"];

                    Assert.That(persons[0], Is.InstanceOf<Dictionary<string, object>>());
                    Assert.That((Dictionary<string, object>)persons[0], Contains.Key("name"));
                    Assert.That(((Dictionary<string, object>)persons[0])["name"], Is.EqualTo("Doe"));

                    Assert.That(persons[1], Is.InstanceOf<Dictionary<string, object>>());
                    Assert.That((Dictionary<string, object>)persons[1], Contains.Key("name"));
                    Assert.That(((Dictionary<string, object>)persons[1])["name"], Is.EqualTo("Joe"));

                    Assert.That(commandInstance.RequestContext.Items["__array__"], Is.InstanceOf<List<object>>());
                    List<object> array = (List<object>)commandInstance.RequestContext.Items["__array__"];
                    Assert.That(array[0], Is.EqualTo("Doe"));
                    Assert.That(array[1], Is.EqualTo("Joe"));
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
        public async Task MultiPartDataFormCanCombineJsonFilesAndFieldsAtSameTime(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = new Random().Next(1000, 9999);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

            DataFromRequestCommand commandInstance = new DataFromRequestCommand();

            serverBuilder.Services.Remove(
                serverBuilder.Services.First(x => x.ServiceType == typeof(DataFromRequestCommand))
            );
            serverBuilder.Services.AddScoped<DataFromRequestCommand>((s) =>
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
                string filePath = "wwwroot/testfile.txt";
                using (HttpClient client = new HttpClient())
                using (MultipartFormDataContent multipartContent = new MultipartFormDataContent())
                using (FileStream fileStream = File.OpenRead(filePath))
                {
                    multipartContent.Add(new StringContent("John", Encoding.UTF8, "text/plain"), "name");
                    multipartContent.Add(new StringContent("123 Main St", Encoding.UTF8, "text/plain"), "address.street");
                    multipartContent.Add(new StringContent("Anytown", Encoding.UTF8, "text/plain"), "address.city");
                    multipartContent.Add(new StringContent("Front", Encoding.UTF8, "text/plain"), "address.doors[0]");
                    multipartContent.Add(new StringContent("Back", Encoding.UTF8, "text/plain"), "address.doors[1]");

                    multipartContent.Add(new StringContent("Doe", Encoding.UTF8, "text/plain"), "persons[0].name");
                    multipartContent.Add(new StringContent("Joe", Encoding.UTF8, "text/plain"), "persons[1].name");

                    multipartContent.Add(new StringContent("Doe", Encoding.UTF8, "text/plain"), "[0]");
                    multipartContent.Add(new StringContent("Joe", Encoding.UTF8, "text/plain"), "[1]");

                    StreamContent fileContent = new StreamContent(fileStream);
                    fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
                    multipartContent.Add(fileContent, "file", Path.GetFileName(filePath));

                    string expectedData = JsonConvert.SerializeObject(new { age = 25, scores = new int[] { 1, 2, 3 } });
                    multipartContent.Add(new StringContent(
                            expectedData,
                            Encoding.UTF8,
                            "application/json"
                    ));

                    HttpRequestMessage request = new HttpRequestMessage()
                    {
                        Method = new HttpMethod(httpMethod.ToString()),
                        RequestUri = new Uri($"http://localhost:{port}/99"),
                        Content = multipartContent
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

                    Assert.That(commandInstance.RequestContext.Items, Contains.Key("persons"));
                    Assert.That(commandInstance.RequestContext.Items["persons"], Is.InstanceOf<List<object>>());

                    Assert.That(commandInstance.RequestContext.Items, Contains.Key("__array__"));
                    Assert.That(commandInstance.RequestContext.Items["__array__"], Is.InstanceOf<List<object>>());

                    Dictionary<string, object> address = (Dictionary<string, object>)commandInstance.RequestContext.Items["address"];
                    Assert.That(address, Contains.Key("street"));
                    Assert.That(address, Contains.Key("city"));

                    Assert.That(address["street"], Is.EqualTo("123 Main St"));
                    Assert.That(address["city"], Is.EqualTo("Anytown"));

                    Assert.That(address["doors"], Is.InstanceOf<List<object>>());
                    List<object> doors = (List<object>)address["doors"];
                    Assert.That(doors[0], Is.EqualTo("Front"));
                    Assert.That(doors[1], Is.EqualTo("Back"));

                    Assert.That(commandInstance.RequestContext.Items["persons"], Is.InstanceOf<List<object>>());
                    List<object> persons = (List<object>)commandInstance.RequestContext.Items["persons"];

                    Assert.That(persons[0], Is.InstanceOf<Dictionary<string, object>>());
                    Assert.That((Dictionary<string, object>)persons[0], Contains.Key("name"));
                    Assert.That(((Dictionary<string, object>)persons[0])["name"], Is.EqualTo("Doe"));

                    Assert.That(persons[1], Is.InstanceOf<Dictionary<string, object>>());
                    Assert.That((Dictionary<string, object>)persons[1], Contains.Key("name"));
                    Assert.That(((Dictionary<string, object>)persons[1])["name"], Is.EqualTo("Joe"));

                    Assert.That(commandInstance.RequestContext.Items["__array__"], Is.InstanceOf<List<object>>());
                    List<object> array = (List<object>)commandInstance.RequestContext.Items["__array__"];
                    Assert.That(array[0], Is.EqualTo("Doe"));
                    Assert.That(array[1], Is.EqualTo("Joe"));

                    Assert.That(commandInstance.RequestContext.Items, Contains.Key("age"));
                    Assert.That(commandInstance.RequestContext.Items["age"], Is.EqualTo(25));

                    Assert.That(commandInstance.RequestContext.Items, Contains.Key("scores"));
                    Assert.That(commandInstance.RequestContext.Items["scores"], Is.InstanceOf<List<object>>());

                    List<object> list = (List<object>)commandInstance.RequestContext.Items["scores"];

                    Assert.That(list[0], Is.EqualTo(1));
                    Assert.That(list[1], Is.EqualTo(2));
                    Assert.That(list[2], Is.EqualTo(3));

                    Assert.That(commandInstance.RequestContext.Files, Is.Not.Null);
                    Assert.That(commandInstance.RequestContext.Files, Has.Count.EqualTo(1));

                    HttpFile file = commandInstance.RequestContext.Files.First();
                    Assert.That(file.Name, Is.EqualTo("testfile.txt"));
                    Assert.That(file.Content, Is.Not.Null);

                    string commandFileContent = Encoding.UTF8.GetString(file.Content);
                    string originalFileContent = Encoding.UTF8.GetString(File.ReadAllBytes(filePath));

                    Assert.That(commandFileContent, Is.EqualTo(originalFileContent));
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