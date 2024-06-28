using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

using CrudeServer.Integration.Commands;
using CrudeServer.Integration.Mocks;
using CrudeServer.Server.Contracts;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace CrudeServer.Integration
{
    public class CommandDataTests
    {
        [NonParallelizable]
        [TestCase(Enums.HttpMethod.GET)]
        [TestCase(Enums.HttpMethod.POST)]
        [TestCase(Enums.HttpMethod.PUT)]
        [TestCase(Enums.HttpMethod.DELETE)]
        [TestCase(Enums.HttpMethod.OPTIONS)]
        public async Task CanGetDataFromUrlRequest(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/path/{id:\\d+}/{page:\\w+}", httpMethod);

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
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("application/json"));

                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JObject.Parse(responseContent);

                    Assert.That(responseData, Is.Not.Null);

                    Assert.That(responseData.items, Is.Not.Null);
                    dynamic item = responseData.items;

                    Assert.That(item.id, Is.Not.Null);

                    string id = item.id;
                    Assert.That(id, Is.EqualTo("99"));
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
        [NonParallelizable]
        public async Task CanGetDataFromJsonPostRequest(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

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
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("application/json"));

                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JObject.Parse(responseContent);

                    Assert.That(responseData, Is.Not.Null);

                    Assert.That(responseData.items, Is.Not.Null);
                    dynamic item = responseData.items;

                    Assert.That(item.id, Is.Not.Null);

                    string id = item.id;
                    Assert.That(id, Is.EqualTo("99"));

                    Assert.That(item.name, Is.Not.Null);

                    string name = item.name;
                    Assert.That(name, Is.EqualTo("John"));
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
        [NonParallelizable]
        public async Task ComplexJsonDataCanBeSet(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

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
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("application/json"));

                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JObject.Parse(responseContent);

                    Assert.That(responseData, Is.Not.Null);

                    Assert.That(responseData.items, Is.Not.Null);
                    dynamic item = responseData.items;

                    Assert.That(item.id, Is.Not.Null);

                    string id = item.id;
                    Assert.That(id, Is.EqualTo("99"));

                    Assert.That(item.name, Is.Not.Null);

                    string name = item.name;
                    Assert.That(name, Is.EqualTo("John"));

                    Assert.That(item.address, Is.Not.Null);

                    dynamic address = item.address;

                    string street = address.street;
                    string city = address.city;

                    Assert.That(street, Is.EqualTo("123 Main St"));
                    Assert.That(city, Is.EqualTo("Anytown"));

                    Assert.That(address.doors, Is.Not.Null);

                    List<string> doors = ((JArray)address.doors).Select(x => x.Value<string>()).ToList();

                    Assert.That(doors[0], Is.EqualTo("Front"));
                    Assert.That(doors[1], Is.EqualTo("Back"));

                    Assert.That(item.scores, Is.Not.Null);
                    List<int> scores = ((JArray)item.scores).Select(x => x.Value<int>()).ToList();

                    Assert.That(scores[0], Is.EqualTo(1));
                    Assert.That(scores[1], Is.EqualTo(2));
                    Assert.That(scores[2], Is.EqualTo(3));
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
        [NonParallelizable]
        public async Task MultiPartDataFormFilesCanBeRetrieved(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

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
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("application/json"));
                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JObject.Parse(responseContent);

                    Assert.That(responseData.files, Is.Not.Null);

                    List<dynamic> files = ((JArray)responseData.files).Select(x => (dynamic)x).ToList();

                    Assert.That(files.Count, Is.EqualTo(1));

                    Assert.That((string)files[0].Name, Is.EqualTo("testfile.txt"));
                    Assert.That(files[0].Content, Is.Not.Null);

                    string commandFileContent = Encoding.UTF8.GetString((byte[])files[0].Content);
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
        [NonParallelizable]
        public async Task MultiPartDataFormMultipleFilesCanBeRetrieved(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

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
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("application/json"));

                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JObject.Parse(responseContent);

                    Assert.That(responseData.files, Is.Not.Null);

                    List<dynamic> files = ((JArray)responseData.files).Select(x => (dynamic)x).ToList();

                    Assert.That(files.Count, Is.EqualTo(2));

                    Assert.That((string)files[0].Name, Is.EqualTo("testfile.txt"));
                    Assert.That(files[0].Content, Is.Not.Null);

                    string commandFileContent = Encoding.UTF8.GetString((byte[])files[0].Content);
                    string originalFileContent = Encoding.UTF8.GetString(File.ReadAllBytes(filePath));
                    Assert.That(commandFileContent, Is.EqualTo(originalFileContent));

                    Assert.That((string)files[1].Name, Is.EqualTo("testfile2.txt"));
                    Assert.That(files[1].Content, Is.Not.Null);

                    commandFileContent = Encoding.UTF8.GetString((byte[])files[1].Content);
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
        [NonParallelizable]
        public async Task MultiPartDataFormJsonCanBeRetrieved(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

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
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("application/json"));

                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JObject.Parse(responseContent);

                    Assert.That(responseData, Is.Not.Null);

                    Assert.That(responseData.items, Is.Not.Null);
                    dynamic item = responseData.items;

                    Assert.That(item.id, Is.Not.Null);

                    string id = item.id;
                    Assert.That(id, Is.EqualTo("99"));

                    Assert.That(item.name, Is.Not.Null);

                    string name = item.name;
                    Assert.That(name, Is.EqualTo("John"));

                    Assert.That(item.address, Is.Not.Null);

                    dynamic address = item.address;
                    
                    string street = address.street;
                    string city = address.city;

                    Assert.That(street, Is.EqualTo("123 Main St"));
                    Assert.That(city, Is.EqualTo("Anytown"));

                    Assert.That(address.doors, Is.Not.Null);

                    List<string> doors = ((JArray)address.doors).Select(x => x.Value<string>()).ToList();

                    Assert.That(doors[0], Is.EqualTo("Front"));
                    Assert.That(doors[1], Is.EqualTo("Back"));

                    Assert.That(item.scores, Is.Not.Null);
                    List<int> scores = ((JArray)item.scores).Select(x => x.Value<int>()).ToList();

                    Assert.That(scores[0], Is.EqualTo(1));
                    Assert.That(scores[1], Is.EqualTo(2));
                    Assert.That(scores[2], Is.EqualTo(3));
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
        [NonParallelizable]
        public async Task MultiPartDataFormFilesAndJsonCanBeRetrievedAtSameTime(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

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
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("application/json"));

                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JObject.Parse(responseContent);

                    Assert.That(responseData, Is.Not.Null);

                    Assert.That(responseData.items, Is.Not.Null);
                    dynamic item = responseData.items;

                    Assert.That(item.id, Is.Not.Null);

                    string id = item.id;
                    Assert.That(id, Is.EqualTo("99"));

                    Assert.That(item.name, Is.Not.Null);

                    string name = item.name;
                    Assert.That(name, Is.EqualTo("John"));

                    Assert.That(item.address, Is.Not.Null);

                    dynamic address = item.address;

                    string street = address.street;
                    string city = address.city;

                    Assert.That(street, Is.EqualTo("123 Main St"));
                    Assert.That(city, Is.EqualTo("Anytown"));

                    Assert.That(address.doors, Is.Not.Null);

                    List<string> doors = ((JArray)address.doors).Select(x => x.Value<string>()).ToList();

                    Assert.That(doors[0], Is.EqualTo("Front"));
                    Assert.That(doors[1], Is.EqualTo("Back"));

                    Assert.That(item.scores, Is.Not.Null);
                    List<int> scores = ((JArray)item.scores).Select(x => x.Value<int>()).ToList();

                    Assert.That(scores[0], Is.EqualTo(1));
                    Assert.That(scores[1], Is.EqualTo(2));
                    Assert.That(scores[2], Is.EqualTo(3));

                    Assert.That(responseData.files, Is.Not.Null);

                    List<dynamic> files = ((JArray)responseData.files).Select(x => (dynamic)x).ToList();

                    Assert.That((string)files[0].Name, Is.EqualTo("testfile.txt"));
                    Assert.That(files[0].Content, Is.Not.Null);

                    string commandFileContent = Encoding.UTF8.GetString((byte[])files[0].Content);
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
        [NonParallelizable]
        public async Task MultiPartDataFormFieldsCanBeRetrievedAtSameTime(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

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
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("application/json"));

                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JObject.Parse(responseContent);

                    Assert.That(responseData, Is.Not.Null);

                    Assert.That(responseData.items, Is.Not.Null);
                    dynamic item = responseData.items;

                    Assert.That(item.age, Is.Not.Null);

                    string age = item.age;
                    Assert.That(age, Is.EqualTo("99"));

                    Assert.That(item.name, Is.Not.Null);

                    string name = item.name;
                    Assert.That(name, Is.EqualTo("John"));
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
        [NonParallelizable]
        public async Task MultiPartDataFormComplexFieldsCanBeRetrievedAtSameTime(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

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
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("application/json"));

                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JObject.Parse(responseContent);

                    Assert.That(responseData, Is.Not.Null);

                    Assert.That(responseData.items, Is.Not.Null);
                    dynamic item = responseData.items;

                    Assert.That(item.id, Is.Not.Null);

                    string id = item.id;
                    Assert.That(id, Is.EqualTo("99"));

                    Assert.That(item.name, Is.Not.Null);

                    string name = item.name;
                    Assert.That(name, Is.EqualTo("John"));

                    Assert.That(item.address, Is.Not.Null);

                    dynamic address = item.address;

                    string street = address.street;
                    string city = address.city;

                    Assert.That(street, Is.EqualTo("123 Main St"));
                    Assert.That(city, Is.EqualTo("Anytown"));

                    Assert.That(address.doors, Is.Not.Null);

                    List<string> doors = ((JArray)address.doors).Select(x => x.Value<string>()).ToList();

                    Assert.That(doors[0], Is.EqualTo("Front"));
                    Assert.That(doors[1], Is.EqualTo("Back"));

                    List<dynamic> persons = ((JArray)item.persons).Select(x => (dynamic)x).ToList();

                    Assert.That(persons.Count, Is.EqualTo(2));

                    Assert.That((string)persons[0].name, Is.EqualTo("Doe"));
                    Assert.That((string)persons[1].name, Is.EqualTo("Joe"));

                    Assert.That(item.__array__, Is.Not.Null);

                    List<string> array = ((JArray)item.__array__).Select(x => x.Value<string>()).ToList();
                    Assert.That(array.Count, Is.EqualTo(2));

                    Assert.That((string)array[0], Is.EqualTo("Doe"));
                    Assert.That((string)array[1], Is.EqualTo("Joe"));
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
        [NonParallelizable]
        public async Task MultiPartDataFormCanCombineJsonFilesAndFieldsAtSameTime(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

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
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("application/json"));

                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JObject.Parse(responseContent);

                    Assert.That(responseData, Is.Not.Null);

                    Assert.That(responseData.items, Is.Not.Null);
                    dynamic item = responseData.items;

                    Assert.That(item.id, Is.Not.Null);

                    string id = item.id;
                    Assert.That(id, Is.EqualTo("99"));

                    Assert.That(item.name, Is.Not.Null);

                    string name = item.name;
                    Assert.That(name, Is.EqualTo("John"));

                    Assert.That(item.address, Is.Not.Null);

                    dynamic address = item.address;

                    string street = address.street;
                    string city = address.city;

                    Assert.That(street, Is.EqualTo("123 Main St"));
                    Assert.That(city, Is.EqualTo("Anytown"));

                    Assert.That(address.doors, Is.Not.Null);

                    List<string> doors = ((JArray)address.doors).Select(x => x.Value<string>()).ToList();

                    Assert.That(doors[0], Is.EqualTo("Front"));
                    Assert.That(doors[1], Is.EqualTo("Back"));

                    Assert.That(item.scores, Is.Not.Null);
                    List<int> scores = ((JArray)item.scores).Select(x => x.Value<int>()).ToList();

                    Assert.That(scores[0], Is.EqualTo(1));
                    Assert.That(scores[1], Is.EqualTo(2));
                    Assert.That(scores[2], Is.EqualTo(3));

                    List<dynamic> persons = ((JArray)item.persons).Select(x => (dynamic)x).ToList();

                    Assert.That(persons.Count, Is.EqualTo(2));

                    Assert.That((string)persons[0].name, Is.EqualTo("Doe"));
                    Assert.That((string)persons[1].name, Is.EqualTo("Joe"));

                    Assert.That(item.__array__, Is.Not.Null);

                    List<string> array = ((JArray)item.__array__).Select(x => x.Value<string>()).ToList();
                    Assert.That(array.Count, Is.EqualTo(2));

                    Assert.That((string)array[0], Is.EqualTo("Doe"));
                    Assert.That((string)array[1], Is.EqualTo("Joe"));

                    Assert.That(item.age, Is.Not.Null);
                    Assert.That((int)item.age, Is.EqualTo(25));

                    Assert.That(responseData.files, Is.Not.Null);

                    List<dynamic> files = ((JArray)responseData.files).Select(x => (dynamic)x).ToList();

                    Assert.That((string)files[0].Name, Is.EqualTo("testfile.txt"));
                    Assert.That(files[0].Content, Is.Not.Null);

                    string commandFileContent = Encoding.UTF8.GetString((byte[])files[0].Content);
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
        [NonParallelizable]
        public async Task CanGetDataFromUrlEncodedForm(Enums.HttpMethod httpMethod)
        {
            // Arrange
            int port = RandomNumberGenerator.GetInt32(1000, 20000);
            IServerBuilder serverBuilder = ServerBuilderCreator.CreateTestServerBuilder(port);
            serverBuilder.AddCommand<DataFromRequestCommand>("/{id:\\d+}", httpMethod);

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
                        Content = new StringContent("test%5B0%5D.name=name&test%5B0%5D.value=value&test%5B1%5D.value=value_1&test%5B1%5D.name=name_1&UserName=test%40test.com&Password=test", Encoding.UTF8, "application/x-www-form-urlencoded")
                    };

                    HttpResponseMessage response = await client.SendAsync(request);

                    Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
                    Assert.That(response.Content.Headers.Any(x => x.Key == "Content-Type"), Is.True);
                    Assert.That(response.Content.Headers.First(x => x.Key == "Content-Type").Value.First(), Is.EqualTo("application/json"));

                    string responseContent = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JObject.Parse(responseContent);

                    Assert.That(responseData, Is.Not.Null);
                    dynamic item = responseData.items;

                    Assert.That(item.test, Is.Not.Null);
                    List<dynamic> testArray = ((JArray)item.test).Select(x => (dynamic)x).ToList();

                    Assert.That(testArray.Count, Is.EqualTo(2));

                    Assert.That((string)testArray[0].name, Is.EqualTo("name"));
                    Assert.That((string)testArray[0].value, Is.EqualTo("value"));

                    Assert.That((string)testArray[1].name, Is.EqualTo("name_1"));
                    Assert.That((string)testArray[1].value, Is.EqualTo("value_1"));

                    Assert.That((string)item.UserName, Is.Not.Null);
                    Assert.That((string)item.UserName, Is.EqualTo("test@test.com"));

                    Assert.That((string)item.Password, Is.Not.Null);
                    Assert.That((string)item.Password, Is.EqualTo("test"));
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