using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using CrudeServer.Models.Contracts;
using CrudeServer.Server.Contracts;

namespace CrudeServer.Server
{
    public class ServerRunner : IServerRunner
    {
        private readonly HttpListener _listener;

        private readonly IServerConfig _configuration;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpRequestExecutor _httpRequestExecutor;

        public ServerRunner(
            IServiceProvider serviceProvider,
            IHttpRequestExecutor httpRequestExecutor,
            IServerConfig Configuration)
        {
            this._serviceProvider = serviceProvider;
            this._httpRequestExecutor = httpRequestExecutor;
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

            _listener.Prefixes.Add($"{this._configuration.Host}:{this._configuration.Port}/");
            _listener.Start();

            Console.WriteLine("Listening for connections on {0}:{1}", this._configuration.Host, this._configuration.Port);
            try
            {
                while (true)
                {
                    HttpListenerContext context = await _listener.GetContextAsync();
                    this._httpRequestExecutor.Execute(context);
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
