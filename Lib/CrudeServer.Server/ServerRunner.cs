using System;
using System.Net;
using System.Threading.Tasks;

using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Enums;
using CrudeServer.HttpCommands;
using CrudeServer.HttpCommands.Contract;
using CrudeServer.HttpCommands.Responses;
using CrudeServer.Models;
using CrudeServer.Server.Contracts;

namespace CrudeServer.Server
{
    public class ServerRunner : IServerRunner
    {
        private static int requestCount = 0;

        private readonly HttpListener _listener;

        private readonly ServerConfig _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly ICommandRegistry _commandRegistry;

        public ServerRunner(
            IServiceProvider serviceProvider,
            ICommandRegistry CommandRegistry,
            ServerConfig Configuration)
        {
            this._serviceProvider = serviceProvider;
            this._commandRegistry = CommandRegistry;
            this._configuration = Configuration;

            _listener = new HttpListener();
        }

        public async Task Run()
        {
            if (this._serviceProvider == null)
            {
                throw new InvalidOperationException("Service provider is not set. Please call Build() before running the server.");
            }

            _listener.Prefixes.Add($"{this._configuration.Host}:{this._configuration.Port}/");
            _listener.Start();

            Console.WriteLine("Listening for connections on {0}:{1}", this._configuration.Host, this._configuration.Port);
            try
            {
                while (true)
                {
                    HttpListenerContext context = await _listener.GetContextAsync();
                    HttpListenerRequest req = context.Request;
                    HttpListenerResponse resp = context.Response;

                    try
                    {
                        Console.WriteLine("Request #: {0}", ++requestCount);
                        Console.WriteLine(req.Url.ToString());
                        Console.WriteLine(req.HttpMethod);
                        Console.WriteLine(req.UserHostName);
                        Console.WriteLine(req.UserAgent);
                        Console.WriteLine();

                        HttpCommandRegistration commandRegistration = _commandRegistry.GetCommand(req.Url.AbsolutePath, HttpMethodExtensions.FromHttpString(req.HttpMethod.ToUpper()));
                        IHttpResponse httpResponse = null;

                        if (commandRegistration == null)
                        {
                            httpResponse = new NotFoundResponse();
                        }
                        else
                        {
                            HttpCommand command = (HttpCommand)_serviceProvider.GetService(commandRegistration.Command);
                            command.SetContext(context);
                            httpResponse = await command.Process();
                        }

                        resp.StatusCode = httpResponse.StatusCode;
                        resp.ContentType = httpResponse.ContentType;

                        await resp.OutputStream.WriteAsync(httpResponse.ResponseData, 0, httpResponse.ResponseData.Length);

                        resp.Close();
                    }
                    catch (Exception e)
                    {
                        resp.OutputStream.SetLength(0);
                        resp.StatusCode = 500;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                _listener.Close();
            }
        }
    }
}
