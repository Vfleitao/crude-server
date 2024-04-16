using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using CrudeServer.Models.Contracts;
using CrudeServer.Server.Contracts;

using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.Server
{
    public class ServerRunner : IServerRunner
    {
        private bool _isRunning;

        private readonly HttpListener _listener;

        private readonly IServerConfig _configuration;
        private readonly IServiceProvider _serviceProvider;

        public ServerRunner(IServiceProvider serviceProvider, IServerConfig Configuration)
        {
            this._serviceProvider = serviceProvider;
            this._configuration = Configuration;

            bool couldChange = ThreadPool.SetMinThreads(600, 600);
            ServicePointManager.DefaultConnectionLimit = 600;
            ServicePointManager.MaxServicePoints = 600;

            this._listener = new HttpListener() { };
        }

        public async Task Run()
        {
            if (this._serviceProvider == null)
            {
                throw new InvalidOperationException("Service provider is not set. Please call Build() before running the server.");
            }

            _isRunning = true;

            _listener.Prefixes.Add($"{this._configuration.Host}:{this._configuration.Port}/");
            _listener.Start();

            Console.WriteLine("Listening for connections on {0}:{1}", this._configuration.Host, this._configuration.Port);
            try
            {
                while (_isRunning)
                {
                    HttpListenerContext context = await _listener.GetContextAsync();

                    Task.Run(async () =>
                    {
                        try
                        {
                            using (var scope = this._serviceProvider.CreateAsyncScope())
                            {
                                IHttpRequestExecutor httpRequestExecutor = (IHttpRequestExecutor)scope.ServiceProvider.GetService(typeof(IHttpRequestExecutor));
                                await httpRequestExecutor.Execute(context);
                            }
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e.Message);
                        }
                    });
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

        public Task Stop()
        {
            _isRunning = false;

            return Task.CompletedTask;
        }
    }
}
