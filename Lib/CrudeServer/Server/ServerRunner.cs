using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using CrudeServer.Models;
using CrudeServer.Server.Contracts;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CrudeServer.Server
{
    public class ServerRunner : IServerRunner
    {
        private readonly HttpListener listener;
        private readonly IOptions<ServerConfiguration> configuration;
        private readonly IServiceProvider _serviceProvider;

        private bool isRunning;

        public ServerRunner(IServiceProvider serviceProvider, IOptions<ServerConfiguration> Configuration)
        {
            this._serviceProvider = serviceProvider;
            this.configuration = Configuration;
            ThreadPool.SetMinThreads(600, 600);
            ServicePointManager.DefaultConnectionLimit = 600;
            ServicePointManager.MaxServicePoints = 600;

            this.listener = new HttpListener() { };
        }

        public async Task Run()
        {
            isRunning = true;

            foreach (string host in this.configuration.Value.Hosts)
            {
                listener.Prefixes.Add(host);
            }

            listener.Start();

            foreach (string host in this.configuration.Value.Hosts)
            {
                Console.WriteLine("Listening for connections on {0}", host);
            }

            try
            {
                while (isRunning)
                {
                    HttpListenerContext context = await listener.GetContextAsync();

                    Task.Run(async () =>
                    {
                        try
                        {
                            using (AsyncServiceScope scope = this._serviceProvider.CreateAsyncScope())
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
                isRunning = false;
            }
            finally
            {
                listener.Close();
            }
        }

        public Task Stop()
        {
            isRunning = false;
            return Task.CompletedTask;
        }
    }
}
