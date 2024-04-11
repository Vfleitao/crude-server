using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using CrudeServer.CommandRegistration;
using CrudeServer.CommandRegistration.Contracts;
using CrudeServer.Models;

using Microsoft.Extensions.DependencyInjection;

namespace CrudeServer.Server
{
    public class ServerBuilder
    {
        private readonly HttpListener listener;

        private int pageViews = 0;
        private int requestCount = 0;
        private ServerConfig configuration;

        private const string pageData =
            @"<!DOCTYPE>
            <html>
              <head>
                <title>HttpListener Example</title>
              </head>
              <body>
                <p>Page Views: {0}</p>
                <form method=""post"" action=""shutdown"">
                  <input type=""submit"" value=""Shutdown"" {1}>
                </form>
              </body>
            </html>";

        public IServiceCollection ServiceCollection { get; }
        public IServiceProvider ServiceProvider { get; private set; }
        public ICommandRegistry CommandRegistry { get; }

        public ServerBuilder()
        {
            listener = new HttpListener();
            ServiceCollection = new ServiceCollection();
            CommandRegistry = new CommandRegistry(ServiceCollection);

            this.configuration = new ServerConfig
            {
                Host = "http://localhost",
                Port = "8000"
            };
        }

        /// <summary>
        /// Allows the user to set the configuration of the server in case they need a different host or port
        /// </summary>
        /// <param name="config"></param>
        public void SetConfiguration(ServerConfig config)
        {
            this.configuration = config;
        }

        public void Buid()
        {
            this.ServiceProvider = ServiceCollection.BuildServiceProvider(true);
        }

        public async Task Run()
        {
            if(this.ServiceProvider == null)
            {
                throw new InvalidOperationException("Service provider is not set. Please call Build() before running the server.");
            }

            listener.Prefixes.Add($"{this.configuration.Host}:{this.configuration.Port}/");
            listener.Start();

            Console.WriteLine("Listening for connections on {0}:{1}", this.configuration.Host, this.configuration.Port);

            bool runServer = true;

            while (runServer)
            {
                HttpListenerContext ctx = await listener.GetContextAsync();
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;

                Console.WriteLine("Request #: {0}", ++requestCount);
                Console.WriteLine(req.Url.ToString());
                Console.WriteLine(req.HttpMethod);
                Console.WriteLine(req.UserHostName);
                Console.WriteLine(req.UserAgent);
                Console.WriteLine();

                if ((req.HttpMethod == "POST") && (req.Url.AbsolutePath == "/shutdown"))
                {
                    Console.WriteLine("Shutdown requested");
                    runServer = false;
                }

                if (req.Url.AbsolutePath != "/favicon.ico")
                    pageViews += 1;

                string disableSubmit = !runServer ? "disabled" : "";
                byte[] data = Encoding.UTF8.GetBytes(String.Format(pageData, pageViews, disableSubmit));
                resp.ContentType = "text/html";
                resp.ContentEncoding = Encoding.UTF8;
                resp.ContentLength64 = data.LongLength;

                await resp.OutputStream.WriteAsync(data, 0, data.Length);

                resp.Close();
            }

            listener.Close();
        }
    }
}
