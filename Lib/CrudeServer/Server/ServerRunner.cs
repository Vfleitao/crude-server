using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using CrudeServer.Models.Contracts;
using CrudeServer.Server.Contracts;

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
            _isRunning = true;

            foreach (string host in this._configuration.Hosts)
            {
                _listener.Prefixes.Add(host);
            }

            _listener.Start();

            foreach (string host in this._configuration.Hosts)
            {
                Console.WriteLine("Listening for connections on {0}", host);
            }

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
                _isRunning = false;
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
